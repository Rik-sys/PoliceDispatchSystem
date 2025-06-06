
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
                var originalNodesIds = _nodesInOriginalBounds
                    .Where(kvp => kvp.Value == true)
                    .Select(kvp => kvp.Key)
                    .ToHashSet();

                var result = _kCenterService.DistributePolice(_latestGraph, k, originalNodesIds);

                const double averageSpeed = 13.89;
                double maxResponseTimeInSeconds = result.MaxDistance / averageSpeed;

                var response = new
                {
                    PolicePositions = result.CenterNodes.Select(nodeId => new
                    {
                        NodeId = nodeId,
                        Latitude = _latestGraph.Nodes[nodeId].Latitude,
                        Longitude = _latestGraph.Nodes[nodeId].Longitude
                    }).ToList(),
                    MaxDistance = result.MaxDistance,
                    MaxResponseTimeInSeconds = maxResponseTimeInSeconds,
                    Message = $"פוזרו {k} שוטרים בהצלחה. זמן תגובה מקסימלי: {(int)maxResponseTimeInSeconds} שניות."
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בביצוע האלגוריתם: {ex.Message}");
            }
        }

        [HttpPost("distribute-for-event/{eventId}")]
        public ActionResult DistributePoliceForEvent(int eventId, int k)
        {
            var graphData = GraphController.GetGraphForEvent(eventId);
            if (graphData == null)
                return BadRequest($"לא נמצא גרף עבור אירוע {eventId}");

            if (k <= 0)
                return BadRequest("מספר השוטרים חייב להיות גדול מאפס");

            try
            {
                var originalNodesIds = graphData.NodesInOriginalBounds
                    .Where(kvp => kvp.Value == true)
                    .Select(kvp => kvp.Key)
                    .ToHashSet();

                var result = _kCenterService.DistributePolice(graphData.Graph, k, originalNodesIds);

                const double averageSpeed = 13.89;
                double maxResponseTimeInSeconds = result.MaxDistance / averageSpeed;

                var response = new
                {
                    EventId = eventId,
                    PolicePositions = result.CenterNodes.Select(nodeId => new
                    {
                        NodeId = nodeId,
                        Latitude = graphData.Graph.Nodes[nodeId].Latitude,
                        Longitude = graphData.Graph.Nodes[nodeId].Longitude
                    }).ToList(),
                    MaxDistance = result.MaxDistance,
                    MaxResponseTimeInSeconds = maxResponseTimeInSeconds,
                    Message = $"פוזרו {k} שוטרים בהצלחה עבור אירוע {eventId}. זמן תגובה מקסימלי: {(int)maxResponseTimeInSeconds} שניות."
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
        [HttpPost("distribute-with-strategic")]
        public ActionResult DistributePoliceWithStrategic([FromBody] DistributeWithStrategicRequest request)
        {
            if (_latestGraph == null)
                return BadRequest("לא הועלה קובץ גרף");

            if (request.K <= 0)
                return BadRequest("מספר השוטרים חייב להיות גדול מאפס");

            try
            {
                var originalNodesIds = _nodesInOriginalBounds
                    .Where(kvp => kvp.Value == true)
                    .Select(kvp => kvp.Key)
                    .ToHashSet();

                Console.WriteLine($"🔍 מספר צמתים בתחום: {originalNodesIds.Count}");
                Console.WriteLine($"🛣️  מספר קטעי דרך זמינים: {_latestGraph.WaySegments.Count}");

                // **יצירת צמתים אסטרטגיים על Ways אמיתיים**
                List<long> strategicNodeIds = new List<long>();

                if (request.StrategicZones != null && request.StrategicZones.Any())
                {
                    Console.WriteLine($"🎯 יוצר {request.StrategicZones.Count} צמתים אסטרטגיים על דרכים:");

                    foreach (var zone in request.StrategicZones)
                    {
                        Console.WriteLine($"\n📍 מעבד אזור: ({zone.Latitude}, {zone.Longitude})");

                        // **שימוש בפונקציה החדשה שמפצלת Ways**
                        var newStrategicNodeId = _latestGraph.CreateStrategicNodeOnWay(
                            zone.Latitude,
                            zone.Longitude,
                            originalNodesIds
                        );

                        if (newStrategicNodeId != -1)
                        {
                            strategicNodeIds.Add(newStrategicNodeId);

                            // עדכון המילונים הגלובליים
                            var actualCoord = _latestGraph.Nodes[newStrategicNodeId];
                            _latestNodes[newStrategicNodeId] = (actualCoord.Latitude, actualCoord.Longitude);
                            _nodesInOriginalBounds[newStrategicNodeId] = true;

                            Console.WriteLine($"✅ נוצר צומת אסטרטגי {newStrategicNodeId} על דרך אמיתית");
                        }
                        else
                        {
                            Console.WriteLine($"❌ כשל ביצירת צומת אסטרטגי - לא נמצא קטע דרך מתאים");
                        }
                    }

                    strategicNodeIds = strategicNodeIds.Distinct().ToList();
                    Console.WriteLine($"\n🎯 סה\"כ צמתים אסטרטגיים נוצרו: {strategicNodeIds.Count}");
                }

                // עדכון רשימת הצמתים המותרים
                var allowedNodesForDistribution = new HashSet<long>(originalNodesIds);
                foreach (var strategicId in strategicNodeIds)
                {
                    allowedNodesForDistribution.Add(strategicId);
                }

                Console.WriteLine($"📊 סה\"כ צמתים זמינים לפיזור: {allowedNodesForDistribution.Count}");

                // פיזור עם צמתים אסטרטגיים
                var result = _kCenterService.DistributePolice(_latestGraph, request.K, allowedNodesForDistribution, strategicNodeIds);

                Console.WriteLine($"\n📍 האלגוריתם בחר {result.CenterNodes.Count} צמתים:");
                foreach (var nodeId in result.CenterNodes)
                {
                    if (_latestNodes.TryGetValue(nodeId, out var coord))
                    {
                        var isStrategic = strategicNodeIds.Contains(nodeId) ? "🎯 אסטרטגי" : "👮 רגיל";
                        var nodeType = _latestGraph.IsStrategicNode(nodeId) ? " (על דרך)" : " (OSM מקורי)";
                        Console.WriteLine($"   {isStrategic}: צומת {nodeId} במיקום ({coord.lat:F6}, {coord.lon:F6}){nodeType}");
                    }
                }

                // בדיקה שכל הצמתים האסטרטגיים נכללו
                var missingStrategic = strategicNodeIds.Where(id => !result.CenterNodes.Contains(id)).ToList();
                if (missingStrategic.Any())
                {
                    Console.WriteLine($"❌ צמתים אסטרטגיים שלא נכללו: {string.Join(", ", missingStrategic)}");
                    return BadRequest($"האלגוריתם לא הצליח לכלול את כל הצמתים האסטרטגיים. חסרים: {string.Join(", ", missingStrategic)}");
                }

                const double averageSpeed = 13.89;
                double maxResponseTimeInSeconds = result.MaxDistance / averageSpeed;

                var strategicCount = strategicNodeIds.Count;
                var regularCount = result.CenterNodes.Count - strategicCount;

                var response = new
                {
                    PolicePositions = result.CenterNodes.Select(nodeId => new
                    {
                        NodeId = nodeId,
                        Latitude = _latestGraph.Nodes[nodeId].Latitude,
                        Longitude = _latestGraph.Nodes[nodeId].Longitude,
                        IsStrategic = strategicNodeIds.Contains(nodeId),
                        IsOnRealRoad = _latestGraph.IsStrategicNode(nodeId)  // 🆕 צומת על דרך אמיתית
                    }).ToList(),
                    MaxDistance = result.MaxDistance,
                    MaxResponseTimeInSeconds = maxResponseTimeInSeconds,
                    StrategicOfficers = strategicCount,
                    RegularOfficers = regularCount,
                    NodesCreatedOnRoads = strategicNodeIds.Count,  // 🆕
                    Message = strategicCount > 0
                        ? $"פוזרו {request.K} שוטרים - {strategicCount} בצמתים אסטרטגיים על דרכים אמיתיות ו-{regularCount} נוספים. זמן תגובה מקסימלי: {(int)maxResponseTimeInSeconds} שניות."
                        : $"פוזרו {request.K} שוטרים בהצלחה. זמן תגובה מקסימלי: {(int)maxResponseTimeInSeconds} שניות."
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ שגיאה בפיזור: {ex.Message}");
                return BadRequest($"שגיאה בביצוע האלגוריתם: {ex.Message}");
            }
        }
        // הוספה ל-KCenterController.cs

        //[HttpPost("distribute-with-strategic")]
        //public ActionResult DistributePoliceWithStrategic([FromBody] DistributeWithStrategicRequest request)
        //{
        //    if (_latestGraph == null)
        //        return BadRequest("לא הועלה קובץ גרף");

        //    if (request.K <= 0)
        //        return BadRequest("מספר השוטרים חייב להיות גדול מאפס");

        //    try
        //    {
        //        var originalNodesIds = _nodesInOriginalBounds
        //            .Where(kvp => kvp.Value == true)
        //            .Select(kvp => kvp.Key)
        //            .ToHashSet();

        //        Console.WriteLine($"🔍 מספר צמתים בתחום: {originalNodesIds.Count}");

        //        // המרת אזורים אסטרטגיים לצמתים
        //        List<long> strategicNodeIds = new List<long>();

        //        if (request.StrategicZones != null && request.StrategicZones.Any())
        //        {
        //            Console.WriteLine($"🎯 מעבד {request.StrategicZones.Count} אזורים אסטרטגיים:");

        //            foreach (var zone in request.StrategicZones)
        //            {
        //                Console.WriteLine($"   אזור: ({zone.Latitude}, {zone.Longitude})");

        //                var closestNode = FindClosestNodeInBounds(zone.Latitude, zone.Longitude, originalNodesIds);

        //                if (closestNode != -1)
        //                {
        //                    strategicNodeIds.Add(closestNode);

        //                    if (_latestNodes.TryGetValue(closestNode, out var nodeCoord))
        //                    {
        //                        var distance = Math.Sqrt(
        //                            Math.Pow(nodeCoord.lat - zone.Latitude, 2) +
        //                            Math.Pow(nodeCoord.lon - zone.Longitude, 2)
        //                        );
        //                        Console.WriteLine($"   ✅ נמצא צומת {closestNode} במיקום ({nodeCoord.lat}, {nodeCoord.lon}), מרחק: {distance:F6}");
        //                    }
        //                }
        //                else
        //                {
        //                    Console.WriteLine($"   ❌ לא נמצא צומת קרוב לאזור ({zone.Latitude}, {zone.Longitude})");
        //                }
        //            }

        //            strategicNodeIds = strategicNodeIds.Distinct().ToList();
        //            Console.WriteLine($"🎯 סה\"כ צמתים אסטרטגיים: {strategicNodeIds.Count}");
        //        }

        //        // פיזור עם אזורים אסטרטגיים
        //        var result = _kCenterService.DistributePolice(_latestGraph, request.K, originalNodesIds, strategicNodeIds);

        //        Console.WriteLine($"📍 האלגוריתם בחר {result.CenterNodes.Count} צמתים:");
        //        foreach (var nodeId in result.CenterNodes)
        //        {
        //            if (_latestNodes.TryGetValue(nodeId, out var coord))
        //            {
        //                var isStrategic = strategicNodeIds.Contains(nodeId) ? "🎯 אסטרטגי" : "👮 רגיל";
        //                Console.WriteLine($"   {isStrategic}: צומת {nodeId} במיקום ({coord.lat}, {coord.lon})");
        //            }
        //        }

        //        // בדיקה שכל האזורים האסטרטגיים נכללו
        //        var missingStrategic = strategicNodeIds.Where(id => !result.CenterNodes.Contains(id)).ToList();
        //        if (missingStrategic.Any())
        //        {
        //            Console.WriteLine($"❌ צמתים אסטרטגיים שלא נכללו: {string.Join(", ", missingStrategic)}");
        //            return BadRequest($"האלגוריתם לא הצליח לכלול את כל האזורים האסטרטגיים. חסרים: {string.Join(", ", missingStrategic)}");
        //        }

        //        const double averageSpeed = 13.89;
        //        double maxResponseTimeInSeconds = result.MaxDistance / averageSpeed;

        //        var strategicCount = strategicNodeIds.Count;
        //        var regularCount = result.CenterNodes.Count - strategicCount;

        //        var response = new
        //        {
        //            PolicePositions = result.CenterNodes.Select(nodeId => new
        //            {
        //                NodeId = nodeId,
        //                Latitude = _latestGraph.Nodes[nodeId].Latitude,
        //                Longitude = _latestGraph.Nodes[nodeId].Longitude,
        //                IsStrategic = strategicNodeIds.Contains(nodeId)  // 🎯 סימון אזורים אסטרטגיים
        //            }).ToList(),
        //            MaxDistance = result.MaxDistance,
        //            MaxResponseTimeInSeconds = maxResponseTimeInSeconds,
        //            StrategicOfficers = strategicCount,
        //            RegularOfficers = regularCount,
        //            Message = strategicCount > 0
        //                ? $"פוזרו {request.K} שוטרים - {strategicCount} באזורים אסטרטגיים ו-{regularCount} נוספים. זמן תגובה מקסימלי: {(int)maxResponseTimeInSeconds} שניות."
        //                : $"פוזרו {request.K} שוטרים בהצלחה. זמן תגובה מקסימלי: {(int)maxResponseTimeInSeconds} שניות."
        //        };

        //        return Ok(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"❌ שגיאה בפיזור: {ex.Message}");
        //        return BadRequest($"שגיאה בביצוע האלגוריתם: {ex.Message}");
        //    }
        //}

        /// <summary>
        /// מציאת צומת קרוב באזור המותר
        /// </summary>
        private long FindClosestNodeInBounds(double latitude, double longitude, HashSet<long> allowedNodes)
        {
            long closestNodeId = -1;
            double minDistance = double.MaxValue;

            foreach (var nodeId in allowedNodes)
            {
                if (_latestGraph.Nodes.TryGetValue(nodeId, out var node))
                {
                    double distance = Math.Sqrt(
                        Math.Pow(node.Latitude - latitude, 2) +
                        Math.Pow(node.Longitude - longitude, 2)
                    );

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestNodeId = node.Id;
                    }
                }
            }

            return closestNodeId;
        }

        // מחלקה עבור הבקשה החדשה
        public class DistributeWithStrategicRequest
        {
            public int K { get; set; }
            public List<StrategicZoneRequest> StrategicZones { get; set; } = new List<StrategicZoneRequest>();
        }

        public class StrategicZoneRequest
        {
            public double Latitude { get; set; }
            public double Longitude { get; set; }
        }
    }
}