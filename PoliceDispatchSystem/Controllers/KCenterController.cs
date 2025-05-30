
//using BLL;
//using DTO;
//using IBL;
//using Microsoft.AspNetCore.Mvc;
//using System.Collections.Generic;
//using System.Linq;

//namespace PoliceDispatchSystem.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class KCenterController : ControllerBase
//    {
//        private readonly IKCenterService _kCenterService;
//        private static Graph _latestGraph;
//        private static Dictionary<long, (double lat, double lon)> _latestNodes;
//        private static Dictionary<long, bool> _nodesInOriginalBounds = new Dictionary<long, bool>();

//        public KCenterController(IKCenterService kCenterService)
//        {
//            _kCenterService = kCenterService;
//        }

//        public static void SetLatestGraph(Graph graph)
//        {
//            _latestGraph = graph;
//        }

//        public static void SetLatestNodes(Dictionary<long, (double lat, double lon)> nodes)
//        {
//            _latestNodes = nodes;
//        }

//        public static void SetNodesInOriginalBounds(Dictionary<long, bool> nodesInBounds)
//        {
//            _nodesInOriginalBounds = new Dictionary<long, bool>(nodesInBounds);
//        }

//        [HttpPost("distribute")]
//        public ActionResult DistributePolice(int k)
//        {
//            if (_latestGraph == null)
//                return BadRequest("לא הועלה קובץ גרף");

//            if (k <= 0)
//                return BadRequest("מספר השוטרים חייב להיות גדול מאפס");

//            try
//            {
//                var originalNodesIds = _nodesInOriginalBounds
//                    .Where(kvp => kvp.Value == true)
//                    .Select(kvp => kvp.Key)
//                    .ToHashSet();

//                var result = _kCenterService.DistributePolice(_latestGraph, k, originalNodesIds);

//                // המרה של מרחק (מטרים) לזמן תגובה בשניות (בהנחה ש-13.89 מטר לשנייה = 50 קמ"ש)
//                const double averageSpeed = 13.89;
//                double maxResponseTimeInSeconds = result.MaxDistance / averageSpeed;

//                var response = new
//                {
//                    PolicePositions = result.CenterNodes.Select(nodeId => new
//                    {
//                        NodeId = nodeId,
//                        Latitude = _latestGraph.Nodes[nodeId].Latitude,
//                        Longitude = _latestGraph.Nodes[nodeId].Longitude
//                    }).ToList(),
//                    MaxDistance = result.MaxDistance, // מרחק במטרים
//                    MaxResponseTimeInSeconds = maxResponseTimeInSeconds, // זמן תגובה בשניות
//                    Message = $"פוזרו {k} שוטרים בהצלחה. זמן תגובה מקסימלי: {(int)maxResponseTimeInSeconds} שניות."
//                };

//                return Ok(response);
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה בביצוע האלגוריתם: {ex.Message}");
//            }
//        }

//        [HttpGet("is-in-original-bounds")]
//        public ActionResult IsNodeInOriginalBounds(long nodeId)
//        {
//            if (_nodesInOriginalBounds.TryGetValue(nodeId, out bool isInBounds))
//            {
//                return Ok(new { nodeId, isInOriginalBounds = isInBounds });
//            }
//            return NotFound($"Node ID {nodeId} not found in bounds information.");
//        }

//        [HttpGet("original-bounds-nodes")]
//        public ActionResult GetNodesInOriginalBounds()
//        {
//            var originalBoundsNodes = _nodesInOriginalBounds
//                .Where(kvp => kvp.Value == true)
//                .Select(kvp => kvp.Key)
//                .ToList();

//            return Ok(new
//            {
//                Count = originalBoundsNodes.Count,
//                NodeIds = originalBoundsNodes
//            });
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
    }
}