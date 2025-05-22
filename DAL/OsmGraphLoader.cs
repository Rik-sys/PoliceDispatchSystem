// תלויות:
// - OsmSharp
// - System.Collections.Generic
// - System.Linq
// - System.IO

using OsmSharp;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

//ניסוי פיזור בתחום
namespace DAL
{
    public static class OsmGraphLoader
    {
        public static (Dictionary<long, (double lat, double lon)> nodes,
                      List<(long from, long to)> edges)
            LoadGraph(string filePath, Func<(double lat, double lon), bool> isInBounds)
        {
            var allNodes = new Dictionary<long, (double lat, double lon)>();
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
                            var coord = ((double)node.Latitude, (double)node.Longitude);
                            if (isInBounds(coord))
                                allNodes[node.Id.Value] = coord;
                        }
                    }
                }

                fileStream.Position = 0;
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

  

        private static Dictionary<long, List<long>> BuildGraph(List<(long from, long to)> edges)
        {
            var graph = new Dictionary<long, List<long>>();
            foreach (var (from, to) in edges)
            {
                if (!graph.ContainsKey(from)) graph[from] = new List<long>();
                if (!graph.ContainsKey(to)) graph[to] = new List<long>();
                graph[from].Add(to);
                graph[to].Add(from);
            }
            return graph;
        }

        private static List<HashSet<long>> FindConnectedComponents(Dictionary<long, List<long>> graph)
        {
            var visited = new HashSet<long>();
            var components = new List<HashSet<long>>();

            foreach (var node in graph.Keys)
            {
                if (!visited.Contains(node))
                {
                    var stack = new Stack<long>();
                    var component = new HashSet<long>();
                    stack.Push(node);
                    while (stack.Count > 0)
                    {
                        var current = stack.Pop();
                        if (!visited.Add(current)) continue;
                        component.Add(current);
                        foreach (var neighbor in graph[current])
                        {
                            if (!visited.Contains(neighbor))
                                stack.Push(neighbor);
                        }
                    }
                    components.Add(component);
                }
            }
            return components;
        }

        private static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371e3; // Earth radius in meters
            double phi1 = lat1 * Math.PI / 180;
            double phi2 = lat2 * Math.PI / 180;
            double dPhi = (lat2 - lat1) * Math.PI / 180;
            double dLambda = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dPhi / 2) * Math.Sin(dPhi / 2) +
                       Math.Cos(phi1) * Math.Cos(phi2) *
                       Math.Sin(dLambda / 2) * Math.Sin(dLambda / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}

//לא השתמשתי
//public static void EnsureConnectivity(
//    ref Dictionary<long, (double lat, double lon)> localNodes,
//    ref List<(long from, long to)> localEdges,
//    Dictionary<long, (double lat, double lon)> fullNodes,
//    List<(long from, long to)> fullEdges,
//    double maxBridgeDistance)
//{
//    var graph = BuildGraph(localEdges);
//    var components = FindConnectedComponents(graph);
//    if (components.Count <= 1) return; // כבר קשיר

//    foreach (var fromComp in components)
//    {
//        foreach (var toComp in components)
//        {
//            if (fromComp == toComp) continue;

//            foreach (var a in fromComp)
//            {
//                foreach (var b in toComp)
//                {
//                    if (!fullNodes.ContainsKey(a) || !fullNodes.ContainsKey(b)) continue;
//                    var coordA = fullNodes[a];
//                    var coordB = fullNodes[b];
//                    double d = Haversine(coordA.lat, coordA.lon, coordB.lat, coordB.lon);
//                    if (d <= maxBridgeDistance)
//                    {
//                        localEdges.Add((a, b));
//                        if (!localNodes.ContainsKey(a))
//                            localNodes[a] = fullNodes[a];
//                        if (!localNodes.ContainsKey(b))
//                            localNodes[b] = fullNodes[b];
//                        return; // מחברים רכיב אחד בכל פעם
//                    }
//                }
//            }
//        }
//    }
//}