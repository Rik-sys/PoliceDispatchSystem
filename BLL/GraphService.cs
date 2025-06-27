using DAL;
using DTO;
using IBL;
using Utilities;

namespace BLL
{
    public class GraphService : IGraphService
    {
        private readonly IGraphManagerService _graphManager;

        public GraphService(IGraphManagerService graphManager)
        {
            _graphManager = graphManager;
        }

        #region New Methods - Processing OSM Files

        public object ProcessInitialOsmFile(string tempOsmPath, double? minLat, double? maxLat,
            double? minLon, double? maxLon)
        {
            try
            {
                // המרה ל־ PBF
                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

                // tupleוהכנסה ל nullable המרה ל 
                (double minLat, double maxLat, double minLon, double maxLon)? bounds = null;
                if (minLat.HasValue && maxLat.HasValue && minLon.HasValue && maxLon.HasValue)
                {
                    bounds = (minLat.Value, maxLat.Value, minLon.Value, maxLon.Value);
                }

                var graph = OsmFileReader.LoadOsmDataToGraph(pbfPath, coord =>
                {
                    return (!minLat.HasValue || coord.lat >= minLat.Value) &&
                           (!maxLat.HasValue || coord.lat <= maxLat.Value) &&
                           (!minLon.HasValue || coord.lon >= minLon.Value) &&
                           (!maxLon.HasValue || coord.lon <= maxLon.Value);
                });

                var nodesData = graph.Nodes.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (kvp.Value.Latitude, kvp.Value.Longitude)
                );

                // חישוב אילו צמתים בתחום המקורי
                var nodesInOriginalBounds = new Dictionary<long, bool>();
                foreach (var nodeId in nodesData.Keys)
                {
                    var coord = nodesData[nodeId];
                    bool inBounds =
                        (!minLat.HasValue || coord.Item1 >= minLat.Value) &&
                        (!maxLat.HasValue || coord.Item1 <= maxLat.Value) &&
                        (!minLon.HasValue || coord.Item2 >= minLon.Value) &&
                        (!maxLon.HasValue || coord.Item2 <= maxLon.Value);

                    nodesInOriginalBounds[nodeId] = inBounds;
                }

                // שמירה ב-GraphManager
                _graphManager.SetCurrentGraph(graph, nodesData, nodesInOriginalBounds, bounds);

                // יצירת תמונה
               // GraphToImageConverter.ConvertGraphToImage(graph);

                // בדיקת קשירות והחזרת תשובה
                if (graph.IsConnected())
                {
                    return new
                    {
                        IsConnected = true,
                        Message = "הגרף קשיר, ניתן להמשיך לאלגוריתם פיזור השוטרים",
                        ImagePath = "graph_image.png",
                        ComponentCount = 1,
                        NodeCount = nodesData.Count,
                        WaySegmentsCount = graph.WaySegments.Count
                    };
                }
                else
                {
                    var components = graph.GetConnectedComponents();
                    return new
                    {
                        IsConnected = false,
                        Message = $"הגרף לא קשיר - נמצאו {components.Count} רכיבים קשירים. נא לטעון קובץ עם תחום רחב יותר",
                        ComponentCount = components.Count,
                        NodeCount = nodesData.Count,
                        WaySegmentsCount = graph.WaySegments.Count
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"שגיאה בעיבוד קובץ OSM: {ex.Message}", ex);
            }
        }

        public object RepairGraphWithExtendedFile(string tempOsmPath)
        {
            try
            {
                var currentGraph = _graphManager.GetCurrentGraph();
                var currentNodes = _graphManager.GetCurrentNodes();

                if (currentGraph == null || currentNodes == null)
                    throw new InvalidOperationException("לא הועלה קובץ בסיסי קודם");

                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

                var repairedGraph = TryRepairWithExtendedFile(currentGraph, currentNodes, pbfPath);

                // עדכון הגרף המתוקן
                _graphManager.SetDisplayGraph(repairedGraph);

                // יצירת תמונה מעודכנת
               // GraphToImageConverter.ConvertGraphToImage(repairedGraph);

                var nodesInOriginalBounds = _graphManager.GetNodesInOriginalBounds();

                if (repairedGraph.IsConnected())
                {
                    return new
                    {
                        IsConnected = true,
                        Message = "בוצע חיבור חכם בין רכיבי הקשירות",
                        ImagePath = "graph_image.png",
                        NodeCount = repairedGraph.Nodes.Count,
                        OriginalNodesCount = nodesInOriginalBounds.Count(kvp => kvp.Value == true)
                    };
                }
                else
                {
                    var components = repairedGraph.GetConnectedComponents();
                    return new
                    {
                        IsConnected = false,
                        Message = $"עדיין לא הצלחנו לחבר את הגרף. נמצאו {components.Count} רכיבים קשירים.",
                        ComponentCount = components.Count,
                        NodeCount = repairedGraph.Nodes.Count
                    };
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"שגיאה בתיקון הגרף: {ex.Message}", ex);
            }
        }

        #endregion

        #region Info Methods

        public object GetAllEventGraphsInfo()
        {
            var eventGraphs = _graphManager.GetAllEventGraphs();
            return new
            {
                TotalEvents = eventGraphs.Count,
                Events = eventGraphs.Select(kvp => new
                {
                    EventId = kvp.Key,
                    CreatedAt = kvp.Value,
                    NodeCount = _graphManager.GetGraphForEvent(kvp.Key)?.Nodes.Count ?? 0
                }).ToList()
            };
        }

        public object CleanupOldEventGraphs(int maxAgeHours)
        {
            var initialCount = _graphManager.GetAllEventGraphs().Count;
            _graphManager.CleanupOldGraphs(TimeSpan.FromHours(maxAgeHours));
            var finalCount = _graphManager.GetAllEventGraphs().Count;

            return new
            {
                Message = $"נוקו {initialCount - finalCount} גרפים ישנים",
                RemainingGraphs = finalCount
            };
        }

        public object GetConnectedComponentsInfo()
        {
            var currentGraph = _graphManager.GetCurrentGraph();
            if (currentGraph == null)
                return null;

            var components = currentGraph.GetConnectedComponents();
            return new
            {
                TotalComponents = components.Count,
                ComponentSizes = components.Select(c => c.Count).ToList()
            };
        }

        public object GetNodeLocation(long nodeId)
        {
            var location = _graphManager.GetNodeLocation(nodeId);
            if (location.HasValue)
            {
                return new { lat = location.Value.lat, lon = location.Value.lon };
            }
            return null;
        }

        public object GetCurrentBounds()
        {
            var bounds = _graphManager.GetCurrentBounds();
            if (bounds.HasValue)
            {
                return new
                {
                    minLat = bounds.Value.minLat,
                    maxLat = bounds.Value.maxLat,
                    minLon = bounds.Value.minLon,
                    maxLon = bounds.Value.maxLon
                };
            }
            return null;
        }

        #endregion

        #region Original Methods (לתאימות לאחור)

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
                    double weight = GeoUtils.CalculateDistance(
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

        public Graph TryRepairWithExtendedFile(Graph disconnectedGraph, Dictionary<long, (double lat, double lon)> originalNodes, string extendedFilePath)
        {
            var components = disconnectedGraph.GetConnectedComponents();
            if (components.Count <= 1) return disconnectedGraph;

            var (fullNodes, fullEdges) = OsmFileReader.LoadOsmData(extendedFilePath);
            var allAddedEdges = new List<(long from, long to)>();
            double maxSearchDistance = 3000;

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
            //if (!newGraph.IsConnected() && components.Count > 2)
            //{
            //    return RepairGraphIteratively(newGraph, originalNodes, fullNodes, fullEdges, maxSearchDistance);
            //}
            return newGraph;
        }

        #endregion

        #region Private Helper Methods


        //תיקון איטרטיבי לוגית אין בו צורך בכלל ועדיף את הראשון
        //private Graph RepairGraphIteratively(
        //    Graph partiallyRepairedGraph,
        //    Dictionary<long, (double lat, double lon)> originalNodes,
        //    Dictionary<long, (double lat, double lon)> fullNodes,
        //    List<(long from, long to)> fullEdges,
        //    double maxSearchDistance)
        //{
        //    var components = partiallyRepairedGraph.GetConnectedComponents();
        //    if (components.Count <= 1) return partiallyRepairedGraph;

        //    var allAddedEdges = new List<(long from, long to)>();

        //    //מיון בסדר יורד (מהגדול לקטן) לפי מספר האיברים בכל HashSet
        //    components.Sort((a, b) => b.Count.CompareTo(a.Count));

        //    var mainComponent = new HashSet<long>(components[0]);

        //    for (int i = 1; i < components.Count; i++)
        //    {
        //        var currentComponent = new HashSet<long>(components[i]);
        //        var connectingPath = OsmGraphRepairer.FindConnectingPath(
        //            mainComponent,
        //            currentComponent,
        //            fullNodes,
        //            fullEdges,
        //            maxSearchDistance);

        //        if (connectingPath.Count > 0)
        //        {
        //            allAddedEdges.AddRange(connectingPath);
        //            foreach (var nodeId in currentComponent)
        //            {
        //                mainComponent.Add(nodeId);
        //            }
        //            foreach (var (from, to) in connectingPath)
        //            {
        //                if (!originalNodes.ContainsKey(from) && fullNodes.ContainsKey(from))
        //                    originalNodes[from] = fullNodes[from];
        //                if (!originalNodes.ContainsKey(to) && fullNodes.ContainsKey(to))
        //                    originalNodes[to] = fullNodes[to];
        //            }
        //        }
        //    }

        //    return BuildGraph(originalNodes, partiallyRepairedGraph.GetAllEdges().Concat(allAddedEdges).ToList());
        //}

        private Graph BuildGraph(Dictionary<long, (double lat, double lon)> nodesData,
                             List<(long from, long to)> edgesData)
        {
            var graph = new Graph();

            foreach (var (id, (lat, lon)) in nodesData)
            {
                graph.Nodes[id] = new Node { Id = id, Latitude = lat, Longitude = lon };
            }

            foreach (var (from, to) in edgesData)
            {
                if (graph.Nodes.ContainsKey(from) && graph.Nodes.ContainsKey(to))
                {
                    double weight = GeoUtils.CalculateDistance(graph.Nodes[from].Latitude, graph.Nodes[from].Longitude,
                                          graph.Nodes[to].Latitude, graph.Nodes[to].Longitude);
                    graph.AddEdge(from, to, weight);
                }
            }

            return graph;
        }

        #endregion
    }
}