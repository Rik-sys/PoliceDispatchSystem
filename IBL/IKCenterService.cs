using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface IKCenterService
    {
        (List<long> centers, double maxResponseTime) SolveKCenter(Graph graph, int k);

        // Add the missing method definition for DistributePolice  
        (List<long> CenterNodes, double MaxDistance) DistributePolice(Graph graph, int k, HashSet<long> allowedNodes);
    }
    //לוגיקת פיזור בתוך התחום עובדת ולוגיקת קשיר לא
    //public interface IKCenterService
    //{      
    //    (List<long> centers, double maxResponseTime) SolveKCenter(Graph graph, int k);
    //   // List<long> FindMinimumOfficersForResponseTime(Graph graph, double maxResponseTime);
    //}
}
