// GraphManagerService.cs - גרסה מתוקנת לחלוטין
using DTO;
using IBL;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BLL
{
    public class GraphManagerService : IGraphManagerService
    {
        // נתוני הגרף הנוכחי
        private static Dictionary<long, (double lat, double lon)> _latestNodes = null;
        private static Graph _latestGraph = null;
        private static (double minLat, double maxLat, double minLon, double maxLon)? _latestBounds = null;
        private static Graph _displayGraph = null;
        private static Dictionary<long, bool> _nodesInOriginalBounds = new Dictionary<long, bool>();

        // מילון לשמירת גרפים לפי מזהה אירוע
        private static Dictionary<int, GraphData> _eventGraphs = new Dictionary<int, GraphData>();

        // lock objects for thread safety
        private static readonly object _currentGraphLock = new object();
        private static readonly object _eventGraphsLock = new object();

        #region Current Graph Management

        public void SetCurrentGraph(Graph graph, Dictionary<long, (double lat, double lon)> nodes,
            Dictionary<long, bool> nodesInOriginalBounds,
            (double minLat, double maxLat, double minLon, double maxLon)? bounds = null)
        {
            lock (_currentGraphLock)
            {
                _latestGraph = graph;
                _latestNodes = new Dictionary<long, (double lat, double lon)>(nodes);
                _nodesInOriginalBounds = new Dictionary<long, bool>(nodesInOriginalBounds ?? new Dictionary<long, bool>());
                _displayGraph = graph;
                _latestBounds = bounds;
            }
        }

        public Graph GetCurrentGraph()
        {
            lock (_currentGraphLock)
            {
                return _latestGraph;
            }
        }

        public Graph GetDisplayGraph()
        {
            lock (_currentGraphLock)
            {
                return _displayGraph;
            }
        }

        public Dictionary<long, (double lat, double lon)> GetCurrentNodes()
        {
            lock (_currentGraphLock)
            {
                return _latestNodes;
            }
        }

        public Dictionary<long, bool> GetNodesInOriginalBounds()
        {
            lock (_currentGraphLock)
            {
                return _nodesInOriginalBounds;
            }
        }

        public (double minLat, double maxLat, double minLon, double maxLon)? GetCurrentBounds()
        {
            lock (_currentGraphLock)
            {
                return _latestBounds;
            }
        }

        public bool HasCurrentGraph()
        {
            lock (_currentGraphLock)
            {
                return _latestGraph != null;
            }
        }

        public void SetDisplayGraph(Graph graph)
        {
            lock (_currentGraphLock)
            {
                _displayGraph = graph;

                // עדכון הצמתים שלא היו בתחום המקורי
                foreach (var nodeId in graph.Nodes.Keys)
                {
                    if (!_nodesInOriginalBounds.ContainsKey(nodeId))
                    {
                        _nodesInOriginalBounds[nodeId] = false;
                    }
                }
            }
        }

        public (double lat, double lon)? GetNodeLocation(long nodeId)
        {
            lock (_currentGraphLock)
            {
                if (_latestNodes != null && _latestNodes.TryGetValue(nodeId, out var coords))
                {
                    return coords;
                }
                return null;
            }
        }

        #endregion

        #region Event Graphs Management

        public void SaveGraphForEvent(int eventId, Graph graph, Dictionary<long, (double lat, double lon)> nodes,
            Dictionary<long, bool> nodesInBounds)
        {
            lock (_eventGraphsLock)
            {
                _eventGraphs[eventId] = new GraphData
                {
                    Graph = graph,
                    Nodes = new Dictionary<long, (double lat, double lon)>(nodes),
                    NodesInOriginalBounds = new Dictionary<long, bool>(nodesInBounds),
                    CreatedAt = DateTime.UtcNow
                };
            }
        }

        public GraphData GetGraphForEvent(int eventId)
        {
            lock (_eventGraphsLock)
            {
                return _eventGraphs.TryGetValue(eventId, out var graphData) ? graphData : null;
            }
        }

        public void RemoveGraphForEvent(int eventId)
        {
            lock (_eventGraphsLock)
            {
                _eventGraphs.Remove(eventId);
            }
        }

        public Dictionary<int, DateTime> GetAllEventGraphs()
        {
            lock (_eventGraphsLock)
            {
                return _eventGraphs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.CreatedAt);
            }
        }

        public void CleanupOldGraphs(TimeSpan maxAge)
        {
            lock (_eventGraphsLock)
            {
                var cutoffTime = DateTime.UtcNow - maxAge;
                var keysToRemove = _eventGraphs
                    .Where(kvp => kvp.Value.CreatedAt < cutoffTime)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _eventGraphs.Remove(key);
                }
            }
        }

        #endregion
    }
}