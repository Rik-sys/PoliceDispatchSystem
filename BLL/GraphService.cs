//using DAL;
//using DTO;
//using IBL;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BLL
//{
//    public class GraphService : IGraphService
//    {
//        public Graph BuildGraphFromOsm(string filePath)
//        {
//            var (nodesData, edgesData) = OsmFileReader.LoadOsmData(filePath);
//            var graph = new Graph();

//            // מוסיפים צמתים
//            foreach (var (id, (lat, lon)) in nodesData)
//            {
//                graph.Nodes[id] = new Node { Id = id, Latitude = lat, Longitude = lon };
//            }

//            // מוסיפים קשתות
//            foreach (var (from, to) in edgesData)
//            {
//                graph.AddEdge(from, to);
//            }

//            // אם לא קשיר - נחבר רכיבים
//            //if (!graph.IsConnected())
//            //{
//            //    MakeGraphConnected(graph);
//            //}

//            return graph;
//        }

//        private void MakeGraphConnected(Graph graph)
//        {
//            var components = GetConnectedComponents(graph);

//            while (components.Count > 1)
//            {
//                var compA = components[0];
//                var compB = components[1];

//                Node bestA = null;
//                Node bestB = null;
//                double bestDistance = double.MaxValue;

//                foreach (var nodeA in compA)
//                {
//                    foreach (var nodeB in compB)
//                    {
//                        double distance = GetDistance(nodeA, nodeB);
//                        if (distance < bestDistance)
//                        {
//                            bestDistance = distance;
//                            bestA = nodeA;
//                            bestB = nodeB;
//                        }
//                    }
//                }

//                graph.AddEdge(bestA.Id, bestB.Id, bestDistance);

//                // מעדכנים קומפוננטות
//                components = GetConnectedComponents(graph);
//            }
//        }

//        private List<List<Node>> GetConnectedComponents(Graph graph)
//        {
//            var visited = new HashSet<long>();
//            var components = new List<List<Node>>();

//            foreach (var node in graph.Nodes.Values)
//            {
//                if (!visited.Contains(node.Id))
//                {
//                    var component = new List<Node>();
//                    DFS(node, visited, component);
//                    components.Add(component);
//                }
//            }

//            return components;
//        }

//        private void DFS(Node node, HashSet<long> visited, List<Node> component)
//        {
//            visited.Add(node.Id);
//            component.Add(node);

//            foreach (var edge in node.Edges)
//            {
//                if (!visited.Contains(edge.To.Id))
//                {
//                    DFS(edge.To, visited, component);
//                }
//            }
//        }

//        private double GetDistance(Node a, Node b)
//        {
//            double R = 6371000; // רדיוס כדוה"א במטרים
//            double dLat = ToRadians(b.Latitude - a.Latitude);
//            double dLon = ToRadians(b.Longitude - a.Longitude);
//            double lat1 = ToRadians(a.Latitude);
//            double lat2 = ToRadians(b.Latitude);

//            double aVal = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
//                          Math.Cos(lat1) * Math.Cos(lat2) *
//                          Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
//            double c = 2 * Math.Atan2(Math.Sqrt(aVal), Math.Sqrt(1 - aVal));
//            return R * c;
//        }

//        private double ToRadians(double angle) => angle * Math.PI / 180.0;
//    }
//}


//האחרון
//using DAL;
//using DTO;
//using IBL;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BLL
//{
//    public class GraphService : IGraphService
//    {
//        public Graph BuildGraphFromOsm(string filePath)
//        {
//            var (nodesData, edgesData) = OsmFileReader.LoadOsmData(filePath);
//            var graph = new Graph();

//            // Add nodes
//            foreach (var (id, (lat, lon)) in nodesData)
//            {
//                graph.Nodes[id] = new Node { Id = id, Latitude = lat, Longitude = lon };
//            }

//            // Add edges
//            foreach (var (from, to) in edgesData)
//            {
//                if (graph.Nodes.ContainsKey(from) && graph.Nodes.ContainsKey(to))
//                {
//                    graph.AddEdge(from, to);
//                }
//            }

//            return graph;
//        }

//        private double GetDistance(Node a, Node b)
//        {
//            double R = 6371000; // Radius of the earth in meters
//            double dLat = ToRadians(b.Latitude - a.Latitude);
//            double dLon = ToRadians(b.Longitude - a.Longitude);
//            double lat1 = ToRadians(a.Latitude);
//            double lat2 = ToRadians(b.Latitude);

//            double aVal = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
//                          Math.Cos(lat1) * Math.Cos(lat2) *
//                          Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
//            double c = 2 * Math.Atan2(Math.Sqrt(aVal), Math.Sqrt(1 - aVal));
//            return R * c;
//        }

//        private double ToRadians(double angle) => angle * Math.PI / 180.0;
//    }
//}


//כמעט טוב
//using DAL;
//using DTO;
//using IBL;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace BLL
//{
//    public class GraphService : IGraphService
//    {
//        public Graph BuildGraphFromOsm(string filePath)
//        {
//            var (nodesData, edgesData) = OsmFileReader.LoadOsmData(filePath);
//            var graph = BuildGraph(nodesData, edgesData);
//            return graph;
//        }

//        public Graph TryRepairWithExtendedFile(Graph disconnectedGraph,
//            Dictionary<long, (double lat, double lon)> originalNodes,
//            string extendedFilePath)
//        {
//            var (fullNodes, fullEdges) = OsmFileReader.LoadOsmData(extendedFilePath);

//            // זיהוי רכיבים לא קשירים
//            var components = disconnectedGraph.GetConnectedComponents();
//            if (components.Count <= 1) return disconnectedGraph;

//            var path = OsmGraphRepairer.FindConnectingPath(
//                components[0], components[1], fullNodes, fullEdges, maxSearchDistance: 1000);

//            foreach (var (from, to) in path)
//            {
//                if (!originalNodes.ContainsKey(from))
//                    originalNodes[from] = fullNodes[from];
//                if (!originalNodes.ContainsKey(to))
//                    originalNodes[to] = fullNodes[to];
//            }

//            var allEdges = disconnectedGraph.GetAllEdges().Concat(path).ToList();
//            return BuildGraph(originalNodes, allEdges);
//        }

//        private Graph BuildGraph(Dictionary<long, (double lat, double lon)> nodesData,
//                                 List<(long from, long to)> edgesData)
//        {
//            var graph = new Graph();

//            foreach (var (id, (lat, lon)) in nodesData)
//            {
//                graph.Nodes[id] = new Node { Id = id, Latitude = lat, Longitude = lon };
//            }

//            foreach (var (from, to) in edgesData)
//            {
//                if (graph.Nodes.ContainsKey(from) && graph.Nodes.ContainsKey(to))
//                {
//                    graph.AddEdge(from, to);
//                }
//            }

//            return graph;
//        }
//    }
//}
using DAL;
using DTO;
using IBL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class GraphService : IGraphService
    {
        public Graph BuildGraphFromOsm(string filePath)
        {
            var (nodesData, edgesData) = OsmFileReader.LoadOsmData(filePath);
            var graph = BuildGraph(nodesData, edgesData);
            return graph;
        }

        public Graph TryRepairWithExtendedFile(Graph disconnectedGraph,
            Dictionary<long, (double lat, double lon)> originalNodes,
            string extendedFilePath)
        {
            // בדיקה אם הגרף כבר קשיר
            var components = disconnectedGraph.GetConnectedComponents();
            if (components.Count <= 1) return disconnectedGraph;

            // טעינת הגרף המורחב
            var (fullNodes, fullEdges) = OsmFileReader.LoadOsmData(extendedFilePath);

            // שימוש ב-OsmGraphRepairer לחיבור המרכיבים
            var allComponentPairs = new List<(int, int)>();
            for (int i = 0; i < components.Count - 1; i++)
            {
                allComponentPairs.Add((i, i + 1));
            }

            var allAddedEdges = new List<(long from, long to)>();
            double maxSearchDistance = 5000; // 5 ק"מ מקסימום חיפוש

            foreach (var (compIdxA, compIdxB) in allComponentPairs)
            {
                var componentA = new HashSet<long>(components[compIdxA]);
                var componentB = new HashSet<long>(components[compIdxB]);

                // חיפוש מסלול שמחבר בין רכיבי הקשירות
                var connectingPath = OsmGraphRepairer.FindConnectingPath(
                    componentA,
                    componentB,
                    fullNodes,
                    fullEdges,
                    maxSearchDistance);

                if (connectingPath.Count > 0)
                {
                    allAddedEdges.AddRange(connectingPath);

                    // הוספת הצמתים החדשים למילון המקורי
                    foreach (var (from, to) in connectingPath)
                    {
                        if (!originalNodes.ContainsKey(from) && fullNodes.ContainsKey(from))
                            originalNodes[from] = fullNodes[from];
                        if (!originalNodes.ContainsKey(to) && fullNodes.ContainsKey(to))
                            originalNodes[to] = fullNodes[to];
                    }
                }
            }

            // בניית גרף מחודש עם הקשתות החדשות
            var newGraph = BuildGraph(originalNodes, disconnectedGraph.GetAllEdges().Concat(allAddedEdges).ToList());
            return newGraph;
        }

        private Graph BuildGraph(Dictionary<long, (double lat, double lon)> nodesData,
                             List<(long from, long to)> edgesData)
        {
            var graph = new Graph();

            // יצירת צמתים
            foreach (var (id, (lat, lon)) in nodesData)
            {
                graph.Nodes[id] = new Node { Id = id, Latitude = lat, Longitude = lon };
            }

            // יצירת קשתות
            foreach (var (from, to) in edgesData)
            {
                if (graph.Nodes.ContainsKey(from) && graph.Nodes.ContainsKey(to))
                {
                    double weight = Haversine(graph.Nodes[from].Latitude, graph.Nodes[from].Longitude,
                                          graph.Nodes[to].Latitude, graph.Nodes[to].Longitude);
                    graph.AddEdge(from, to, weight);
                }
            }

            return graph;
        }

        private double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371e3;
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