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

//לויגקת פיזור בתחום עובדת ולוגיקת גרף קשיר לא
//using DTO;
//using IBL;
//using Microsoft.AspNetCore.Mvc;

//[Route("api/[controller]")]
//[ApiController]
//public class KCenterController : ControllerBase
//{
//    private readonly IKCenterService _kCenterService;
//    private static Graph latestGraph = null;
//    private static Dictionary<long, (double lat, double lon)> latestNodes = null;

//    public KCenterController(IKCenterService kCenterService)
//    {
//        _kCenterService = kCenterService;
//    }

//    public static void SetLatestGraph(Graph graph)
//    {
//        latestGraph = graph;
//    }

//    public static void SetLatestNodes(Dictionary<long, (double lat, double lon)> nodes)
//    {
//        latestNodes = nodes;
//    }

//    [HttpGet("solve")]
//    public ActionResult SolveKCenter([FromQuery] int k)
//    {
//        if (latestGraph == null || latestNodes == null)
//            return BadRequest("לא נטען גרף עדיין. יש להעלות קובץ OSM קודם.");

//        try
//        {
//            var (centers, maxResponseTime) = _kCenterService.SolveKCenter(latestGraph, k);
//            return Ok(new
//            {
//                Message = $"נבחרו {centers.Count} מיקומים לשוטרים",
//                Centers = centers,
//                MaxResponseTimeInSeconds = maxResponseTime
//            });
//        }
//        catch (Exception ex)
//        {
//            return BadRequest($"שגיאה במהלך הריצה: {ex.Message}");
//        }
//    }
//}
using BLL;
using DTO;
using IBL;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace PoliceDispatchSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KCenterController : ControllerBase
    {
        private readonly IKCenterService _kCenterService;
        private static Graph _latestGraph;
        private static Dictionary<long, (double lat, double lon)> _latestNodes;

        // מילון שמציין אילו צמתים נמצאים בתוך התחום המקורי
        private static Dictionary<long, bool> _nodesInOriginalBounds = new Dictionary<long, bool>();

        public KCenterController(IKCenterService kCenterService)
        {
            _kCenterService = kCenterService;
        }

        public static void SetLatestGraph(Graph graph)
        {
            _latestGraph = graph;
        }

        public static void SetLatestNodes(Dictionary<long, (double lat, double lon)> nodes)
        {
            _latestNodes = nodes;
        }

        public static void SetNodesInOriginalBounds(Dictionary<long, bool> nodesInBounds)
        {
            _nodesInOriginalBounds = new Dictionary<long, bool>(nodesInBounds);
        }

        [HttpPost("distribute")]
        public ActionResult DistributePolice(int k)
        {
            if (_latestGraph == null)
                return BadRequest("לא הועלה קובץ גרף");

            if (k <= 0)
                return BadRequest("מספר השוטרים חייב להיות גדול מאפס");

            try
            {
                // סינון הצמתים כך שיכללו רק את הצמתים שבתוך התחום המקורי
                var originalNodesIds = _nodesInOriginalBounds
                    .Where(kvp => kvp.Value == true)
                    .Select(kvp => kvp.Key)
                    .ToHashSet();

                // שימוש בשירות ה-KCenter עם הפרמטר החדש שמגביל את הפיזור רק לצמתים מהתחום המקורי
                var result = _kCenterService.DistributePolice(_latestGraph, k, originalNodesIds);

                // המרת התוצאה למבנה מתאים להחזרה
                var response = new
                {
                    PolicePositions = result.CenterNodes.Select(nodeId => new
                    {
                        NodeId = nodeId,
                        Latitude = _latestGraph.Nodes[nodeId].Latitude,
                        Longitude = _latestGraph.Nodes[nodeId].Longitude
                    }).ToList(),
                    MaxDistance = result.MaxDistance,
                    Message = $"פוזרו {k} שוטרים בהצלחה. זמן תגובה מקסימלי: {(int)result.MaxDistance} מטרים."
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בביצוע האלגוריתם: {ex.Message}");
            }
        }

        [HttpGet("is-in-original-bounds")]
        public ActionResult IsNodeInOriginalBounds(long nodeId)
        {
            if (_nodesInOriginalBounds.TryGetValue(nodeId, out bool isInBounds))
            {
                return Ok(new { nodeId, isInOriginalBounds = isInBounds });
            }
            return NotFound($"Node ID {nodeId} not found in bounds information.");
        }

        [HttpGet("original-bounds-nodes")]
        public ActionResult GetNodesInOriginalBounds()
        {
            var originalBoundsNodes = _nodesInOriginalBounds
                .Where(kvp => kvp.Value == true)
                .Select(kvp => kvp.Key)
                .ToList();

            return Ok(new
            {
                Count = originalBoundsNodes.Count,
                NodeIds = originalBoundsNodes
            });
        }
    }
}
