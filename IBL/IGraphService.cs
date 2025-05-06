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

using DTO;
using System.Collections.Generic;

namespace IBL
{
    public interface IGraphService
    {
        Graph BuildGraphFromOsm(string filePath);
        Graph TryRepairWithExtendedFile(Graph disconnectedGraph, Dictionary<long, (double lat, double lon)> originalNodes, string extendedFilePath);
    }
}