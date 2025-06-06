﻿
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
        public Graph BuildGraphFromOsm(
            Dictionary<long, (double lat, double lon)> nodes,
            List<(long from, long to)> edges)
        {
            var graph = new Graph();

            foreach (var nodeKvp in nodes)
            {
                long nodeId = nodeKvp.Key;
                var coordinates = nodeKvp.Value;

                graph.AddNode(nodeId, coordinates.lat, coordinates.lon);
            }

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

        public Graph BuildGraphFromOsm(string pbfFilePath,
            double? minLat = null, double? maxLat = null,
            double? minLon = null, double? maxLon = null)
        {

            var (nodes, edges) = OsmFileReader.LoadOsmData(pbfFilePath, minLat, maxLat, minLon, maxLon);
            return BuildGraphFromOsm(nodes, edges);
        }

        public Graph TryRepairWithExtendedFile(Graph disconnectedGraph,Dictionary<long, (double lat, double lon)> originalNodes,string extendedFilePath)
        {
            var components = disconnectedGraph.GetConnectedComponents();
            if (components.Count <= 1) return disconnectedGraph;
            var (fullNodes, fullEdges) = OsmFileReader.LoadOsmData(extendedFilePath);
            var allAddedEdges = new List<(long from, long to)>();
            double maxSearchDistance = 1000; 

            for (int i = 0; i < components.Count; i++)
            {
                for (int j = i + 1; j < components.Count; j++)
                {
                    var componentA = new HashSet<long>(components[i]);
                    var componentB = new HashSet<long>(components[j]);

                    var connectingPath = OsmGraphRepairer.FindConnectingPath(
                        componentA,
                        componentB,
                        fullNodes,
                        fullEdges,
                        maxSearchDistance);

                    if (connectingPath.Count > 0)
                    {
                        allAddedEdges.AddRange(connectingPath);

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
            var newGraph = BuildGraph(originalNodes, disconnectedGraph.GetAllEdges().Concat(allAddedEdges).ToList());           
            if (!newGraph.IsConnected() && components.Count > 2)
            {
                return RepairGraphIteratively(newGraph, originalNodes, fullNodes, fullEdges, maxSearchDistance);
            }
            return newGraph;
        }

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

          
            components.Sort((a, b) => b.Count.CompareTo(a.Count));

            var mainComponent = new HashSet<long>(components[0]);

            for (int i = 1; i < components.Count; i++)
            {
                var currentComponent = new HashSet<long>(components[i]);
                var connectingPath = OsmGraphRepairer.FindConnectingPath(
                    mainComponent,
                    currentComponent,
                    fullNodes,
                    fullEdges,
                    maxSearchDistance);

                if (connectingPath.Count > 0)
                {
                    allAddedEdges.AddRange(connectingPath);
                    foreach (var nodeId in currentComponent)
                    {
                        mainComponent.Add(nodeId);
                    }
                    foreach (var (from, to) in connectingPath)
                    {
                        if (!originalNodes.ContainsKey(from) && fullNodes.ContainsKey(from))
                            originalNodes[from] = fullNodes[from];
                        if (!originalNodes.ContainsKey(to) && fullNodes.ContainsKey(to))
                            originalNodes[to] = fullNodes[to];
                    }
                }
            }

            return BuildGraph(originalNodes, partiallyRepairedGraph.GetAllEdges().Concat(allAddedEdges).ToList());
        }
        public Graph TryRepairWithExtendedFile(Graph originalGraph,Dictionary<long, (double lat, double lon)> originalNodes,
            Dictionary<long, (double lat, double lon)> additionalNodes,
            List<(long from, long to)> additionalEdges)
        {
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
            var components = originalGraph.GetConnectedComponents();
            if (components.Count <= 1) return originalGraph;
            var allAddedEdges = new List<(long from, long to)>();
            double maxSearchDistance = 1000;
            for (int i = 0; i < components.Count; i++)
            {
                for (int j = i + 1; j < components.Count; j++)
                {
                    var componentA = new HashSet<long>(components[i]);
                    var componentB = new HashSet<long>(components[j]);
                    var connectingPath = OsmGraphRepairer.FindConnectingPath(
                        componentA,
                        componentB,
                        fullNodes,
                        fullEdges,
                        maxSearchDistance);
                    if (connectingPath.Count > 0)
                    {
                        allAddedEdges.AddRange(connectingPath);
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
            var newGraph = BuildGraph(originalNodes, originalGraph.GetAllEdges().Concat(allAddedEdges).ToList());
            if (!newGraph.IsConnected() && components.Count > 2)
            {
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

