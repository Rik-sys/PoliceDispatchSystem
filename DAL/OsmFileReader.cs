
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
using OsmSharp.Streams;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace DAL
{
    public static class OsmFileReader
    {
        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges) LoadOsmData(
            string filePath,
            double? minLat = null,
            double? maxLat = null,
            double? minLon = null,
            double? maxLon = null)
        {
            var allNodes = new Dictionary<long, (double lat, double lon)>();
            var edges = new List<(long from, long to)>();
            var allowedHighwayTypes = new HashSet<string> {
                "residential", "primary", "secondary", "tertiary",
                "unclassified", "service", "living_street", "pedestrian",
                "footway", "path", "cycleway", "track"
            };

            // קריאת כל הצמתים מהקובץ וסינון לפי גבולות
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
                            double lat = (double)node.Latitude;
                            double lon = (double)node.Longitude;

                            // סינון צמתים לפי גבולות גיאוגרפיים (אם סופקו)
                            bool isInBounds = true;
                            if (minLat.HasValue && lat < minLat.Value) isInBounds = false;
                            if (maxLat.HasValue && lat > maxLat.Value) isInBounds = false;
                            if (minLon.HasValue && lon < minLon.Value) isInBounds = false;
                            if (maxLon.HasValue && lon > maxLon.Value) isInBounds = false;

                            if (isInBounds)
                            {
                                allNodes[node.Id.Value] = (lat, lon);
                            }
                        }
                    }
                }
            }

            // קריאה שנייה לדרכים והוספת קשתות רק אם שני הצמתים בתוך הגבולות
            using (var fileStream = File.OpenRead(filePath))
            {
                var source = new PBFOsmStreamSource(fileStream);
                foreach (var element in source)
                {
                    if (element.Type == OsmGeoType.Way)
                    {
                        var way = (Way)element;
                        if (way.Tags != null &&
                            way.Tags.TryGetValue("highway", out string highwayValue) &&
                            allowedHighwayTypes.Contains(highwayValue))
                        {
                            if (way.Nodes != null && way.Nodes.Length > 1)
                            {
                                for (int i = 0; i < way.Nodes.Length - 1; i++)
                                {
                                    long from = way.Nodes[i];
                                    long to = way.Nodes[i + 1];

                                    // הוספת קשת רק אם שני הצמתים נמצאים בתוך הגבולות
                                    if (allNodes.ContainsKey(from) && allNodes.ContainsKey(to))
                                    {
                                        edges.Add((from, to));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return (allNodes, edges);
        }
    }
}

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




