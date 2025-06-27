
//using DTO;
//using System.Collections.Generic;

//namespace IBL
//{
//    public interface IKCenterService
//    {
//        /// <summary>
//        /// פיזור שוטרים על גרף לפי אלגוריתם K-Center, עם תמיכה באזורים אסטרטגיים אם יש.
//        /// </summary>
//        /// <param name="graph">הגרף שעליו יתבצע הפיזור</param>
//        /// <param name="k">מספר השוטרים (מרכזים)</param>
//        /// <param name="allowedNodes">צמתים מותרים בתחום התחום</param>
//        /// <param name="strategicNodes">רשימת צמתים אסטרטגיים (אם קיימת)</param>
//        /// <returns>רשימת מזהי צמתים שנבחרו כמרכזים, ורדיוס מקסימלי</returns>
//        (List<long> CenterNodes, double MaxDistance) DistributePolice(
//            Graph graph,
//            int k,
//            HashSet<long> allowedNodes,
//            List<long> strategicNodes = null
//        );
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


        KCenterResultDTO DistributePoliceWithStrategic(
            Graph graph,
            Dictionary<long, (double lat, double lon)> nodes,
            Dictionary<long, bool> bounds,
            DistributeWithStrategicRequest request
        );

        KCenterResultDTO DistributePoliceStandard(
            Graph graph,
            Dictionary<long, (double lat, double lon)> nodes,
            Dictionary<long, bool> inBounds,
            int k,
            int? eventId = null
        );
    }
}
