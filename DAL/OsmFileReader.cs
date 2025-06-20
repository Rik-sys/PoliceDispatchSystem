
using OsmSharp.Streams;
using OsmSharp;
using OsmSharp.Tags;
using DTO;
using Utilities;
namespace DAL
{
    public static class OsmFileReader
    {
        private static readonly HashSet<string> allowedHighwayTypes = new HashSet<string>
        {
             "residential", "primary", "secondary", "tertiary",
             "unclassified", "service", "living_street", "pedestrian",
             "footway", "path", "cycleway", "track","road",
             "motorway", "motorway_link", "trunk", "trunk_link"
        };

        // (הגדרת אורך מקסימלי לקשת (במטרים 
        private const double MAX_SEGMENT_LENGTH = 30.0;

        // מונה לצמתים חדשים שנוצרו בפיצול
        private static long _nextVirtualNodeId = 200_000_000_000L;

        /// <summary>
        /// קריאת קובץ OSM עם שמירת מידע Ways לצורך פיצול ותמיכה בדרכים חד־כיווניות
        /// </summary>
        public static Graph LoadOsmDataToGraph(string filePath, Func<(double lat, double lon), bool> isInBounds)
        {
            var graph = new Graph();
            var allNodes = new Dictionary<long, (double lat, double lon)>();

            // מונים סטטיסטיים
            int onewayCount = 0;//חד כיווני
            int bidirectionalCount = 0;//דו כיווני
            int reverseOnewayCount = 0;//חד כיווני הפוך

            // שלב 1: קריאת כל הצמתים
            using (var fileStream = File.OpenRead(filePath))
            {
                var source = new PBFOsmStreamSource(fileStream);
                foreach (var element in source)
                {
                    if (element.Type == OsmGeoType.Node)
                    {
                        var node = (OsmSharp.Node)element;
                        if (node.Latitude != null && node.Longitude != null)
                        {
                            var coord = ((double)node.Latitude, (double)node.Longitude);
                            if (isInBounds(coord))
                            {
                                allNodes[node.Id.Value] = coord;
                                graph.AddNode(node.Id.Value, coord.Item1, coord.Item2);
                            }
                        }
                    }
                }
            }

            // שלב 2: קריאת Ways והוספת קשתות + מידע לפיצול
            using (var fileStream = File.OpenRead(filePath))
            {
                var waySource = new PBFOsmStreamSource(fileStream);
                foreach (var element in waySource)
                {
                    if (element.Type == OsmGeoType.Way)
                    {
                        var way = (Way)element;
                        if (way.Tags != null &&
                            way.Tags.TryGetValue("highway", out string highwayValue) &&
                            allowedHighwayTypes.Contains(highwayValue) &&
                            way.Nodes != null && way.Nodes.Length > 1)
                        {
                            //  בדיקת כיווניות הדרך
                            var onewayInfo = DetermineOnewayDirection(way.Tags);

                            // יצירת קשתות ושמירת מידע קטעים עם פיצול אוטומטי
                            for (int i = 0; i < way.Nodes.Length - 1; i++)
                            {
                                var fromId = way.Nodes[i];
                                var toId = way.Nodes[i + 1];

                                if (allNodes.ContainsKey(fromId) && allNodes.ContainsKey(toId))
                                {
                                    var fromCoord = allNodes[fromId];
                                    var toCoord = allNodes[toId];

                                    // טיפול בקשת עם פיצול אוטומטי ותמיכה בכיווניות
                                    ProcessSegmentWithSplittingAndDirectionality(
                                        graph, way.Id.Value, fromId, toId,
                                        fromCoord, toCoord, highwayValue, onewayInfo
                                    );

                                    // עדכון מונים
                                    switch (onewayInfo.Direction)
                                    {
                                        case OnewayDirection.Forward:
                                            onewayCount++;
                                            break;
                                        case OnewayDirection.Reverse:
                                            reverseOnewayCount++;
                                            break;
                                        case OnewayDirection.Bidirectional:
                                            bidirectionalCount++;
                                            break;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // הדפסת סטטיסטיקות
            Console.WriteLine($" נטען גרף עם {graph.Nodes.Count} צמתים ו-{graph.WaySegments.Count} קטעי דרך");
            Console.WriteLine($" סטטיסטיקות כיווניות:");
            Console.WriteLine($"  דו־כיווניות: {bidirectionalCount}");
            Console.WriteLine($"  חד־כיווניות (קדימה): {onewayCount}");
            Console.WriteLine($"  חד־כיווניות (אחורה): {reverseOnewayCount}");

            if (Config.AllowReverseDirection)
            {
                Console.WriteLine($"מקדם עונש תנועה הפוכה: {Config.ReverseDirectionPenalty:F1}x");
            }

            return graph;
        }

        /// <summary>
        ///מזהה את כיוון התנועה של הדרך מתוך התגיות
        /// </summary>
        private static OnewayInfo DetermineOnewayDirection(TagsCollectionBase tags)
        {
            if (tags == null)
                return new OnewayInfo { Direction = OnewayDirection.Bidirectional };

            // בדיקת תגית oneway
            if (tags.TryGetValue("oneway", out string onewayValue))
            {
                switch (onewayValue.ToLower())
                {
                    case "yes":
                    case "true":
                    case "1":
                        return new OnewayInfo
                        {
                            Direction = OnewayDirection.Forward,
                            Reason = "oneway=yes"
                        };

                    case "-1":
                    case "reverse":
                        return new OnewayInfo
                        {
                            Direction = OnewayDirection.Reverse,
                            Reason = "oneway=-1"
                        };

                    case "no":
                    case "false":
                    case "0":
                        return new OnewayInfo
                        {
                            Direction = OnewayDirection.Bidirectional,
                            Reason = "oneway=no"
                        };
                }
            }

            // בדיקת תגיות נוספות שמציינות חד־כיווניות
            if (tags.TryGetValue("highway", out string highway))
            {
                // כניסות/יציאות לכבישים מהירים לרוב חד־כיווניות
                if (highway == "motorway_link" || highway == "trunk_link")
                {
                    return new OnewayInfo
                    {
                        Direction = OnewayDirection.Forward,
                        Reason = $"highway={highway} (assumed oneway)"
                    };
                }
            }

            // בדיקת כיכר
            if (tags.TryGetValue("junction", out string junction) && junction == "roundabout")
            {
                return new OnewayInfo
                {
                    Direction = OnewayDirection.Forward,
                    Reason = "junction=roundabout"
                };
            }

            // ברירת מחדל - דו־כיוונית
            return new OnewayInfo { Direction = OnewayDirection.Bidirectional };
        }

        /// <summary>
        ///  מעבד קטע עם פיצול אוטומטי ותמיכה בכיווניות
        /// </summary>


        private static void ProcessSegmentWithSplittingAndDirectionality(
                Graph graph,               // הגרף שלנו שאליו נכניס את הקשתות
                long wayId,                    // מזהה הדרך (Way)
                long fromId, long toId,        // מזהי הצמתים בקצה הקטע
                (double lat, double lon) fromCoord, (double lat, double lon) toCoord, // קואורדינטות של הצמתים
                string highwayValue,          // סוג הדרך (highway=residential למשל)
                OnewayInfo onewayInfo         // מידע על כיווניות הדרך
        )

        {
            double totalDistance = GeoUtils.CalculateDistance(
                fromCoord.lat, fromCoord.lon,
                toCoord.lat, toCoord.lon
            );

            // אם הקטע קצר מספיק, פשוט נוסיף אותו כמו שהוא
            if (totalDistance <= MAX_SEGMENT_LENGTH)
            {
                AddEdgeBasedOnDirectionality(graph, fromId, toId, totalDistance, onewayInfo);
                graph.AddWaySegment(wayId, fromId, toId, fromCoord, toCoord, highwayValue);
                return;
            }

            // הקטע ארוך מדי - צריך לפצל אותו
            int numSegments = (int)Math.Ceiling(totalDistance / MAX_SEGMENT_LENGTH);

            if (Config.VerboseOnewayLogging)
            {
                Console.WriteLine($"✂️  מפצל קטע {onewayInfo.Direction} באורך {totalDistance:F0}m ל-{numSegments} קטעים");
            }

            var currentFromId = fromId;
            var currentFromCoord = fromCoord;

            for (int segment = 0; segment < numSegments; segment++)
            {
                long currentToId;
                (double lat, double lon) currentToCoord;

                if (segment == numSegments - 1)
                {
                    // הקטע האחרון - מסיים בצומת המקורי
                    currentToId = toId;
                    currentToCoord = toCoord;
                }
                else
                {
                    // צריך ליצור צומת ביניים
                    double ratio = (double)(segment + 1) / numSegments;
                    currentToCoord = InterpolateCoordinates(fromCoord, toCoord, ratio);
                    currentToId = _nextVirtualNodeId++;

                    // הוספת הצומת החדש לגרף
                    graph.AddNode(currentToId, currentToCoord.lat, currentToCoord.lon);
                }

                // חישוב משקל הקטע הנוכחי
                double segmentDistance = GeoUtils.CalculateDistance(
                    currentFromCoord.lat, currentFromCoord.lon,
                    currentToCoord.lat, currentToCoord.lon
                );

                // הוספת הקשת והקטע בהתאם לכיווניות
                AddEdgeBasedOnDirectionality(graph, currentFromId, currentToId, segmentDistance, onewayInfo);
                graph.AddWaySegment(wayId, currentFromId, currentToId,
                    currentFromCoord, currentToCoord, highwayValue);

                // הכנה לאיטרציה הבאה
                currentFromId = currentToId;
                currentFromCoord = currentToCoord;
            }
        }

        /// <summary>
        ///  מוסיף קשת לגרף בהתאם לכיווניות הדרך
        /// </summary>
        private static void AddEdgeBasedOnDirectionality(Graph graph, long fromId, long toId,
            double distance, OnewayInfo onewayInfo)
        {
            switch (onewayInfo.Direction)
            {
                case OnewayDirection.Bidirectional:
                    // דרך דו־כיוונית רגילה
                    graph.AddEdge(fromId, toId, distance);
                    break;

                case OnewayDirection.Forward:
                    // דרך חד־כיוונית מ־from ל־to
                    graph.AddOnewayEdge(fromId, toId, distance,Config.AllowReverseDirection);
                    break;

                case OnewayDirection.Reverse:
                    // דרך חד־כיוונית מ־to ל־from
                    graph.AddOnewayEdge(toId, fromId, distance,Config.AllowReverseDirection);
                    break;
            }
        }

        /// <summary>
        /// אינטרפולציה ליניארית בין שתי נקודות גיאוגרפיות
        /// </summary>
        private static (double lat, double lon) InterpolateCoordinates(
            (double lat, double lon) from,
            (double lat, double lon) to,
            double ratio)
        {
            double lat = from.lat + (to.lat - from.lat) * ratio;
            double lon = from.lon + (to.lon - from.lon) * ratio;
            return (lat, lon);
        }

        /// <summary>
        /// פונקציה ישנה לתאימות אחורה - מחזירה tuple במקום Graph
        /// </summary>
        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges)
            LoadOsmData(string filePath, Func<(double lat, double lon), bool> isInBounds)
        {
            var graph = LoadOsmDataToGraph(filePath, isInBounds);

            var nodes = graph.Nodes.ToDictionary(
                kvp => kvp.Key,
                kvp => (kvp.Value.Latitude, kvp.Value.Longitude)
            );

            var edges = graph.GetAllEdges();

            return (nodes, edges);
        }

        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges)
            LoadOsmData(
                string filePath,
                double? minLat = null, double? maxLat = null,
                double? minLon = null, double? maxLon = null)
        {
            return LoadOsmData(filePath, coord =>
            {
                double lat = coord.lat;
                double lon = coord.lon;

                if (minLat.HasValue && lat < minLat.Value) return false;
                if (maxLat.HasValue && lat > maxLat.Value) return false;
                if (minLon.HasValue && lon < minLon.Value) return false;
                if (maxLon.HasValue && lon > maxLon.Value) return false;

                return true;
            });
        }
    }

    /// <summary>
    ///  אנם לכיווני תנועה
    /// </summary>
    public enum OnewayDirection
    {
        Bidirectional,  // דו־כיוונית
        Forward,        // חד־כיוונית בכיוון הגדרת הצמתים
        Reverse         // חד־כיוונית בכיוון הפוך
    }

    /// <summary>
    ///  מידע על כיווניות דרך
    /// </summary>
    public class OnewayInfo
    {
        public OnewayDirection Direction { get; set; } = OnewayDirection.Bidirectional;
        public string Reason { get; set; } = "";
    }
}

