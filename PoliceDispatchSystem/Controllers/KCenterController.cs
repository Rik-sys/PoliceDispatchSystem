
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