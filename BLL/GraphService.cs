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

//לוגיקה של הפיכה לגרף קשיר עובדת אבל שוטרים לא נמצאים בתוך התחום
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
//            // בדיקה אם הגרף כבר קשיר
//            var components = disconnectedGraph.GetConnectedComponents();
//            if (components.Count <= 1) return disconnectedGraph;

//            // טעינת הגרף המורחב
//            var (fullNodes, fullEdges) = OsmFileReader.LoadOsmData(extendedFilePath);

//            // שימוש ב-OsmGraphRepairer לחיבור המרכיבים
//            var allComponentPairs = new List<(int, int)>();
//            for (int i = 0; i < components.Count - 1; i++)
//            {
//                allComponentPairs.Add((i, i + 1));
//            }

//            var allAddedEdges = new List<(long from, long to)>();
//            double maxSearchDistance = 5000; // 5 ק"מ מקסימום חיפוש

//            foreach (var (compIdxA, compIdxB) in allComponentPairs)
//            {
//                var componentA = new HashSet<long>(components[compIdxA]);
//                var componentB = new HashSet<long>(components[compIdxB]);

//                // חיפוש מסלול שמחבר בין רכיבי הקשירות
//                var connectingPath = OsmGraphRepairer.FindConnectingPath(
//                    componentA,
//                    componentB,
//                    fullNodes,
//                    fullEdges,
//                    maxSearchDistance);

//                if (connectingPath.Count > 0)
//                {
//                    allAddedEdges.AddRange(connectingPath);

//                    // הוספת הצמתים החדשים למילון המקורי
//                    foreach (var (from, to) in connectingPath)
//                    {
//                        if (!originalNodes.ContainsKey(from) && fullNodes.ContainsKey(from))
//                            originalNodes[from] = fullNodes[from];
//                        if (!originalNodes.ContainsKey(to) && fullNodes.ContainsKey(to))
//                            originalNodes[to] = fullNodes[to];
//                    }
//                }
//            }

//            // בניית גרף מחודש עם הקשתות החדשות
//            var newGraph = BuildGraph(originalNodes, disconnectedGraph.GetAllEdges().Concat(allAddedEdges).ToList());
//            return newGraph;
//        }

//        private Graph BuildGraph(Dictionary<long, (double lat, double lon)> nodesData,
//                             List<(long from, long to)> edgesData)
//        {
//            var graph = new Graph();

//            // יצירת צמתים
//            foreach (var (id, (lat, lon)) in nodesData)
//            {
//                graph.Nodes[id] = new Node { Id = id, Latitude = lat, Longitude = lon };
//            }

//            // יצירת קשתות
//            foreach (var (from, to) in edgesData)
//            {
//                if (graph.Nodes.ContainsKey(from) && graph.Nodes.ContainsKey(to))
//                {
//                    double weight = Haversine(graph.Nodes[from].Latitude, graph.Nodes[from].Longitude,
//                                          graph.Nodes[to].Latitude, graph.Nodes[to].Longitude);
//                    graph.AddEdge(from, to, weight);
//                }
//            }

//            return graph;
//        }

//        private double Haversine(double lat1, double lon1, double lat2, double lon2)
//        {
//            double R = 6371e3;
//            double phi1 = lat1 * Math.PI / 180;
//            double phi2 = lat2 * Math.PI / 180;
//            double dPhi = (lat2 - lat1) * Math.PI / 180;
//            double dLambda = (lon2 - lon1) * Math.PI / 180;
//            double a = Math.Sin(dPhi / 2) * Math.Sin(dPhi / 2) +
//                       Math.Cos(phi1) * Math.Cos(phi2) *
//                       Math.Sin(dLambda / 2) * Math.Sin(dLambda / 2);
//            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
//            return R * c;
//        }
//    }
//}


//לוגיקת פיזור בתוך התחום עובדת ולוגיקת גרף קשיר לא
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
//        // מתודה חדשה שמקבלת צמתים וקשתות במקום PBF
//        public Graph BuildGraphFromOsm(
//            Dictionary<long, (double lat, double lon)> nodes,
//            List<(long from, long to)> edges)
//        {
//            var graph = new Graph();

//            // הוספת צמתים
//            foreach (var nodeKvp in nodes)
//            {
//                long nodeId = nodeKvp.Key;
//                var coordinates = nodeKvp.Value;

//                graph.AddNode(nodeId, coordinates.lat, coordinates.lon);
//            }

//            // הוספת קשתות
//            foreach (var edge in edges)
//            {
//                if (graph.Nodes.ContainsKey(edge.from) && graph.Nodes.ContainsKey(edge.to))
//                {
//                    double weight = CalculateDistance(
//                        nodes[edge.from].lat, nodes[edge.from].lon,
//                        nodes[edge.to].lat, nodes[edge.to].lon);

//                    graph.AddEdge(edge.from, edge.to, weight);
//                }
//            }

//            return graph;
//        }

//        // שיטה קיימת שמקבלת קובץ PBF
//        public Graph BuildGraphFromOsm(string pbfFilePath)
//        {
//            var (nodes, edges) = OsmFileReader.LoadOsmData(pbfFilePath);
//            return BuildGraphFromOsm(nodes, edges);
//        }

//        public Graph TryRepairWithExtendedFile(
//            Graph originalGraph,
//            Dictionary<long, (double lat, double lon)> originalNodes,
//            Dictionary<long, (double lat, double lon)> additionalNodes,
//            List<(long from, long to)> additionalEdges)
//        {
//            // מיזוג הצמתים מהגרף המקורי עם הצמתים הנוספים
//            var mergedNodes = new Dictionary<long, (double lat, double lon)>(originalNodes);
//            foreach (var nodeKvp in additionalNodes)
//            {
//                if (!mergedNodes.ContainsKey(nodeKvp.Key))
//                {
//                    mergedNodes.Add(nodeKvp.Key, nodeKvp.Value);
//                }
//            }

//            // יצירת גרף חדש שמשלב את שני הגרפים
//            var newGraph = new Graph();

//            // הוספת כל הצמתים מהמקור והחדשים
//            foreach (var nodeKvp in mergedNodes)
//            {
//                newGraph.AddNode(nodeKvp.Key, nodeKvp.Value.lat, nodeKvp.Value.lon);
//            }

//            // הוספת הקשתות מהגרף המקורי
//            foreach (var node in originalGraph.Nodes.Values)
//            {
//                foreach (var edge in node.Edges)
//                {
//                    newGraph.AddEdge(node.Id, edge.To.Id, edge.Weight);
//                }
//            }

//            // הוספת הקשתות החדשות
//            foreach (var edge in additionalEdges)
//            {
//                if (newGraph.Nodes.ContainsKey(edge.from) && newGraph.Nodes.ContainsKey(edge.to))
//                {
//                    double weight = CalculateDistance(
//                        mergedNodes[edge.from].lat, mergedNodes[edge.from].lon,
//                        mergedNodes[edge.to].lat, mergedNodes[edge.to].lon);

//                    newGraph.AddEdge(edge.from, edge.to, weight);
//                }
//            }

//            // בדיקה אם הגרף קשיר, אם לא - ננסה לחבר רכיבים קשירים
//            if (!newGraph.IsConnected())
//            {
//                ConnectComponents(newGraph, mergedNodes);
//            }

//            return newGraph;
//        }

//        // גרסה קיימת של שיטת התיקון שצריכה לעבוד עם גרסה מעודכנת 
//        public Graph TryRepairWithExtendedFile(Graph originalGraph, Dictionary<long, (double lat, double lon)> originalNodes, string pbfFilePath)
//        {
//            var (additionalNodes, additionalEdges) = OsmFileReader.LoadOsmData(pbfFilePath);
//            return TryRepairWithExtendedFile(originalGraph, originalNodes, additionalNodes, additionalEdges);
//        }

//        private void ConnectComponents(Graph graph, Dictionary<long, (double lat, double lon)> nodes)
//        {
//            var components = graph.GetConnectedComponents();
//            if (components.Count <= 1) return; // הגרף כבר קשיר

//            // מציאת המרחק המינימלי בין רכיבים קשירים ויצירת קשתות מחברות

//            for (int i = 0; i < components.Count - 1; i++)
//            {
//                var compA = components[i];
//                var compB = components[i + 1];

//                // חיפוש מסלול בגרף המורחב
//                var connectingPath = OsmGraphRepairer.FindConnectingPath(
//                    compA, compB,
//                    nodes,
//                    graph.GetAllEdges(), // הקשתות הקיימות
//                    maxSearchDistance: 10000 // לדוגמה - עד 3 ק"מ חיבור
//                );

//                foreach (var (from, to) in connectingPath)
//                {
//                    if (nodes.TryGetValue(from, out var coords1) && nodes.TryGetValue(to, out var coords2))
//                    {
//                        double weight = CalculateDistance(coords1.lat, coords1.lon, coords2.lat, coords2.lon);
//                        graph.AddEdge(from, to, weight);
//                    }
//                }
//            }

//        }

//        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
//        {

//                double R = 6371e3; // Earth radius in meters
//                double phi1 = lat1 * Math.PI / 180;
//                double phi2 = lat2 * Math.PI / 180;
//                double dPhi = (lat2 - lat1) * Math.PI / 180;
//                double dLambda = (lon2 - lon1) * Math.PI / 180;
//                double a = Math.Sin(dPhi / 2) * Math.Sin(dPhi / 2) +
//                           Math.Cos(phi1) * Math.Cos(phi2) *
//                           Math.Sin(dLambda / 2) * Math.Sin(dLambda / 2);
//                double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
//                return R * c;


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
        // שיטה לבניית גרף מנתוני OSM שכבר נטענו
        public Graph BuildGraphFromOsm(
            Dictionary<long, (double lat, double lon)> nodes,
            List<(long from, long to)> edges)
        {
            var graph = new Graph();

            // הוספת צמתים
            foreach (var nodeKvp in nodes)
            {
                long nodeId = nodeKvp.Key;
                var coordinates = nodeKvp.Value;

                graph.AddNode(nodeId, coordinates.lat, coordinates.lon);
            }

            // הוספת קשתות
            foreach (var edge in edges)
            {
                if (graph.Nodes.ContainsKey(edge.from) && graph.Nodes.ContainsKey(edge.to))
                {
                    double weight = CalculateDistance(
                        nodes[edge.from].lat, nodes[edge.from].lon,
                        nodes[edge.to].lat, nodes[edge.to].lon);

                    graph.AddEdge(edge.from, edge.to, weight);
                }
            }

            return graph;
        }

        // שיטה לבניית גרף מקובץ PBF
        public Graph BuildGraphFromOsm(string pbfFilePath,
            double? minLat = null, double? maxLat = null,
            double? minLon = null, double? maxLon = null)
        {

            var (nodes, edges) = OsmFileReader.LoadOsmData(pbfFilePath, minLat, maxLat, minLon, maxLon);
            return BuildGraphFromOsm(nodes, edges);
        }

        // שיטה לתיקון גרף לא קשיר באמצעות קובץ מורחב
        public Graph TryRepairWithExtendedFile(
            Graph disconnectedGraph,
            Dictionary<long, (double lat, double lon)> originalNodes,
            string extendedFilePath)
        {
            // בדיקה אם הגרף כבר קשיר
            var components = disconnectedGraph.GetConnectedComponents();
            if (components.Count <= 1) return disconnectedGraph;

            // טעינת הגרף המורחב - ללא מגבלות תחום כדי לקבל את כל הנתונים
            var (fullNodes, fullEdges) = OsmFileReader.LoadOsmData(extendedFilePath);



            // שימוש ב-OsmGraphRepairer לחיבור המרכיבים
            var allAddedEdges = new List<(long from, long to)>();
            double maxSearchDistance = 5000; // 5 ק"מ מקסימום חיפוש

            // עבור על כל זוגות הרכיבים ונסה לחבר אותם
            for (int i = 0; i < components.Count; i++)
            {
                for (int j = i + 1; j < components.Count; j++)
                {
                    var componentA = new HashSet<long>(components[i]);
                    var componentB = new HashSet<long>(components[j]);

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
            }

            // בניית גרף מחודש עם הקשתות החדשות
            var newGraph = BuildGraph(originalNodes, disconnectedGraph.GetAllEdges().Concat(allAddedEdges).ToList());

            // בדיקה אם הגרף אכן הפך לקשיר
            if (!newGraph.IsConnected() && components.Count > 2)
            {
                // אם עדיין לא קשיר, ננסה לחבר את הרכיבים באופן הדרגתי
                return RepairGraphIteratively(newGraph, originalNodes, fullNodes, fullEdges, maxSearchDistance);
            }

            return newGraph;
        }

        // שיטה חדשה שמנסה לחבר את הגרף באופן הדרגתי אם החיבור הישיר לא עבד
        private Graph RepairGraphIteratively(
            Graph partiallyRepairedGraph,
            Dictionary<long, (double lat, double lon)> originalNodes,
            Dictionary<long, (double lat, double lon)> fullNodes,
            List<(long from, long to)> fullEdges,
            double maxSearchDistance)
        {
            var components = partiallyRepairedGraph.GetConnectedComponents();
            if (components.Count <= 1) return partiallyRepairedGraph;

            var allAddedEdges = new List<(long from, long to)>();

            // נתחיל מהרכיב הגדול ביותר ונחבר אליו בהדרגה את הרכיבים האחרים
            components.Sort((a, b) => b.Count.CompareTo(a.Count));

            var mainComponent = new HashSet<long>(components[0]);

            for (int i = 1; i < components.Count; i++)
            {
                var currentComponent = new HashSet<long>(components[i]);

                // חיפוש מסלול שמחבר את הרכיב הנוכחי לרכיב המרכזי
                var connectingPath = OsmGraphRepairer.FindConnectingPath(
                    mainComponent,
                    currentComponent,
                    fullNodes,
                    fullEdges,
                    maxSearchDistance);

                if (connectingPath.Count > 0)
                {
                    allAddedEdges.AddRange(connectingPath);

                    // הוספת הצמתים מהרכיב הנוכחי לרכיב המרכזי
                    foreach (var nodeId in currentComponent)
                    {
                        mainComponent.Add(nodeId);
                    }

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
            return BuildGraph(originalNodes, partiallyRepairedGraph.GetAllEdges().Concat(allAddedEdges).ToList());
        }

        // גרסה נוספת שעובדת עם נתונים שכבר נטענו
        public Graph TryRepairWithExtendedFile(
            Graph originalGraph,
            Dictionary<long, (double lat, double lon)> originalNodes,
            Dictionary<long, (double lat, double lon)> additionalNodes,
            List<(long from, long to)> additionalEdges)
        {
            // מיזוג הנתונים מהגרף המורחב לתוך קובץ מלא
            var fullNodes = new Dictionary<long, (double lat, double lon)>(originalNodes);
            foreach (var node in additionalNodes)
            {
                if (!fullNodes.ContainsKey(node.Key))
                {
                    fullNodes.Add(node.Key, node.Value);
                }
            }

            var fullEdges = new List<(long from, long to)>(originalGraph.GetAllEdges());
            fullEdges.AddRange(additionalEdges);

            // בדיקה אם הגרף כבר קשיר
            var components = originalGraph.GetConnectedComponents();
            if (components.Count <= 1) return originalGraph;

            // שימוש בלוגיקה הקודמת שעבדה
            var allAddedEdges = new List<(long from, long to)>();
            double maxSearchDistance = 5000; // 5 ק"מ מקסימום חיפוש

            // עבור על כל זוגות הרכיבים ונסה לחבר אותם
            for (int i = 0; i < components.Count; i++)
            {
                for (int j = i + 1; j < components.Count; j++)
                {
                    var componentA = new HashSet<long>(components[i]);
                    var componentB = new HashSet<long>(components[j]);

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
            }

            // בניית גרף מחודש עם הקשתות החדשות
            var newGraph = BuildGraph(originalNodes, originalGraph.GetAllEdges().Concat(allAddedEdges).ToList());

            // בדיקה אם הגרף אכן הפך לקשיר
            if (!newGraph.IsConnected() && components.Count > 2)
            {
                // אם עדיין לא קשיר, ננסה לחבר את הרכיבים באופן הדרגתי
                return RepairGraphIteratively(newGraph, originalNodes, fullNodes, fullEdges, maxSearchDistance);
            }

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
                    double weight = CalculateDistance(graph.Nodes[from].Latitude, graph.Nodes[from].Longitude,
                                          graph.Nodes[to].Latitude, graph.Nodes[to].Longitude);
                    graph.AddEdge(from, to, weight);
                }
            }

            return graph;
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
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


//חישוב מרחק בין צמתים של קלוד
//const double EarthRadiusKm = 6371.0;

//var dLat = ToRadians(lat2 - lat1);
//var dLon = ToRadians(lon2 - lon1);

//lat1 = ToRadians(lat1);
//lat2 = ToRadians(lat2);

//var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
//       Math.Sin(dLon / 2) * Math.Sin(dLon / 2) *
//       Math.Cos(lat1) * Math.Cos(lat2);
//var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

//return EarthRadiusKm * c;
//private double ToRadians(double degrees)
//{
//    return degrees * Math.PI / 180.0;
//}