using DAL;
using DTO;
using IBL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class GraphService : IGraphService
    {
        public Graph BuildGraphFromOsm(string filePath)
        {
            var (nodesData, edgesData) = OsmFileReader.LoadOsmData(filePath);
            var graph = new Graph();

            // מוסיפים צמתים
            foreach (var (id, (lat, lon)) in nodesData)
            {
                graph.Nodes[id] = new Node { Id = id, Latitude = lat, Longitude = lon };
            }

            // מוסיפים קשתות
            foreach (var (from, to) in edgesData)
            {
                graph.AddEdge(from, to);
            }

            // אם לא קשיר - נחבר רכיבים
            //if (!graph.IsConnected())
            //{
            //    MakeGraphConnected(graph);
            //}

            return graph;
        }

        private void MakeGraphConnected(Graph graph)
        {
            var components = GetConnectedComponents(graph);

            while (components.Count > 1)
            {
                var compA = components[0];
                var compB = components[1];

                Node bestA = null;
                Node bestB = null;
                double bestDistance = double.MaxValue;

                foreach (var nodeA in compA)
                {
                    foreach (var nodeB in compB)
                    {
                        double distance = GetDistance(nodeA, nodeB);
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            bestA = nodeA;
                            bestB = nodeB;
                        }
                    }
                }

                graph.AddEdge(bestA.Id, bestB.Id, bestDistance);

                // מעדכנים קומפוננטות
                components = GetConnectedComponents(graph);
            }
        }

        private List<List<Node>> GetConnectedComponents(Graph graph)
        {
            var visited = new HashSet<long>();
            var components = new List<List<Node>>();

            foreach (var node in graph.Nodes.Values)
            {
                if (!visited.Contains(node.Id))
                {
                    var component = new List<Node>();
                    DFS(node, visited, component);
                    components.Add(component);
                }
            }

            return components;
        }

        private void DFS(Node node, HashSet<long> visited, List<Node> component)
        {
            visited.Add(node.Id);
            component.Add(node);

            foreach (var edge in node.Edges)
            {
                if (!visited.Contains(edge.To.Id))
                {
                    DFS(edge.To, visited, component);
                }
            }
        }

        private double GetDistance(Node a, Node b)
        {
            double R = 6371000; // רדיוס כדוה"א במטרים
            double dLat = ToRadians(b.Latitude - a.Latitude);
            double dLon = ToRadians(b.Longitude - a.Longitude);
            double lat1 = ToRadians(a.Latitude);
            double lat2 = ToRadians(b.Latitude);

            double aVal = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                          Math.Cos(lat1) * Math.Cos(lat2) *
                          Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(aVal), Math.Sqrt(1 - aVal));
            return R * c;
        }

        private double ToRadians(double angle) => angle * Math.PI / 180.0;
    }
}
