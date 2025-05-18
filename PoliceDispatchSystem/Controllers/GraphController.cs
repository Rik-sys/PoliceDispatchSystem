
//עם המרה-מקבל קובץ OSM רגיל
using BLL;
using DAL;
using DTO;
using IBL;
using Microsoft.AspNetCore.Mvc;
using PoliceDispatchSystem.Services;
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

        private static Dictionary<long, (double lat, double lon)> latestNodes = null;
        private static Graph latestGraph = null;

        [HttpPost("upload-osm")]
        public ActionResult UploadInitialGraph(IFormFile file)
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

                // המרה לקובץ PBF באופן אוטומטי
                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

                // עיבוד הגרף מהקובץ המומר
                var (nodesData, edgesData) = OsmFileReader.LoadOsmData(pbfPath);
                var graph = _graphService.BuildGraphFromOsm(pbfPath);

                latestNodes = nodesData;
                latestGraph = graph;

                if (graph.IsConnected())
                {
                    GraphToImageConverter.ConvertGraphToImage(graph);
                    return Ok(new
                    {
                        IsConnected = true,
                        Message = "הגרף קשיר, ניתן להמשיך לאלגוריתם פיזור השוטרים",
                        ImagePath = "graph_image.png",
                        ComponentCount = 1
                    });
                }
                else
                {
                    var components = graph.GetConnectedComponents();
                    return Ok(new
                    {
                        IsConnected = false,
                        Message = $"הגרף לא קשיר - נמצאו {components.Count} רכיבים קשירים. נא לטעון קובץ עם תחום רחב יותר",
                        ComponentCount = components.Count
                    });
                }
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
            if (latestGraph == null || latestNodes == null)
                return BadRequest("לא הועלה קובץ בסיסי קודם");

            var tempOsmPath = Path.GetTempFileName();
            try
            {
                using (var stream = System.IO.File.Create(tempOsmPath))
                {
                    file.CopyTo(stream);
                }

                // המרה לקובץ PBF באופן אוטומטי
                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

                // תיקון הגרף באמצעות הקובץ המומר
                var repairedGraph = _graphService.TryRepairWithExtendedFile(latestGraph, latestNodes, pbfPath);

                if (repairedGraph.IsConnected())
                {
                    GraphToImageConverter.ConvertGraphToImage(repairedGraph);
                    return Ok(new
                    {
                        IsConnected = true,
                        Message = "בוצע חיבור חכם בין רכיבי הקשירות",
                        ImagePath = "graph_image.png"
                    });
                }
                else
                {
                    var components = repairedGraph.GetConnectedComponents();
                    return Ok(new
                    {
                        IsConnected = false,
                        Message = $"עדיין לא הצלחנו לחבר את הגרף. נמצאו {components.Count} רכיבים קשירים.",
                        ComponentCount = components.Count
                    });
                }
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
            if (latestGraph == null)
                return BadRequest("לא הועלה קובץ");

            var components = latestGraph.GetConnectedComponents();
            return Ok(new
            {
                TotalComponents = components.Count,
                ComponentSizes = components.Select(c => c.Count).ToList()
            });
        }
    }
}

