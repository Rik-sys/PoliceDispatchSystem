//using DTO;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace IBL
//{
//    public interface IGraphService
//    {
//        Graph BuildGraphFromOsm(string filePath);
//    }
//}
//using DTO;
//using System;
//using System.Collections.Generic;

//namespace IBL
//{
//    public interface IGraphService
//    {
//        // בניית גרף רגיל מקובץ OSM
//        Graph BuildGraphFromOsm(string filePath);

//        // ניסיון לתקן גרף לא קשיר באמצעות קובץ מורחב
//        Graph TryRepairWithExtendedFile(Graph disconnectedGraph,
//                                        Dictionary<long, (double lat, double lon)> originalNodes,
//                                        string extendedFilePath);
//    }
//}


//ניסיון פיזור בתחום
//using DTO;
//using System.Collections.Generic;

//namespace IBL
//{
//    public interface IGraphService
//    {
//        Graph BuildGraphFromOsm(string filePath);
//        Graph TryRepairWithExtendedFile(Graph disconnectedGraph, Dictionary<long, (double lat, double lon)> originalNodes, string extendedFilePath);
//    }
//}

//פיזור בתוך התחום עובד ולוגיקת גרף קשיר לא
//using DTO;
//using System.Collections.Generic;

//namespace IBL
//{
//    public interface IGraphService
//    {
//        // בנייה מנתוני צמתים וקשתות
//        Graph BuildGraphFromOsm(Dictionary<long, (double lat, double lon)> nodes, List<(long from, long to)> edges);

//        // בנייה מקובץ PBF
//        Graph BuildGraphFromOsm(string pbfFilePath);

//        // תיקון גרף עם קובץ PBF נוסף
//        Graph TryRepairWithExtendedFile(Graph originalGraph, Dictionary<long, (double lat, double lon)> originalNodes, string pbfFilePath);

//        // תיקון גרף עם נתוני צמתים וקשתות נוספים
//        Graph TryRepairWithExtendedFile(Graph originalGraph, Dictionary<long, (double lat, double lon)> originalNodes, Dictionary<long, (double lat, double lon)> additionalNodes, List<(long from, long to)> additionalEdges);
//    }
//}
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
