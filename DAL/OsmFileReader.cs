


//משולב גרף לאודר וזה עצמו אבל הגרף לא ממש מייצג את הרחובות,הוא לוקח קצה והתחלה של רחוב
using OsmSharp.Streams;
using OsmSharp;


namespace DAL
{
    public static class OsmFileReader
    {
        private static readonly HashSet<string> allowedHighwayTypes = new HashSet<string>
        {
            "residential", "primary", "secondary", "tertiary",
            "unclassified", "service", "living_street", "pedestrian",
            "footway", "path", "cycleway", "track"
        };

        /// <summary>
        /// קריאת קובץ OSM עם שמירת מידע Ways לצורך פיצול
        /// </summary>
        public static DTO.Graph LoadOsmDataToGraph(string filePath, Func<(double lat, double lon), bool> isInBounds)
        {
            var graph = new DTO.Graph();
            var allNodes = new Dictionary<long, (double lat, double lon)>();

            // שלב 1: קריאת כל הצמתים
            using (var fileStream = File.OpenRead(filePath))
            {
                var source = new PBFOsmStreamSource(fileStream);
                foreach (var element in source)
                {
                    if (element.Type == OsmGeoType.Node)
                    {
                        var node = (Node)element;
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
                            // יצירת קשתות ושמירת מידע קטעים
                            for (int i = 0; i < way.Nodes.Length - 1; i++)
                            {
                                var fromId = way.Nodes[i];
                                var toId = way.Nodes[i + 1];

                                if (allNodes.ContainsKey(fromId) && allNodes.ContainsKey(toId))
                                {
                                    var fromCoord = allNodes[fromId];
                                    var toCoord = allNodes[toId];

                                    // חישוב משקל לפי מרחק אמיתי
                                    double weight = CalculateDistanceInMeters(
                                        fromCoord.lat, fromCoord.lon,
                                        toCoord.lat, toCoord.lon
                                    );

                                    // הוספת קשת לגרף
                                    graph.AddEdge(fromId, toId, weight);

                                    // 🆕 שמירת מידע הקטע לצורך פיצול עתידי
                                    graph.AddWaySegment(
                                        way.Id.Value,
                                        fromId, toId,
                                        fromCoord, toCoord,
                                        highwayValue
                                    );
                                }
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"📊 נטען גרף עם {graph.Nodes.Count} צמתים ו-{graph.WaySegments.Count} קטעי דרך");
            return graph;
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

        private static double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000;
            double lat1Rad = lat1 * Math.PI / 180;
            double lat2Rad = lat2 * Math.PI / 180;
            double deltaLat = (lat2 - lat1) * Math.PI / 180;
            double deltaLon = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
    }
}

//    public static class OsmFileReader
//    {
//        private static readonly HashSet<string> allowedHighwayTypes = new HashSet<string>
//        {
//            "residential", "primary", "secondary", "tertiary",
//            "unclassified", "service", "living_street", "pedestrian",
//            "footway", "path", "cycleway", "track"
//        };

//        /// <summary>
//        /// קריאת קובץ OSM עם אפשרות סינון לפי delegate מותאם
//        /// </summary>
//        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges)
//            LoadOsmData(string filePath, Func<(double lat, double lon), bool> isInBounds)
//        {
//            var allNodes = new Dictionary<long, (double lat, double lon)>();
//            var edges = new List<(long from, long to)>();
//            using (var fileStream = File.OpenRead(filePath))
//            {
//                var source = new PBFOsmStreamSource(fileStream);
//                foreach (var element in source)
//                {
//                    if (element.Type == OsmGeoType.Node)
//                    {
//                        var node = (Node)element;
//                        if (node.Latitude != null && node.Longitude != null)
//                        {
//                            var coord = ((double)node.Latitude, (double)node.Longitude);
//                            if (isInBounds(coord))
//                                allNodes[node.Id.Value] = coord;
//                        }
//                    }
//                }
//                fileStream.Position = 0;
//                var waySource = new PBFOsmStreamSource(fileStream);
//                foreach (var element in waySource)
//                {
//                    if (element.Type == OsmGeoType.Way)
//                    {
//                        var way = (Way)element;
//                        if (way.Tags != null &&
//                            way.Tags.TryGetValue("highway", out string highwayValue) &&
//                            allowedHighwayTypes.Contains(highwayValue) &&
//                            way.Nodes != null && way.Nodes.Length > 1)
//                        {
//                            for (int i = 0; i < way.Nodes.Length - 1; i++)
//                            {
//                                var from = way.Nodes[i];
//                                var to = way.Nodes[i + 1];
//                                if (allNodes.ContainsKey(from) && allNodes.ContainsKey(to))
//                                    edges.Add((from, to));
//                            }
//                        }
//                    }
//                }
//            }
//            return (allNodes, edges);
//        }

//        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges)
//            LoadOsmData(
//                string filePath,
//                double? minLat = null, double? maxLat = null,
//                double? minLon = null, double? maxLon = null)
//        {
//            return LoadOsmData(filePath, coord =>
//            {
//                double lat = coord.lat;
//                double lon = coord.lon;

//                if (minLat.HasValue && lat < minLat.Value) return false;
//                if (maxLat.HasValue && lat > maxLat.Value) return false;
//                if (minLon.HasValue && lon < minLon.Value) return false;
//                if (maxLon.HasValue && lon > maxLon.Value) return false;

//                return true;
//            });
//        }
//    }
//}

