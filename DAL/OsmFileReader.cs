
//ניסיון פיזור בתחום
//using OsmSharp.Streams;
//using OsmSharp;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DAL
//{
//    public static class OsmFileReader
//    {
//        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges) LoadOsmData(string filePath)
//        {
//            var allNodes = new Dictionary<long, (double lat, double lon)>();
//            var edges = new List<(long from, long to)>();

//            var allowedHighwayTypes = new HashSet<string> {
//                "residential", "primary", "secondary", "tertiary",
//                "unclassified", "service", "living_street", "pedestrian",
//                "footway", "path", "cycleway", "track"
//            };

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
//                            allNodes[node.Id.Value] = ((double)node.Latitude, (double)node.Longitude);
//                        }
//                    }
//                    else if (element.Type == OsmGeoType.Way)
//                    {
//                        var way = (Way)element;

//                        if (way.Tags != null &&
//                            way.Tags.TryGetValue("highway", out string highwayValue) &&
//                            allowedHighwayTypes.Contains(highwayValue))
//                        {
//                            if (way.Nodes != null && way.Nodes.Length > 1)
//                            {
//                                for (int i = 0; i < way.Nodes.Length - 1; i++)
//                                {
//                                    long from = way.Nodes[i];
//                                    long to = way.Nodes[i + 1];
//                                    edges.Add((from, to));
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            // Remove nodes that are not part of the edges
//            var usedNodes = new HashSet<long>(edges.SelectMany(e => new[] { e.from, e.to }));
//            var finalNodes = allNodes.Where(kvp => usedNodes.Contains(kvp.Key)).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

//            return (finalNodes, edges);
//        }
//    }
//}

//פיזור יותר צפוף ע''י הוספת נקודות ביניים
//using OsmSharp.Streams;
//using OsmSharp;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;

//namespace DAL
//{
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
//        /// כולל שיפורים לכיסוי מלא יותר של הרחובות
//        /// </summary>
//        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges)
//            LoadOsmData(string filePath, Func<(double lat, double lon), bool> isInBounds, double maxSegmentLengthMeters = 50)
//        {
//            // מילון לשמירת כל הנקודות
//            var allNodes = new Dictionary<long, (double lat, double lon)>();

//            // רשימה זמנית לשמירת כל הדרכים
//            var tempWays = new List<(string highwayType, long[] nodeIds)>();

//            // קריאת הקובץ פעם אחת בלבד
//            using (var fileStream = File.OpenRead(filePath))
//            {
//                var source = new PBFOsmStreamSource(fileStream);
//                foreach (var element in source)
//                {
//                    // קריאת נקודות
//                    if (element.Type == OsmGeoType.Node)
//                    {
//                        var node = (Node)element;
//                        if (node.Latitude != null && node.Longitude != null)
//                        {
//                            var coord = ((double)node.Latitude, (double)node.Longitude);
//                            allNodes[node.Id.Value] = coord;
//                        }
//                    }
//                    // קריאת דרכים
//                    else if (element.Type == OsmGeoType.Way)
//                    {
//                        var way = (Way)element;
//                        if (way.Tags != null &&
//                            way.Tags.TryGetValue("highway", out string highwayValue) &&
//                            allowedHighwayTypes.Contains(highwayValue) &&
//                            way.Nodes != null && way.Nodes.Length > 1)
//                        {
//                            tempWays.Add((highwayValue, way.Nodes));
//                        }
//                    }
//                }
//            }

//            // סינון נקודות רק לאלו שבתוך התחום
//            var nodesInBounds = new Dictionary<long, (double lat, double lon)>();
//            foreach (var node in allNodes)
//            {
//                if (isInBounds(node.Value))
//                {
//                    nodesInBounds[node.Key] = node.Value;
//                }
//            }

//            // יצירת קשתות ונקודות ביניים
//            var edges = new List<(long from, long to)>();
//            var intermediateNodes = new Dictionary<long, (double lat, double lon)>();
//            long nextId = -1; // ID שלילי לנקודות חדשות

//            foreach (var way in tempWays)
//            {
//                bool hasNodeInBounds = way.nodeIds.Any(nodeId => nodesInBounds.ContainsKey(nodeId));

//                // בדיקה אם לפחות נקודה אחת בדרך נמצאת בתחום
//                if (hasNodeInBounds)
//                {
//                    for (int i = 0; i < way.nodeIds.Length - 1; i++)
//                    {
//                        var fromId = way.nodeIds[i];
//                        var toId = way.nodeIds[i + 1];

//                        // אם שתי הנקודות נמצאות במפה (לא בהכרח בתחום)
//                        if (allNodes.TryGetValue(fromId, out var fromCoord) &&
//                            allNodes.TryGetValue(toId, out var toCoord))
//                        {
//                            // אם לפחות אחת מהנקודות בתחום, הוסף את הקשת
//                            bool fromInBounds = nodesInBounds.ContainsKey(fromId);
//                            bool toInBounds = nodesInBounds.ContainsKey(toId);

//                            if (fromInBounds || toInBounds)
//                            {
//                                // אם הנקודה לא בתחום, הוסף אותה כדי לאפשר קישוריות
//                                if (!fromInBounds)
//                                {
//                                    nodesInBounds[fromId] = fromCoord;
//                                }
//                                if (!toInBounds)
//                                {
//                                    nodesInBounds[toId] = toCoord;
//                                }

//                                // הוסף את הקשת לרשימת הקשתות
//                                edges.Add((fromId, toId));

//                                // חישוב המרחק בין הנקודות
//                                double distance = CalculateDistance(fromCoord, toCoord);

//                                // אם המרחק גדול מהמקסימום, הוסף נקודות ביניים
//                                if (distance > maxSegmentLengthMeters)
//                                {
//                                    int segments = (int)Math.Ceiling(distance / maxSegmentLengthMeters);
//                                    long prevNodeId = fromId;

//                                    for (int j = 1; j < segments; j++)
//                                    {
//                                        double ratio = (double)j / segments;
//                                        double newLat = fromCoord.lat + (toCoord.lat - fromCoord.lat) * ratio;
//                                        double newLon = fromCoord.lon + (toCoord.lon - fromCoord.lon) * ratio;

//                                        // אם הנקודה החדשה בתחום, הוסף אותה
//                                        var newCoord = (newLat, newLon);
//                                        if (isInBounds(newCoord))
//                                        {
//                                            intermediateNodes[nextId] = newCoord;
//                                            edges.Add((prevNodeId, nextId));
//                                            prevNodeId = nextId;
//                                            nextId--;
//                                        }
//                                    }

//                                    // חבר את הנקודה האחרונה לנקודת היעד המקורית
//                                    if (prevNodeId != fromId)
//                                    {
//                                        edges.Add((prevNodeId, toId));
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            // הוסף את הנקודות הביניים לרשימת הנקודות הסופית
//            foreach (var node in intermediateNodes)
//            {
//                nodesInBounds[node.Key] = node.Value;
//            }

//            // בדוק אם הגרף קשיר ותקן אם צריך
//            if (!IsConnected(nodesInBounds.Keys.ToList(), edges))
//            {
//                // מצא את הרכיב הקשיר הגדול ביותר והשתמש רק בו
//                var largestComponent = FindLargestConnectedComponent(nodesInBounds.Keys.ToList(), edges);

//                // סנן את הנקודות והקשתות כך שיכללו רק את הרכיב הגדול ביותר
//                var filteredNodes = new Dictionary<long, (double lat, double lon)>();
//                foreach (var nodeId in largestComponent)
//                {
//                    filteredNodes[nodeId] = nodesInBounds[nodeId];
//                }

//                var filteredEdges = edges.Where(e =>
//                    largestComponent.Contains(e.from) && largestComponent.Contains(e.to)).ToList();

//                return (filteredNodes, filteredEdges);
//            }

//            return (nodesInBounds, edges);
//        }

//        /// <summary>
//        /// קריאת קובץ OSM עם סינון לפי גבולות גיאוגרפיים
//        /// </summary>
//        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges)
//            LoadOsmData(
//                string filePath,
//                double? minLat = null, double? maxLat = null,
//                double? minLon = null, double? maxLon = null,
//                double maxSegmentLengthMeters = 50)
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
//            }, maxSegmentLengthMeters);
//        }

//        /// <summary>
//        /// חישוב מרחק בקו אווירי בין שתי נקודות
//        /// </summary>
//        private static double CalculateDistance((double lat, double lon) point1, (double lat, double lon) point2)
//        {
//            // רדיוס כדור הארץ במטרים
//            const double earthRadius = 6371000;

//            var dLat = ToRadians(point2.lat - point1.lat);
//            var dLon = ToRadians(point2.lon - point1.lon);

//            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
//                    Math.Cos(ToRadians(point1.lat)) * Math.Cos(ToRadians(point2.lat)) *
//                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

//            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
//            return earthRadius * c;
//        }

//        /// <summary>
//        /// המרת מעלות לרדיאנים
//        /// </summary>
//        private static double ToRadians(double degrees)
//        {
//            return degrees * Math.PI / 180;
//        }

//        /// <summary>
//        /// בדיקה אם הגרף קשיר
//        /// </summary>
//        private static bool IsConnected(List<long> nodes, List<(long from, long to)> edges)
//        {
//            if (nodes.Count == 0 || edges.Count == 0)
//                return true;

//            // בניית מפת שכנים
//            var neighbors = new Dictionary<long, HashSet<long>>();
//            foreach (var nodeId in nodes)
//            {
//                neighbors[nodeId] = new HashSet<long>();
//            }

//            foreach (var edge in edges)
//            {
//                if (neighbors.ContainsKey(edge.from) && neighbors.ContainsKey(edge.to))
//                {
//                    neighbors[edge.from].Add(edge.to);
//                    neighbors[edge.to].Add(edge.from); // גרף לא מכוון
//                }
//            }

//            // הרצת BFS מהצומת הראשון
//            var visited = new HashSet<long>();
//            var queue = new Queue<long>();

//            var firstNode = nodes.FirstOrDefault();
//            queue.Enqueue(firstNode);
//            visited.Add(firstNode);

//            while (queue.Count > 0)
//            {
//                var current = queue.Dequeue();
//                foreach (var neighbor in neighbors[current])
//                {
//                    if (!visited.Contains(neighbor))
//                    {
//                        visited.Add(neighbor);
//                        queue.Enqueue(neighbor);
//                    }
//                }
//            }

//            // הגרף קשיר אם ביקרנו בכל הצמתים
//            return visited.Count == nodes.Count;
//        }

//        /// <summary>
//        /// מציאת הרכיב הקשיר הגדול ביותר בגרף
//        /// </summary>
//        private static HashSet<long> FindLargestConnectedComponent(List<long> nodes, List<(long from, long to)> edges)
//        {
//            // בניית מפת שכנים
//            var neighbors = new Dictionary<long, HashSet<long>>();
//            foreach (var nodeId in nodes)
//            {
//                neighbors[nodeId] = new HashSet<long>();
//            }

//            foreach (var edge in edges)
//            {
//                if (neighbors.ContainsKey(edge.from) && neighbors.ContainsKey(edge.to))
//                {
//                    neighbors[edge.from].Add(edge.to);
//                    neighbors[edge.to].Add(edge.from); // גרף לא מכוון
//                }
//            }

//            var visited = new HashSet<long>();
//            var largestComponent = new HashSet<long>();

//            // עבור על כל הצמתים שעוד לא ביקרנו בהם
//            foreach (var nodeId in nodes)
//            {
//                if (visited.Contains(nodeId))
//                    continue;

//                // מצא את הרכיב הקשיר הנוכחי
//                var currentComponent = new HashSet<long>();
//                var queue = new Queue<long>();

//                queue.Enqueue(nodeId);
//                visited.Add(nodeId);
//                currentComponent.Add(nodeId);

//                while (queue.Count > 0)
//                {
//                    var current = queue.Dequeue();
//                    foreach (var neighbor in neighbors[current])
//                    {
//                        if (!visited.Contains(neighbor))
//                        {
//                            visited.Add(neighbor);
//                            currentComponent.Add(neighbor);
//                            queue.Enqueue(neighbor);
//                        }
//                    }
//                }

//                // אם הרכיב הנוכחי גדול יותר, עדכן את הרכיב הגדול ביותר
//                if (currentComponent.Count > largestComponent.Count)
//                {
//                    largestComponent = currentComponent;
//                }
//            }

//            return largestComponent;
//        }
//    }
//}



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
        /// קריאת קובץ OSM עם אפשרות סינון לפי delegate מותאם
        /// </summary>
        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges)
            LoadOsmData(string filePath, Func<(double lat, double lon), bool> isInBounds)
        {
            var allNodes = new Dictionary<long, (double lat, double lon)>();
            var edges = new List<(long from, long to)>();

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
                                allNodes[node.Id.Value] = coord;
                        }
                    }
                }

                fileStream.Position = 0; // קריאה שניה
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
                            for (int i = 0; i < way.Nodes.Length - 1; i++)
                            {
                                var from = way.Nodes[i];
                                var to = way.Nodes[i + 1];
                                if (allNodes.ContainsKey(from) && allNodes.ContainsKey(to))
                                    edges.Add((from, to));
                            }
                        }
                    }
                }
            }

            return (allNodes, edges);
        }

        /// <summary>
        /// קריאת קובץ OSM עם סינון לפי גבולות גיאוגרפיים
        /// </summary>
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
}

//לפני ניסיון לשלב
//namespace DAL
//{
//    public static class OsmFileReader
//    {
//        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges) LoadOsmData(
//            string filePath,
//            double? minLat = null,
//            double? maxLat = null,
//            double? minLon = null,
//            double? maxLon = null)
//        {
//            var allNodes = new Dictionary<long, (double lat, double lon)>();
//            var edges = new List<(long from, long to)>();
//            var allowedHighwayTypes = new HashSet<string> {
//                "residential", "primary", "secondary", "tertiary",
//                "unclassified", "service", "living_street", "pedestrian",
//                "footway", "path", "cycleway", "track"
//            };

//            // קריאת כל הצמתים מהקובץ וסינון לפי גבולות
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
//                            double lat = (double)node.Latitude;
//                            double lon = (double)node.Longitude;

//                            // סינון צמתים לפי גבולות גיאוגרפיים (אם סופקו)
//                            bool isInBounds = true;
//                            if (minLat.HasValue && lat < minLat.Value) isInBounds = false;
//                            if (maxLat.HasValue && lat > maxLat.Value) isInBounds = false;
//                            if (minLon.HasValue && lon < minLon.Value) isInBounds = false;
//                            if (maxLon.HasValue && lon > maxLon.Value) isInBounds = false;

//                            if (isInBounds)
//                            {
//                                allNodes[node.Id.Value] = (lat, lon);
//                            }
//                        }
//                    }
//                }
//            }

//            // קריאה שנייה לדרכים והוספת קשתות רק אם שני הצמתים בתוך הגבולות
//            using (var fileStream = File.OpenRead(filePath))
//            {
//                var source = new PBFOsmStreamSource(fileStream);
//                foreach (var element in source)
//                {
//                    if (element.Type == OsmGeoType.Way)
//                    {
//                        var way = (Way)element;
//                        if (way.Tags != null &&
//                            way.Tags.TryGetValue("highway", out string highwayValue) &&
//                            allowedHighwayTypes.Contains(highwayValue))
//                        {
//                            if (way.Nodes != null && way.Nodes.Length > 1)
//                            {
//                                for (int i = 0; i < way.Nodes.Length - 1; i++)
//                                {
//                                    long from = way.Nodes[i];
//                                    long to = way.Nodes[i + 1];

//                                    // הוספת קשת רק אם שני הצמתים נמצאים בתוך הגבולות
//                                    if (allNodes.ContainsKey(from) && allNodes.ContainsKey(to))
//                                    {
//                                        edges.Add((from, to));
//                                    }
//                                }
//                            }
//                        }
//                    }
//                }
//            }

//            return (allNodes, edges);
//        }
//    }
//}

//using OsmSharp.Streams;
//using OsmSharp;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DAL
//{
//    public static class OsmFileReader
//    {
//public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges) LoadOsmData(string filePath)
//{
//    var allNodes = new Dictionary<long, (double lat, double lon)>();
//    var nodeUsageCount = new Dictionary<long, int>();
//    var edges = new List<(long from, long to)>();
//    var allowedHighwayTypes = new HashSet<string> {
//        "residential", "primary", "secondary", "tertiary",
//        "unclassified", "service", "living_street", "pedestrian",
//        "footway", "path", "cycleway", "track"
//    };

//    using (var fileStream = File.OpenRead(filePath))
//    {
//        var source = new PBFOsmStreamSource(fileStream);

//        foreach (var element in source)
//        {
//            if (element.Type == OsmGeoType.Node)
//            {
//                var node = (OsmSharp.Node)element;
//                if (node.Latitude != null && node.Longitude != null)
//                    allNodes[node.Id.Value] = ((double)node.Latitude, (double)node.Longitude);
//            }
//        }

//        fileStream.Position = 0; // reset to read ways now
//        var waySource = new PBFOsmStreamSource(fileStream);

//        foreach (var element in waySource)
//        {
//            if (element.Type == OsmGeoType.Way)
//            {
//                var way = (Way)element;

//                if (way.Tags != null &&
//                    way.Tags.TryGetValue("highway", out string highwayValue) &&
//                    allowedHighwayTypes.Contains(highwayValue) &&
//                    way.Nodes != null && way.Nodes.Length > 1)
//                {
//                    // count node usage
//                    foreach (var nd in way.Nodes)
//                    {
//                        if (nodeUsageCount.ContainsKey(nd))
//                            nodeUsageCount[nd]++;
//                        else
//                            nodeUsageCount[nd] = 1;
//                    }

//                    // add edges between sequential nds
//                    for (int i = 0; i < way.Nodes.Length - 1; i++)
//                    {
//                        long from = way.Nodes[i];
//                        long to = way.Nodes[i + 1];
//                        edges.Add((from, to));
//                    }
//                }
//            }
//        }
//    }

//    // filter nodes that were used (intersections and path points)
//    var usedNodes = new HashSet<long>(edges.SelectMany(e => new[] { e.from, e.to }));
//    var finalNodes = allNodes
//        .Where(kvp => usedNodes.Contains(kvp.Key))
//        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

//    return (finalNodes, edges);
//}

//    }
//    }


//public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges) LoadOsmData(string filePath)
//{
//    var nodes = new Dictionary<long, (double lat, double lon)>();
//    var edges = new List<(long from, long to)>();

//    using (var fileStream = File.OpenRead(filePath))
//    {
//        var source = new PBFOsmStreamSource(fileStream);

//        foreach (var element in source)
//        {
//            if (element.Type == OsmGeoType.Node)
//            {
//                var node = (Node)element;
//                if (node.Latitude != null && node.Longitude != null)
//                {
//                    nodes[node.Id.Value] = ((double)node.Latitude, (double)node.Longitude);
//                }
//            }
//            else if (element.Type == OsmGeoType.Way)
//            {
//                var way = (Way)element;
//                if (way.Nodes != null && way.Nodes.Length > 1)
//                {
//                    for (int i = 0; i < way.Nodes.Length - 1; i++)
//                    {
//                        edges.Add((way.Nodes[i], way.Nodes[i + 1]));
//                    }
//                }
//            }
//        }
//    }

//    return (nodes, edges);
//}




