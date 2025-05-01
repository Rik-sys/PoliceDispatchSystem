using OsmSharp.Streams;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public static class OsmFileReader
    {
        public static (Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges) LoadOsmData(string filePath)
        {
            var allNodes = new Dictionary<long, (double lat, double lon)>();
            var usedNodes = new HashSet<long>();
            var edges = new List<(long from, long to)>();

            var allowedHighwayTypes = new HashSet<string> {
        "residential", "primary", "secondary", "tertiary",
        "unclassified", "service", "living_street", "pedestrian",
        "footway", "path", "cycleway", "track"
    };

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
                            allNodes[node.Id.Value] = ((double)node.Latitude, (double)node.Longitude);
                        }
                    }
                    else if (element.Type == OsmGeoType.Way)
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

                                    edges.Add((from, to));
                                    usedNodes.Add(from);
                                    usedNodes.Add(to);
                                }
                            }
                        }
                    }
                }
            }

            var finalNodes = allNodes
                .Where(kvp => usedNodes.Contains(kvp.Key))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            return (finalNodes, edges);
        }
    }
    }


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




