
////עם המרה-מקבל קובץ OSM רגיל
//using BLL;
//using DAL;
//using DTO;
//using IBL;
//using Microsoft.AspNetCore.Mvc;
//using PoliceDispatchSystem.Services;
//using System.IO;

//namespace PoliceDispatchSystem.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class GraphController : ControllerBase
//    {
//        private readonly IGraphService _graphService;

//        public GraphController(IGraphService graphService)
//        {
//            _graphService = graphService;
//        }

//        private static Dictionary<long, (double lat, double lon)> latestNodes = null;
//        private static Graph latestGraph = null;

//        [HttpPost("upload-osm")]
//        public ActionResult UploadInitialGraph(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                return BadRequest("קובץ לא סופק");

//            var tempOsmPath = Path.GetTempFileName();

//            try
//            {
//                using (var stream = System.IO.File.Create(tempOsmPath))
//                {
//                    file.CopyTo(stream);
//                }

//                // המרה לקובץ PBF באופן אוטומטי
//                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

//                // עיבוד הגרף מהקובץ המומר
//                var (nodesData, edgesData) = OsmFileReader.LoadOsmData(pbfPath);
//                var graph = _graphService.BuildGraphFromOsm(pbfPath);

//                latestNodes = nodesData;
//                latestGraph = graph;

//                if (graph.IsConnected())
//                {
//                    GraphToImageConverter.ConvertGraphToImage(graph);
//                    return Ok(new
//                    {
//                        IsConnected = true,
//                        Message = "הגרף קשיר, ניתן להמשיך לאלגוריתם פיזור השוטרים",
//                        ImagePath = "graph_image.png",
//                        ComponentCount = 1
//                    });
//                }
//                else
//                {
//                    var components = graph.GetConnectedComponents();
//                    return Ok(new
//                    {
//                        IsConnected = false,
//                        Message = $"הגרף לא קשיר - נמצאו {components.Count} רכיבים קשירים. נא לטעון קובץ עם תחום רחב יותר",
//                        ComponentCount = components.Count
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה: {ex.Message}");
//            }
//            finally
//            {
//                if (System.IO.File.Exists(tempOsmPath))
//                    System.IO.File.Delete(tempOsmPath);
//            }
//        }

//        [HttpPost("repair-osm")]
//        public ActionResult UploadExtendedOsm(IFormFile file)
//        {
//            if (latestGraph == null || latestNodes == null)
//                return BadRequest("לא הועלה קובץ בסיסי קודם");

//            var tempOsmPath = Path.GetTempFileName();
//            try
//            {
//                using (var stream = System.IO.File.Create(tempOsmPath))
//                {
//                    file.CopyTo(stream);
//                }

//                // המרה לקובץ PBF באופן אוטומטי
//                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

//                // תיקון הגרף באמצעות הקובץ המומר
//                var repairedGraph = _graphService.TryRepairWithExtendedFile(latestGraph, latestNodes, pbfPath);

//                if (repairedGraph.IsConnected())
//                {
//                    GraphToImageConverter.ConvertGraphToImage(repairedGraph);
//                    return Ok(new
//                    {
//                        IsConnected = true,
//                        Message = "בוצע חיבור חכם בין רכיבי הקשירות",
//                        ImagePath = "graph_image.png"
//                    });
//                }
//                else
//                {
//                    var components = repairedGraph.GetConnectedComponents();
//                    return Ok(new
//                    {
//                        IsConnected = false,
//                        Message = $"עדיין לא הצלחנו לחבר את הגרף. נמצאו {components.Count} רכיבים קשירים.",
//                        ComponentCount = components.Count
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה: {ex.Message}");
//            }
//            finally
//            {
//                if (System.IO.File.Exists(tempOsmPath))
//                    System.IO.File.Delete(tempOsmPath);
//            }
//        }


//        [HttpGet("components")]
//        public ActionResult GetComponents()
//        {
//            if (latestGraph == null)
//                return BadRequest("לא הועלה קובץ");

//            var components = latestGraph.GetConnectedComponents();
//            return Ok(new
//            {
//                TotalComponents = components.Count,
//                ComponentSizes = components.Select(c => c.Count).ToList()
//            });
//        }
//    }
//}

//מעביר לקונטרולר השני
//using BLL;
//using DAL;
//using DTO;
//using IBL;
//using Microsoft.AspNetCore.Mvc;
//using PoliceDispatchSystem.Controllers;
//using PoliceDispatchSystem.Services;
//using System.IO;

//namespace PoliceDispatchSystem.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class GraphController : ControllerBase
//    {
//        private readonly IGraphService _graphService;

//        public GraphController(IGraphService graphService)
//        {
//            _graphService = graphService;
//        }

//        private static Dictionary<long, (double lat, double lon)> latestNodes = null;
//        private static Graph latestGraph = null;

//        [HttpPost("upload-osm")]
//        public ActionResult UploadInitialGraph(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                return BadRequest("קובץ לא סופק");

//            var tempOsmPath = Path.GetTempFileName();

//            try
//            {
//                using (var stream = System.IO.File.Create(tempOsmPath))
//                {
//                    file.CopyTo(stream);
//                }

//                // המרה לקובץ PBF באופן אוטומטי
//                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

//                // עיבוד הגרף מהקובץ המומר
//                var (nodesData, edgesData) = OsmFileReader.LoadOsmData(pbfPath);
//                var graph = _graphService.BuildGraphFromOsm(pbfPath);

//                latestNodes = nodesData;
//                latestGraph = graph;
//                KCenterController.SetLatestGraph(graph); // העברת הגרף לקונטרולר של האלגוריתם

//                if (graph.IsConnected())
//                {
//                    GraphToImageConverter.ConvertGraphToImage(graph);
//                    return Ok(new
//                    {
//                        IsConnected = true,
//                        Message = "הגרף קשיר, ניתן להמשיך לאלגוריתם פיזור השוטרים",
//                        ImagePath = "graph_image.png",
//                        ComponentCount = 1
//                    });
//                }
//                else
//                {
//                    var components = graph.GetConnectedComponents();
//                    return Ok(new
//                    {
//                        IsConnected = false,
//                        Message = $"הגרף לא קשיר - נמצאו {components.Count} רכיבים קשירים. נא לטעון קובץ עם תחום רחב יותר",
//                        ComponentCount = components.Count
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה: {ex.Message}");
//            }
//            finally
//            {
//                if (System.IO.File.Exists(tempOsmPath))
//                    System.IO.File.Delete(tempOsmPath);
//            }
//        }

//        [HttpPost("repair-osm")]
//        public ActionResult UploadExtendedOsm(IFormFile file)
//        {
//            if (latestGraph == null || latestNodes == null)
//                return BadRequest("לא הועלה קובץ בסיסי קודם");

//            var tempOsmPath = Path.GetTempFileName();
//            try
//            {
//                using (var stream = System.IO.File.Create(tempOsmPath))
//                {
//                    file.CopyTo(stream);
//                }

//                // המרה לקובץ PBF באופן אוטומטי
//                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

//                // תיקון הגרף באמצעות הקובץ המומר
//                var repairedGraph = _graphService.TryRepairWithExtendedFile(latestGraph, latestNodes, pbfPath);

//                if (repairedGraph.IsConnected())
//                {
//                    GraphToImageConverter.ConvertGraphToImage(repairedGraph);
//                    KCenterController.SetLatestGraph(repairedGraph); // עדכון גם כאן
//                    return Ok(new
//                    {
//                        IsConnected = true,
//                        Message = "בוצע חיבור חכם בין רכיבי הקשירות",
//                        ImagePath = "graph_image.png"
//                    });
//                }
//                else
//                {
//                    var components = repairedGraph.GetConnectedComponents();
//                    return Ok(new
//                    {
//                        IsConnected = false,
//                        Message = $"עדיין לא הצלחנו לחבר את הגרף. נמצאו {components.Count} רכיבים קשירים.",
//                        ComponentCount = components.Count
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה: {ex.Message}");
//            }
//            finally
//            {
//                if (System.IO.File.Exists(tempOsmPath))
//                    System.IO.File.Delete(tempOsmPath);
//            }
//        }

//        [HttpGet("components")]
//        public ActionResult GetComponents()
//        {
//            if (latestGraph == null)
//                return BadRequest("לא הועלה קובץ");

//            var components = latestGraph.GetConnectedComponents();
//            return Ok(new
//            {
//                TotalComponents = components.Count,
//                ComponentSizes = components.Select(c => c.Count).ToList()
//            });
//        }

//        [HttpGet("get-node-location")]
//        public ActionResult GetNodeLocation(long nodeId)
//        {
//            if (GraphController.latestNodes != null && GraphController.latestNodes.TryGetValue(nodeId, out var coords))
//            {
//                return Ok(new { lat = coords.lat, lon = coords.lon });
//            }
//            return NotFound($"Node ID {nodeId} not found.");
//        }


//    }
//}

//לוגיקת הפיכה לגרף קשיר עובדת ולוגיקת פיזור רק בתוך התחום לא עובדת
//using BLL;
//using DAL;
//using DTO;
//using IBL;
//using Microsoft.AspNetCore.Mvc;
//using PoliceDispatchSystem.Services;
//using System.IO;

//namespace PoliceDispatchSystem.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class GraphController : ControllerBase
//    {
//        private readonly IGraphService _graphService;

//        public GraphController(IGraphService graphService)
//        {
//            _graphService = graphService;
//        }

//        public static Dictionary<long, (double lat, double lon)> LatestNodes = null;
//        public static Graph LatestGraph = null;

//        [HttpPost("upload-osm")]
//        public ActionResult UploadInitialGraph(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                return BadRequest("קובץ לא סופק");

//            var tempOsmPath = Path.GetTempFileName();

//            try
//            {
//                using (var stream = System.IO.File.Create(tempOsmPath))
//                {
//                    file.CopyTo(stream);
//                }

//                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

//                var (nodesData, edgesData) = OsmFileReader.LoadOsmData(pbfPath);
//                var graph = _graphService.BuildGraphFromOsm(pbfPath);

//                LatestNodes = nodesData;
//                LatestGraph = graph;
//                KCenterController.SetLatestGraph(graph);
//                KCenterController.SetLatestNodes(nodesData);

//                if (graph.IsConnected())
//                {
//                    GraphToImageConverter.ConvertGraphToImage(graph);
//                    return Ok(new
//                    {
//                        IsConnected = true,
//                        Message = "הגרף קשיר, ניתן להמשיך לאלגוריתם פיזור השוטרים",
//                        ImagePath = "graph_image.png",
//                        ComponentCount = 1
//                    });
//                }
//                else
//                {
//                    var components = graph.GetConnectedComponents();
//                    return Ok(new
//                    {
//                        IsConnected = false,
//                        Message = $"הגרף לא קשיר - נמצאו {components.Count} רכיבים קשירים. נא לטעון קובץ עם תחום רחב יותר",
//                        ComponentCount = components.Count
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה: {ex.Message}");
//            }
//            finally
//            {
//                if (System.IO.File.Exists(tempOsmPath))
//                    System.IO.File.Delete(tempOsmPath);
//            }
//        }

//        [HttpPost("repair-osm")]
//        public ActionResult UploadExtendedOsm(IFormFile file)
//        {
//            if (LatestGraph == null || LatestNodes == null)
//                return BadRequest("לא הועלה קובץ בסיסי קודם");

//            var tempOsmPath = Path.GetTempFileName();
//            try
//            {
//                using (var stream = System.IO.File.Create(tempOsmPath))
//                {
//                    file.CopyTo(stream);
//                }

//                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);
//                var repairedGraph = _graphService.TryRepairWithExtendedFile(LatestGraph, LatestNodes, pbfPath);

//                if (repairedGraph.IsConnected())
//                {
//                    GraphToImageConverter.ConvertGraphToImage(repairedGraph);
//                    KCenterController.SetLatestGraph(repairedGraph);
//                    return Ok(new
//                    {
//                        IsConnected = true,
//                        Message = "בוצע חיבור חכם בין רכיבי הקשירות",
//                        ImagePath = "graph_image.png"
//                    });
//                }
//                else
//                {
//                    var components = repairedGraph.GetConnectedComponents();
//                    return Ok(new
//                    {
//                        IsConnected = false,
//                        Message = $"עדיין לא הצלחנו לחבר את הגרף. נמצאו {components.Count} רכיבים קשירים.",
//                        ComponentCount = components.Count
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה: {ex.Message}");
//            }
//            finally
//            {
//                if (System.IO.File.Exists(tempOsmPath))
//                    System.IO.File.Delete(tempOsmPath);
//            }
//        }

//        [HttpGet("components")]
//        public ActionResult GetComponents()
//        {
//            if (LatestGraph == null)
//                return BadRequest("לא הועלה קובץ");

//            var components = LatestGraph.GetConnectedComponents();
//            return Ok(new
//            {
//                TotalComponents = components.Count,
//                ComponentSizes = components.Select(c => c.Count).ToList()
//            });
//        }

//        [HttpGet("get-node-location")]
//        public ActionResult GetNodeLocation(long nodeId)
//        {
//            if (LatestNodes != null && LatestNodes.TryGetValue(nodeId, out var coords))
//            {
//                return Ok(new { lat = coords.lat, lon = coords.lon });
//            }
//            return NotFound($"Node ID {nodeId} not found.");
//        }
//    }
//}
//לוגיקת פיזור בתוך התחום עובדת והקשירות לא
//using BLL;
//using DAL;
//using DTO;
//using IBL;
//using Microsoft.AspNetCore.Mvc;
//using PoliceDispatchSystem.Services;
//using System.IO;

//namespace PoliceDispatchSystem.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class GraphController : ControllerBase
//    {
//        private readonly IGraphService _graphService;

//        public GraphController(IGraphService graphService)
//        {
//            _graphService = graphService;
//        }

//        public static Dictionary<long, (double lat, double lon)> LatestNodes = null;
//        public static Graph LatestGraph = null;
//        public static (double minLat, double maxLat, double minLon, double maxLon)? LatestBounds = null;

//        [HttpPost("upload-osm")]
//        public ActionResult UploadInitialGraph(
//            IFormFile file,
//            [FromForm] double? minLat = null,
//            [FromForm] double? maxLat = null,
//            [FromForm] double? minLon = null,
//            [FromForm] double? maxLon = null)
//        {
//            if (file == null || file.Length == 0)
//                return BadRequest("קובץ לא סופק");

//            var tempOsmPath = Path.GetTempFileName();

//            try
//            {
//                using (var stream = System.IO.File.Create(tempOsmPath))
//                {
//                    file.CopyTo(stream);
//                }

//                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

//                // שמירת הגבולות לשימוש בהמשך
//                if (minLat.HasValue && maxLat.HasValue && minLon.HasValue && maxLon.HasValue)
//                {
//                    LatestBounds = (minLat.Value, maxLat.Value, minLon.Value, maxLon.Value);
//                }

//                // העברת הגבולות לפונקציית הטעינה
//                var (nodesData, edgesData) = OsmFileReader.LoadOsmData(
//                    pbfPath,
//                    minLat,
//                    maxLat,
//                    minLon,
//                    maxLon);

//                var graph = _graphService.BuildGraphFromOsm(nodesData, edgesData);

//                LatestNodes = nodesData;
//                LatestGraph = graph;
//                KCenterController.SetLatestGraph(graph);
//                KCenterController.SetLatestNodes(nodesData);

//                if (graph.IsConnected())
//                {
//                    GraphToImageConverter.ConvertGraphToImage(graph);
//                    return Ok(new
//                    {
//                        IsConnected = true,
//                        Message = "הגרף קשיר, ניתן להמשיך לאלגוריתם פיזור השוטרים",
//                        ImagePath = "graph_image.png",
//                        ComponentCount = 1,
//                        NodeCount = nodesData.Count
//                    });
//                }
//                else
//                {
//                    var components = graph.GetConnectedComponents();
//                    return Ok(new
//                    {
//                        IsConnected = false,
//                        Message = $"הגרף לא קשיר - נמצאו {components.Count} רכיבים קשירים. נא לטעון קובץ עם תחום רחב יותר",
//                        ComponentCount = components.Count,
//                        NodeCount = nodesData.Count
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה: {ex.Message}");
//            }
//            finally
//            {
//                if (System.IO.File.Exists(tempOsmPath))
//                    System.IO.File.Delete(tempOsmPath);
//            }
//        }

//        [HttpPost("repair-osm")]
//        public ActionResult UploadExtendedOsm(
//            IFormFile file,
//            [FromForm] double? minLat = null,
//            [FromForm] double? maxLat = null,
//            [FromForm] double? minLon = null,
//            [FromForm] double? maxLon = null)
//        {
//            if (LatestGraph == null || LatestNodes == null)
//                return BadRequest("לא הועלה קובץ בסיסי קודם");

//            var tempOsmPath = Path.GetTempFileName();
//            try
//            {
//                using (var stream = System.IO.File.Create(tempOsmPath))
//                {
//                    file.CopyTo(stream);
//                }

//                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

//                // שמירת הגבולות החדשים אם סופקו
//                if (minLat.HasValue && maxLat.HasValue && minLon.HasValue && maxLon.HasValue)
//                {
//                    LatestBounds = (minLat.Value, maxLat.Value, minLon.Value, maxLon.Value);
//                }

//                // העברת הגבולות לפונקציית הטעינה
//                var (additionalNodes, additionalEdges) = OsmFileReader.LoadOsmData(
//                    pbfPath,
//                    LatestBounds?.minLat,
//                    LatestBounds?.maxLat,
//                    LatestBounds?.minLon,
//                    LatestBounds?.maxLon);

//                var repairedGraph = _graphService.TryRepairWithExtendedFile(
//                    LatestGraph,
//                    LatestNodes,
//                    additionalNodes,
//                    additionalEdges);

//                if (repairedGraph.IsConnected())
//                {
//                    GraphToImageConverter.ConvertGraphToImage(repairedGraph);
//                    KCenterController.SetLatestGraph(repairedGraph);

//                    // עדכון הצמתים האחרונים (שילוב של הקודמים והחדשים)
//                    var mergedNodes = new Dictionary<long, (double lat, double lon)>(LatestNodes);
//                    foreach (var node in additionalNodes)
//                    {
//                        mergedNodes[node.Key] = node.Value;
//                    }
//                    LatestNodes = mergedNodes;
//                    KCenterController.SetLatestNodes(LatestNodes);

//                    return Ok(new
//                    {
//                        IsConnected = true,
//                        Message = "בוצע חיבור חכם בין רכיבי הקשירות",
//                        ImagePath = "graph_image.png",
//                        NodeCount = LatestNodes.Count
//                    });
//                }
//                else
//                {
//                    var components = repairedGraph.GetConnectedComponents();
//                    return Ok(new
//                    {
//                        IsConnected = false,
//                        Message = $"עדיין לא הצלחנו לחבר את הגרף. נמצאו {components.Count} רכיבים קשירים.",
//                        ComponentCount = components.Count,
//                        NodeCount = additionalNodes.Count
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה: {ex.Message}");
//            }
//            finally
//            {
//                if (System.IO.File.Exists(tempOsmPath))
//                    System.IO.File.Delete(tempOsmPath);
//            }
//        }

//        [HttpGet("components")]
//        public ActionResult GetComponents()
//        {
//            if (LatestGraph == null)
//                return BadRequest("לא הועלה קובץ");

//            var components = LatestGraph.GetConnectedComponents();
//            return Ok(new
//            {
//                TotalComponents = components.Count,
//                ComponentSizes = components.Select(c => c.Count).ToList()
//            });
//        }

//        [HttpGet("get-node-location")]
//        public ActionResult GetNodeLocation(long nodeId)
//        {
//            if (LatestNodes != null && LatestNodes.TryGetValue(nodeId, out var coords))
//            {
//                return Ok(new { lat = coords.lat, lon = coords.lon });
//            }
//            return NotFound($"Node ID {nodeId} not found.");
//        }

//        [HttpGet("bounds")]
//        public ActionResult GetBounds()
//        {
//            if (LatestBounds.HasValue)
//            {
//                return Ok(new
//                {
//                    minLat = LatestBounds.Value.minLat,
//                    maxLat = LatestBounds.Value.maxLat,
//                    minLon = LatestBounds.Value.minLon,
//                    maxLon = LatestBounds.Value.maxLon
//                });
//            }
//            return NotFound("לא נמצאו גבולות מוגדרים");
//        }
//    }
//}
// GraphController.cs - משולב עם תמיכה בקשירויות ותחום פיזור שוטרים בלבד
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

        public static Dictionary<long, (double lat, double lon)> LatestNodes = null;
        public static Graph LatestGraph = null;
        public static (double minLat, double maxLat, double minLon, double maxLon)? LatestBounds = null;

        // גרף עם הקשתות להצגה בלבד, יכול לכלול צמתים מחוץ לתחום
        public static Graph DisplayGraph = null;

        // ערכי מיקום שמזהים אילו צמתים הם בתוך התחום המקורי
        public static Dictionary<long, bool> NodesInOriginalBounds = new Dictionary<long, bool>();

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

                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

                // שמירת הגבולות לשימוש בהמשך
                if (minLat.HasValue && maxLat.HasValue && minLon.HasValue && maxLon.HasValue)
                {
                    LatestBounds = (minLat.Value, maxLat.Value, minLon.Value, maxLon.Value);
                }

                // העברת הגבולות לפונקציית הטעינה
                var (nodesData, edgesData) = OsmFileReader.LoadOsmData(
                    pbfPath,
                    minLat,
                    maxLat,
                    minLon,
                    maxLon);

                var graph = _graphService.BuildGraphFromOsm(nodesData, edgesData);

                LatestNodes = nodesData;
                LatestGraph = graph;
                DisplayGraph = graph; // שמירת הגרף המקורי להצגה

                // סימון כל הצמתים כנמצאים בתוך התחום המקורי
                NodesInOriginalBounds.Clear();
                foreach (var nodeId in nodesData.Keys)
                {
                    NodesInOriginalBounds[nodeId] = true;
                }

                // עדכון המידע בבקר ה-KCenter
                KCenterController.SetLatestGraph(graph);
                KCenterController.SetLatestNodes(nodesData);
                KCenterController.SetNodesInOriginalBounds(NodesInOriginalBounds);

                if (graph.IsConnected())
                {
                    GraphToImageConverter.ConvertGraphToImage(graph);
                    return Ok(new
                    {
                        IsConnected = true,
                        Message = "הגרף קשיר, ניתן להמשיך לאלגוריתם פיזור השוטרים",
                        ImagePath = "graph_image.png",
                        ComponentCount = 1,
                        NodeCount = nodesData.Count
                    });
                }
                else
                {
                    var components = graph.GetConnectedComponents();
                    return Ok(new
                    {
                        IsConnected = false,
                        Message = $"הגרף לא קשיר - נמצאו {components.Count} רכיבים קשירים. נא לטעון קובץ עם תחום רחב יותר",
                        ComponentCount = components.Count,
                        NodeCount = nodesData.Count
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
            if (LatestGraph == null || LatestNodes == null)
                return BadRequest("לא הועלה קובץ בסיסי קודם");

            var tempOsmPath = Path.GetTempFileName();
            try
            {
                using (var stream = System.IO.File.Create(tempOsmPath))
                {
                    file.CopyTo(stream);
                }

                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

                // שימוש באלגוריתם המקורי שפעל עם קבצים חיצוניים
                var repairedGraph = _graphService.TryRepairWithExtendedFile(LatestGraph, LatestNodes, pbfPath);

                // עדכון הגרף להצגה
                DisplayGraph = repairedGraph;

                // סימון הצמתים החדשים כלא נמצאים בתוך התחום המקורי
                foreach (var nodeId in repairedGraph.Nodes.Keys)
                {
                    if (!NodesInOriginalBounds.ContainsKey(nodeId))
                    {
                        NodesInOriginalBounds[nodeId] = false; // צומת מחוץ לתחום המקורי
                    }
                }

                // עדכון המידע בבקר ה-KCenter
                KCenterController.SetLatestGraph(repairedGraph);
                KCenterController.SetNodesInOriginalBounds(NodesInOriginalBounds);

                if (repairedGraph.IsConnected())
                {
                    GraphToImageConverter.ConvertGraphToImage(repairedGraph);
                    return Ok(new
                    {
                        IsConnected = true,
                        Message = "בוצע חיבור חכם בין רכיבי הקשירות",
                        ImagePath = "graph_image.png",
                        NodeCount = repairedGraph.Nodes.Count,
                        OriginalNodesCount = NodesInOriginalBounds.Count(kvp => kvp.Value == true)
                    });
                }
                else
                {
                    var components = repairedGraph.GetConnectedComponents();
                    return Ok(new
                    {
                        IsConnected = false,
                        Message = $"עדיין לא הצלחנו לחבר את הגרף. נמצאו {components.Count} רכיבים קשירים.",
                        ComponentCount = components.Count,
                        NodeCount = repairedGraph.Nodes.Count
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
            if (LatestGraph == null)
                return BadRequest("לא הועלה קובץ");

            var components = LatestGraph.GetConnectedComponents();
            return Ok(new
            {
                TotalComponents = components.Count,
                ComponentSizes = components.Select(c => c.Count).ToList()
            });
        }

        [HttpGet("get-node-location")]
        public ActionResult GetNodeLocation(long nodeId)
        {
            if (LatestNodes != null && LatestNodes.TryGetValue(nodeId, out var coords))
            {
                return Ok(new { lat = coords.lat, lon = coords.lon });
            }
            return NotFound($"Node ID {nodeId} not found.");
        }

        [HttpGet("bounds")]
        public ActionResult GetBounds()
        {
            if (LatestBounds.HasValue)
            {
                return Ok(new
                {
                    minLat = LatestBounds.Value.minLat,
                    maxLat = LatestBounds.Value.maxLat,
                    minLon = LatestBounds.Value.minLon,
                    maxLon = LatestBounds.Value.maxLon
                });
            }
            return NotFound("לא נמצאו גבולות מוגדרים");
        }
    }
}
