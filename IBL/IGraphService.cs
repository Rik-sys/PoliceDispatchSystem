
using DTO;
using System;
using System.Collections.Generic;

namespace IBL
{
    public interface IGraphService
    {
        object ProcessInitialOsmFile(string tempOsmPath, double? minLat, double? maxLat,
                                    double? minLon, double? maxLon);
        object RepairGraphWithExtendedFile(string tempOsmPath);

        // Info methods
        object GetAllEventGraphsInfo();
        object GetConnectedComponentsInfo();
        object GetNodeLocation(long nodeId);
        object GetCurrentBounds();
        object CleanupOldEventGraphs(int maxAgeHours);

        Graph BuildGraphFromOsm(Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges);
        Graph BuildGraphFromOsm(string pbfFilePath, double? minLat = null, double? maxLat = null,
                               double? minLon = null, double? maxLon = null);
        Graph TryRepairWithExtendedFile(Graph disconnectedGraph, Dictionary<long, (double lat, double lon)> originalNodes,
                                       string extendedFilePath);
    }
}
