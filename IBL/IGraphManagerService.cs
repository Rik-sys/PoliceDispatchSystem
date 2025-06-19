using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public interface IGraphManagerService
    {
        // Current Graph Management
        void SetCurrentGraph(Graph graph, Dictionary<long, (double lat, double lon)> nodes,
                            Dictionary<long, bool> nodesInOriginalBounds,
                            (double minLat, double maxLat, double minLon, double maxLon)? bounds = null);

        Graph GetCurrentGraph();
        Graph GetDisplayGraph();
        Dictionary<long, (double lat, double lon)> GetCurrentNodes();
        Dictionary<long, bool> GetNodesInOriginalBounds();
        (double minLat, double maxLat, double minLon, double maxLon)? GetCurrentBounds();
        bool HasCurrentGraph();
        void SetDisplayGraph(Graph graph);
        (double lat, double lon)? GetNodeLocation(long nodeId);

        // Event Graphs Management
        void SaveGraphForEvent(int eventId, Graph graph, Dictionary<long, (double lat, double lon)> nodes,
                              Dictionary<long, bool> nodesInBounds);
        GraphData GetGraphForEvent(int? eventId);
        void RemoveGraphForEvent(int eventId);
        Dictionary<int, DateTime> GetAllEventGraphs();
        void CleanupOldGraphs(TimeSpan maxAge);
    }
}
