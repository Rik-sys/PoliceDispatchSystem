
using IBL;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;

namespace PoliceDispatchSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GraphController : ControllerBase
    {
        private readonly IGraphService _graphService;

        public GraphController(IGraphService graphService)
        {
            _graphService = graphService;
        }

        [HttpPost("upload-osm")]
        public ActionResult UploadInitialGraph(
            IFormFile file,
            [FromForm] double? minLat = null,
            [FromForm] double? maxLat = null,
            [FromForm] double? minLon = null,
            [FromForm] double? maxLon = null)
        {
            if (file == null || file.Length == 0)
                return BadRequest("קובץ לא סופק");

            var tempOsmPath = Path.GetTempFileName();

            try
            {
                using (var stream = System.IO.File.Create(tempOsmPath))
                {
                    file.CopyTo(stream);
                }

                var result = _graphService.ProcessInitialOsmFile(tempOsmPath, minLat, maxLat, minLon, maxLon);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה: {ex.Message}");
            }
            finally
            {
                if (System.IO.File.Exists(tempOsmPath))
                    System.IO.File.Delete(tempOsmPath);
            }
        }

        [HttpPost("repair-osm")]
        public ActionResult UploadExtendedOsm(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("קובץ לא סופק");

            var tempOsmPath = Path.GetTempFileName();
            try
            {
                using (var stream = System.IO.File.Create(tempOsmPath))
                {
                    file.CopyTo(stream);
                }

                var result = _graphService.RepairGraphWithExtendedFile(tempOsmPath);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה: {ex.Message}");
            }
            finally
            {
                if (System.IO.File.Exists(tempOsmPath))
                    System.IO.File.Delete(tempOsmPath);
            }
        }

        [HttpGet("components")]
        public ActionResult GetComponents()
        {
            var result = _graphService.GetConnectedComponentsInfo();
            if (result == null)
                return BadRequest("לא הועלה קובץ");
            return Ok(result);
        }

        [HttpGet("get-node-location")]
        public ActionResult GetNodeLocation(long nodeId)
        {
            var result = _graphService.GetNodeLocation(nodeId);
            if (result == null)
                return NotFound($"Node ID {nodeId} not found.");
            return Ok(result);
        }

        [HttpGet("bounds")]
        public ActionResult GetBounds()
        {
            var result = _graphService.GetCurrentBounds();
            if (result == null)
                return NotFound("לא נמצאו גבולות מוגדרים");
            return Ok(result);
        }

        [HttpGet("event-graphs")]
        public ActionResult GetEventGraphs()
        {
            var result = _graphService.GetAllEventGraphsInfo();
            return Ok(result);
        }

        [HttpDelete("cleanup-old-graphs")]
        public ActionResult CleanupOldEventGraphs([FromQuery] int maxAgeHours = 24)
        {
            var result = _graphService.CleanupOldEventGraphs(maxAgeHours);
            return Ok(result);
        }
    }
}




