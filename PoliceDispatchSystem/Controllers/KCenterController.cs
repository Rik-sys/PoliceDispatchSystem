
//using BLL;
//using DTO;
//using IBL;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace PoliceDispatchSystem.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class KCenterController : ControllerBase
//    {
//        private readonly IKCenterService _kCenterService;
//        private readonly IGraphManagerService _graphManager;

//        public KCenterController(IGraphManagerService graphManager, IKCenterService kCenterService)
//        {
//            _graphManager = graphManager;
//            _kCenterService = kCenterService;
//        }

//        [HttpPost("distribute-for-event/{eventId}")]
//        public ActionResult DistributePoliceForEvent(int eventId, int k)
//        {
//            var graphData = _graphManager.GetGraphForEvent(eventId);
//            if (graphData == null)
//                return BadRequest($"לא נמצא גרף עבור אירוע {eventId}");

//            return RunDistribution(graphData.Graph, graphData.Nodes, graphData.NodesInOriginalBounds, k, eventId);
//        }

//        [HttpPost("distribute")] // עבור הגרף הנוכחי
//        public ActionResult DistributePolice(int k)
//        {
//            if (!_graphManager.HasCurrentGraph())
//                return BadRequest("לא הועלה קובץ גרף");

//            var graph = _graphManager.GetCurrentGraph();
//            var nodes = _graphManager.GetCurrentNodes();
//            var bounds = _graphManager.GetNodesInOriginalBounds();

//            return RunDistribution(graph, nodes, bounds, k);
//        }

//        [HttpPost("distribute-with-strategic")]
//        public ActionResult DistributePoliceWithStrategic([FromBody] DistributeWithStrategicRequest request)
//        {
//            if (!_graphManager.HasCurrentGraph())
//                return BadRequest("לא הועלה קובץ גרף");

//            if (request.K <= 0)
//                return BadRequest("מספר השוטרים חייב להיות גדול מאפס");

//            try
//            {
//                var graph = _graphManager.GetCurrentGraph();
//                var nodes = _graphManager.GetCurrentNodes();
//                var bounds = _graphManager.GetNodesInOriginalBounds();

//                var originalNodes = bounds.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToHashSet();

//                Console.WriteLine($"🔍 מספר צמתים בתחום: {originalNodes.Count}");
//                Console.WriteLine($"🛣️  מספר קטעי דרך זמינים: {graph.WaySegments.Count}");

//                // יצירת צמתים אסטרטגיים על Ways אמיתיים
//                List<long> strategicNodeIds = new List<long>();

//                if (request.StrategicZones != null && request.StrategicZones.Any())
//                {
//                    Console.WriteLine($"🎯 יוצר {request.StrategicZones.Count} צמתים אסטרטגיים על דרכים:");

//                    foreach (var zone in request.StrategicZones)
//                    {
//                        Console.WriteLine($"\n📍 מעבד אזור: ({zone.Latitude}, {zone.Longitude})");

//                        // שימוש בפונקציה שמפצלת Ways
//                        var newStrategicNodeId = graph.CreateStrategicNodeOnWay(
//                            zone.Latitude,
//                            zone.Longitude,
//                            originalNodes
//                        );

//                        if (newStrategicNodeId != -1)
//                        {
//                            strategicNodeIds.Add(newStrategicNodeId);

//                            // עדכון המילונים הגלובליים
//                            var actualCoord = graph.Nodes[newStrategicNodeId];
//                            nodes[newStrategicNodeId] = (actualCoord.Latitude, actualCoord.Longitude);
//                            bounds[newStrategicNodeId] = true;

//                            Console.WriteLine($"✅ נוצר צומת אסטרטגי {newStrategicNodeId} על דרך אמיתית");
//                        }
//                        else
//                        {
//                            Console.WriteLine($"❌ כשל ביצירת צומת אסטרטגי - לא נמצא קטע דרך מתאים");
//                        }
//                    }

//                    strategicNodeIds = strategicNodeIds.Distinct().ToList();
//                    Console.WriteLine($"\n🎯 סה\"כ צמתים אסטרטגיים נוצרו: {strategicNodeIds.Count}");
//                }

//                // עדכון רשימת הצמתים המותרים
//                var allowedNodesForDistribution = new HashSet<long>(originalNodes);
//                foreach (var strategicId in strategicNodeIds)
//                {
//                    allowedNodesForDistribution.Add(strategicId);
//                }

//                Console.WriteLine($"📊 סה\"כ צמתים זמינים לפיזור: {allowedNodesForDistribution.Count}");

//                // פיזור עם צמתים אסטרטגיים
//                var result = _kCenterService.DistributePolice(graph, request.K, allowedNodesForDistribution, strategicNodeIds);

//                Console.WriteLine($"\n📍 האלגוריתם בחר {result.CenterNodes.Count} צמתים:");
//                foreach (var nodeId in result.CenterNodes)
//                {
//                    if (nodes.TryGetValue(nodeId, out var coord))
//                    {
//                        var isStrategic = strategicNodeIds.Contains(nodeId) ? "🎯 אסטרטגי" : "👮 רגיל";
//                        var nodeType = graph.IsStrategicNode(nodeId) ? " (על דרך)" : " (OSM מקורי)";
//                        Console.WriteLine($"   {isStrategic}: צומת {nodeId} במיקום ({coord.lat:F6}, {coord.lon:F6}){nodeType}");
//                    }
//                }

//                // בדיקה שכל הצמתים האסטרטגיים נכללו
//                var missingStrategic = strategicNodeIds.Where(id => !result.CenterNodes.Contains(id)).ToList();
//                if (missingStrategic.Any())
//                {
//                    Console.WriteLine($"❌ צמתים אסטרטגיים שלא נכללו: {string.Join(", ", missingStrategic)}");
//                    return BadRequest($"האלגוריתם לא הצליח לכלול את כל הצמתים האסטרטגיים. חסרים: {string.Join(", ", missingStrategic)}");
//                }

//                double maxDistanceInKilometers = result.MaxDistance / 1000.0;

//                var strategicCount = strategicNodeIds.Count;
//                var regularCount = result.CenterNodes.Count - strategicCount;

//                var response = new
//                {
//                    PolicePositions = result.CenterNodes.Select(nodeId => new
//                    {
//                        NodeId = nodeId,
//                        Latitude = graph.Nodes[nodeId].Latitude,
//                        Longitude = graph.Nodes[nodeId].Longitude,
//                        IsStrategic = strategicNodeIds.Contains(nodeId),
//                        IsOnRealRoad = graph.IsStrategicNode(nodeId)
//                    }).ToList(),
//                    MaxDistance = result.MaxDistance, // מטרים
//                    MaxDistanceInKilometers = maxDistanceInKilometers, // קילומטרים
//                    StrategicOfficers = strategicCount,
//                    RegularOfficers = regularCount,
//                    NodesCreatedOnRoads = strategicNodeIds.Count,
//                    Message = strategicCount > 0
//        ? $"פוזרו {request.K} שוטרים - {strategicCount} בצמתים אסטרטגיים על דרכים אמיתיות ו-{regularCount} נוספים. מרחק מקסימלי: {maxDistanceInKilometers:F2} ק\"מ."
//        : $"פוזרו {request.K} שוטרים בהצלחה. מרחק מקסימלי: {maxDistanceInKilometers:F2} ק\"מ."
//                };

//                return Ok(response);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ שגיאה בפיזור: {ex.Message}");
//                return BadRequest($"שגיאה בביצוע האלגוריתם: {ex.Message}");
//            }
//        }

//        [HttpGet("is-in-original-bounds")]
//        public ActionResult IsNodeInOriginalBounds(long nodeId)
//        {
//            var bounds = _graphManager.GetNodesInOriginalBounds();
//            if (bounds != null && bounds.TryGetValue(nodeId, out bool isInBounds))
//            {
//                return Ok(new { nodeId, isInOriginalBounds = isInBounds });
//            }
//            return NotFound($"Node ID {nodeId} not found in bounds information.");
//        }

//        [HttpGet("original-bounds-nodes")]
//        public ActionResult GetNodesInOriginalBounds()
//        {
//            var bounds = _graphManager.GetNodesInOriginalBounds();
//            if (bounds == null)
//                return BadRequest("לא נטען גרף במערכת");

//            var originalBoundsNodes = bounds
//                .Where(kvp => kvp.Value == true)
//                .Select(kvp => kvp.Key)
//                .ToList();

//            return Ok(new
//            {
//                Count = originalBoundsNodes.Count,
//                NodeIds = originalBoundsNodes
//            });
//        }

//        private ActionResult RunDistribution(Graph graph, Dictionary<long, (double lat, double lon)> nodes,
//                                      Dictionary<long, bool> inBounds, int k, int? eventId = null)
//        {
//            if (k <= 0)
//                return BadRequest("מספר השוטרים חייב להיות גדול מאפס");

//            try
//            {
//                var allowed = inBounds.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToHashSet();
//                var result = _kCenterService.DistributePolice(graph, k, allowed);

//                double maxDistanceInKilometers = result.MaxDistance / 1000.0; // המרה לקילומטרים

//                return Ok(new
//                {
//                    EventId = eventId,
//                    PolicePositions = result.CenterNodes.Select(id => new
//                    {
//                        NodeId = id,
//                        Latitude = graph.Nodes[id].Latitude,
//                        Longitude = graph.Nodes[id].Longitude
//                    }),
//                    MaxDistance = result.MaxDistance, // מטרים
//                    MaxDistanceInKilometers = maxDistanceInKilometers, // קילומטרים
//                    Message = $"פוזרו {k} שוטרים בהצלחה. מרחק מקסימלי: {maxDistanceInKilometers:F2} ק\"מ."
//                });
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה בביצוע האלגוריתם: {ex.Message}");
//            }
//        }

//        public class DistributeWithStrategicRequest
//        {
//            public int K { get; set; }
//            public List<StrategicZoneRequest> StrategicZones { get; set; } = new();
//        }

//        public class StrategicZoneRequest
//        {
//            public double Latitude { get; set; }
//            public double Longitude { get; set; }
//        }
//    }
//}
// Controllers/KCenterController.cs
using BLL;
using DTO;
using IBL;
using Microsoft.AspNetCore.Mvc;

namespace PoliceDispatchSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KCenterController : ControllerBase
    {
        private readonly IKCenterService _kCenterService;
        private readonly IGraphManagerService _graphManager;

        public KCenterController(IGraphManagerService graphManager, IKCenterService kCenterService)
        {
            _graphManager = graphManager;
            _kCenterService = kCenterService;
        }

        [HttpPost("distribute-for-event/{eventId}")]
        public ActionResult<KCenterResultDTO> DistributePoliceForEvent(int eventId, int k)
        {
            try
            {
                var graphData = _graphManager.GetGraphForEvent(eventId);
                if (graphData == null)
                    return BadRequest($"לא נמצא גרף עבור אירוע {eventId}");

                var result = _kCenterService.DistributePoliceStandard(
                    graphData.Graph,
                    graphData.Nodes,
                    graphData.NodesInOriginalBounds,
                    k,
                    eventId);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בביצוע האלגוריתם: {ex.Message}");
            }
        }

        [HttpPost("distribute")]
        public ActionResult<KCenterResultDTO> DistributePolice(int k)
        {
            try
            {
                if (!_graphManager.HasCurrentGraph())
                    return BadRequest("לא הועלה קובץ גרף");

                var graph = _graphManager.GetCurrentGraph();
                var nodes = _graphManager.GetCurrentNodes();
                var bounds = _graphManager.GetNodesInOriginalBounds();

                var result = _kCenterService.DistributePoliceStandard(graph, nodes, bounds, k);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בביצוע האלגוריתם: {ex.Message}");
            }
        }

        [HttpPost("distribute-with-strategic")]
        public ActionResult<KCenterResultDTO> DistributePoliceWithStrategic([FromBody] DistributeWithStrategicRequest request)
        {
            try
            {
                if (!_graphManager.HasCurrentGraph())
                    return BadRequest("לא הועלה קובץ גרף");

                if (request.K <= 0)
                    return BadRequest("מספר השוטרים חייב להיות גדול מאפס");

                var graph = _graphManager.GetCurrentGraph();
                var nodes = _graphManager.GetCurrentNodes();
                var bounds = _graphManager.GetNodesInOriginalBounds();

                var result = _kCenterService.DistributePoliceWithStrategic(graph, nodes, bounds, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בביצוע האלגוריתם: {ex.Message}");
            }
        }

        [HttpGet("is-in-original-bounds")]
        public ActionResult IsNodeInOriginalBounds(long nodeId)
        {
            try
            {
                var bounds = _graphManager.GetNodesInOriginalBounds();
                if (bounds != null && bounds.TryGetValue(nodeId, out bool isInBounds))
                {
                    return Ok(new { nodeId, isInOriginalBounds = isInBounds });
                }
                return NotFound($"Node ID {nodeId} not found in bounds information.");
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בבדיקת גבולות צומת: {ex.Message}");
            }
        }

        [HttpGet("original-bounds-nodes")]
        public ActionResult GetNodesInOriginalBounds()
        {
            try
            {
                var bounds = _graphManager.GetNodesInOriginalBounds();
                if (bounds == null)
                    return BadRequest("לא נטען גרף במערכת");

                var originalBoundsNodes = bounds
                    .Where(kvp => kvp.Value == true)
                    .Select(kvp => kvp.Key)
                    .ToList();

                return Ok(new
                {
                    Count = originalBoundsNodes.Count,
                    NodeIds = originalBoundsNodes
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בשליפת צמתי גבול: {ex.Message}");
            }
        }
    }
}