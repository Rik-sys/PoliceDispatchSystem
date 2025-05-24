
using DTO;
using System.Collections.Generic;

namespace IBL
{
    public interface IGraphService
    {
        // בניית גרף מקובץ PBF, עם אופציה לסינון לפי תחום
        Graph BuildGraphFromOsm(string pbfFilePath, double? minLat = null, double? maxLat = null, double? minLon = null, double? maxLon = null);

        // בניית גרף מנתוני צמתים וקשתות שכבר נטענו
        Graph BuildGraphFromOsm(Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges);

        // תיקון גרף בעזרת קובץ מורחב (גרסה עם path בלבד)
        Graph TryRepairWithExtendedFile(Graph disconnectedGraph, Dictionary<long, (double lat, double lon)> originalNodes, string extendedFilePath);

        // תיקון גרף בעזרת נתונים שכבר נטענו (nodes + edges)
        Graph TryRepairWithExtendedFile(Graph originalGraph, Dictionary<long, (double lat, double lon)> originalNodes, Dictionary<long, (double lat, double lon)> additionalNodes, List<(long from, long to)> additionalEdges);
    }
}
