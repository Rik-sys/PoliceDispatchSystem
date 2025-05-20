//using DTO;
//using IBL;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;

//namespace PoliceDispatchSystem.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class KCenterController : ControllerBase
//    {
//        private readonly IKCenterService _kCenterService;
//        private static Graph latestGraph = null; // מגיע מהגרף שהומר ב-GraphController

//        public KCenterController(IKCenterService kCenterService)
//        {
//            _kCenterService = kCenterService;
//        }

//        // פונקציה ציבורית כדי ש-GraphController יוכל להעביר את הגרף
//        public static void SetLatestGraph(Graph graph)
//        {
//            latestGraph = graph;
//        }

//        /// <summary>
//        /// מפעיל את אלגוריתם k-center על הגרף האחרון שהוזן, עם מספר k נתון
//        /// </summary>
//        /// <param name="k">מספר השוטרים (מרכזים)</param>
//        /// <returns>רשימת הצמתים שנבחרו כמרכזים וזמן התגובה המקסימלי</returns>
//        [HttpGet("solve")]
//        public ActionResult SolveKCenter([FromQuery] int k)
//        {
//            if (latestGraph == null)
//                return BadRequest("לא נטען גרף עדיין. יש להעלות קובץ OSM קודם.");

//            try
//            {
//                var (centers, maxResponseTime) = _kCenterService.SolveKCenter(latestGraph, k);

//                return Ok(new
//                {
//                    Message = $"נבחרו {centers.Count} מיקומים לשוטרים",
//                    Centers = centers,
//                    MaxResponseTimeInSeconds = maxResponseTime
//                });
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה במהלך הריצה: {ex.Message}");
//            }
//        }


//    }
//}
using DTO;
using IBL;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
public class KCenterController : ControllerBase
{
    private readonly IKCenterService _kCenterService;
    private static Graph latestGraph = null;
    private static Dictionary<long, (double lat, double lon)> latestNodes = null;

    public KCenterController(IKCenterService kCenterService)
    {
        _kCenterService = kCenterService;
    }

    public static void SetLatestGraph(Graph graph)
    {
        latestGraph = graph;
    }

    public static void SetLatestNodes(Dictionary<long, (double lat, double lon)> nodes)
    {
        latestNodes = nodes;
    }

    [HttpGet("solve")]
    public ActionResult SolveKCenter([FromQuery] int k)
    {
        if (latestGraph == null || latestNodes == null)
            return BadRequest("לא נטען גרף עדיין. יש להעלות קובץ OSM קודם.");

        try
        {
            var (centers, maxResponseTime) = _kCenterService.SolveKCenter(latestGraph, k);
            return Ok(new
            {
                Message = $"נבחרו {centers.Count} מיקומים לשוטרים",
                Centers = centers,
                MaxResponseTimeInSeconds = maxResponseTime
            });
        }
        catch (Exception ex)
        {
            return BadRequest($"שגיאה במהלך הריצה: {ex.Message}");
        }
    }
}
