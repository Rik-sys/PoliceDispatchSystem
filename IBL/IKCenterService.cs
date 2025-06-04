//using DTO;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace IBL
//{
//    public interface IKCenterService
//    {
//        (List<long> centers, double maxResponseTime) SolveKCenter(Graph graph, int k);

//        // Add the missing method definition for DistributePolice  
//        (List<long> CenterNodes, double MaxDistance) DistributePolice(Graph graph, int k, HashSet<long> allowedNodes);
//    }

//}
using DTO;
using System.Collections.Generic;

namespace IBL
{
    public interface IKCenterService
    {
        /// <summary>
        /// פיזור שוטרים על גרף לפי אלגוריתם K-Center, עם תמיכה באזורים אסטרטגיים אם יש.
        /// </summary>
        /// <param name="graph">הגרף שעליו יתבצע הפיזור</param>
        /// <param name="k">מספר השוטרים (מרכזים)</param>
        /// <param name="allowedNodes">צמתים מותרים בתחום התחום</param>
        /// <param name="strategicNodes">רשימת צמתים אסטרטגיים (אם קיימת)</param>
        /// <returns>רשימת מזהי צמתים שנבחרו כמרכזים, ורדיוס מקסימלי</returns>
        (List<long> CenterNodes, double MaxDistance) DistributePolice(
            Graph graph,
            int k,
            HashSet<long> allowedNodes,
            List<long> strategicNodes = null
        );

        /// <summary>
        /// פתרון K-Center כללי (לא בהכרח על גרף מסונן), עם תמיכה באזורים אסטרטגיים
        /// </summary>
        /// <param name="graph">הגרף כולו</param>
        /// <param name="k">מספר השוטרים</param>
        /// <param name="strategicNodes">רשימת צמתים אסטרטגיים</param>
        /// <returns>מרכזים וזמן תגובה מקסימלי</returns>
        (List<long> centers, double maxResponseTime) SolveKCenter(
            Graph graph,
            int k,
            List<long> strategicNodes = null
        );
    }
}
