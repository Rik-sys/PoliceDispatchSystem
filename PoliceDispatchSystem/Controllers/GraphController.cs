

// GraphController.cs - משולב עם תמיכה בקשירויות ותחום פיזור שוטרים בלבד
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

//        // גרף עם הקשתות להצגה בלבד, יכול לכלול צמתים מחוץ לתחום
//        public static Graph DisplayGraph = null;

//        // ערכי מיקום שמזהים אילו צמתים הם בתוך התחום המקורי
//        public static Dictionary<long, bool> NodesInOriginalBounds = new Dictionary<long, bool>();

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
//                DisplayGraph = graph; // שמירת הגרף המקורי להצגה

//                // סימון כל הצמתים כנמצאים בתוך התחום המקורי
//                NodesInOriginalBounds.Clear();
//                foreach (var nodeId in nodesData.Keys)
//                {
//                    NodesInOriginalBounds[nodeId] = true;
//                }

//                // עדכון המידע בבקר ה-KCenter
//                KCenterController.SetLatestGraph(graph);
//                KCenterController.SetLatestNodes(nodesData);
//                KCenterController.SetNodesInOriginalBounds(NodesInOriginalBounds);

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

//                // שימוש באלגוריתם המקורי שפעל עם קבצים חיצוניים
//                var repairedGraph = _graphService.TryRepairWithExtendedFile(LatestGraph, LatestNodes, pbfPath);

//                // עדכון הגרף להצגה
//                DisplayGraph = repairedGraph;

//                // סימון הצמתים החדשים כלא נמצאים בתוך התחום המקורי
//                foreach (var nodeId in repairedGraph.Nodes.Keys)
//                {
//                    if (!NodesInOriginalBounds.ContainsKey(nodeId))
//                    {
//                        NodesInOriginalBounds[nodeId] = false; // צומת מחוץ לתחום המקורי
//                    }
//                }

//                // עדכון המידע בבקר ה-KCenter
//                KCenterController.SetLatestGraph(repairedGraph);
//                KCenterController.SetNodesInOriginalBounds(NodesInOriginalBounds);

//                if (repairedGraph.IsConnected())
//                {
//                    GraphToImageConverter.ConvertGraphToImage(repairedGraph);
//                    return Ok(new
//                    {
//                        IsConnected = true,
//                        Message = "בוצע חיבור חכם בין רכיבי הקשירות",
//                        ImagePath = "graph_image.png",
//                        NodeCount = repairedGraph.Nodes.Count,
//                        OriginalNodesCount = NodesInOriginalBounds.Count(kvp => kvp.Value == true)
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
//                        NodeCount = repairedGraph.Nodes.Count
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

        // מבנה נתונים לשמירת מידע גרף
        public class GraphData
        {
            public Dictionary<long, (double lat, double lon)> Nodes { get; set; }
            public Graph Graph { get; set; }
            public Dictionary<long, bool> NodesInOriginalBounds { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        // שמירת הגרף הנוכחי (לתאימות לאחור)
        public static Dictionary<long, (double lat, double lon)> LatestNodes = null;
        public static Graph LatestGraph = null;
        public static (double minLat, double maxLat, double minLon, double maxLon)? LatestBounds = null;
        public static Graph DisplayGraph = null;
        public static Dictionary<long, bool> NodesInOriginalBounds = new Dictionary<long, bool>();

        // מילון לשמירת גרפים לפי מזהה אירוע
        private static Dictionary<int, GraphData> _eventGraphs = new Dictionary<int, GraphData>();

        // שמירת גרף עבור אירוע ספציפי
        public static void SaveGraphForEvent(int eventId, Graph graph, Dictionary<long, (double lat, double lon)> nodes, Dictionary<long, bool> nodesInBounds)
        {
            _eventGraphs[eventId] = new GraphData
            {
                Graph = graph,
                Nodes = new Dictionary<long, (double lat, double lon)>(nodes),
                NodesInOriginalBounds = new Dictionary<long, bool>(nodesInBounds),
                CreatedAt = DateTime.UtcNow
            };
        }

        // שליפת גרף עבור אירוע ספציפי
        public static GraphData GetGraphForEvent(int eventId)
        {
            return _eventGraphs.TryGetValue(eventId, out var graphData) ? graphData : null;
        }

        // מחיקת גרף עבור אירוע ספציפי
        public static void RemoveGraphForEvent(int eventId)
        {
            _eventGraphs.Remove(eventId);
        }

        // קבלת רשימת כל האירועים עם גרפים
        public static Dictionary<int, DateTime> GetAllEventGraphs()
        {
            return _eventGraphs.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.CreatedAt);
        }

        // ניקוי גרפים ישנים (אופציונלי - למקרה של זיכרון מלא)
        public static void CleanupOldGraphs(TimeSpan maxAge)
        {
            var cutoffTime = DateTime.UtcNow - maxAge;
            var keysToRemove = _eventGraphs
                .Where(kvp => kvp.Value.CreatedAt < cutoffTime)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in keysToRemove)
            {
                _eventGraphs.Remove(key);
            }
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

                // המרה ל־PBF
                string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

                if (minLat.HasValue && maxLat.HasValue && minLon.HasValue && maxLon.HasValue)
                {
                    LatestBounds = (minLat.Value, maxLat.Value, minLon.Value, maxLon.Value);
                }

                // ✅ שימוש בפונקציה המשודרגת של קלוד
                var graph = OsmFileReader.LoadOsmDataToGraph(pbfPath, coord =>
                {
                    return (!minLat.HasValue || coord.lat >= minLat.Value) &&
                           (!maxLat.HasValue || coord.lat <= maxLat.Value) &&
                           (!minLon.HasValue || coord.lon >= minLon.Value) &&
                           (!maxLon.HasValue || coord.lon <= maxLon.Value);
                });

                var nodesData = graph.Nodes.ToDictionary(
                    kvp => kvp.Key,
                    kvp => (kvp.Value.Latitude, kvp.Value.Longitude)
                );

                LatestNodes = nodesData;
                LatestGraph = graph;
                DisplayGraph = graph;

                // חישוב אילו צמתים בתחום המקורי
                NodesInOriginalBounds.Clear();
                foreach (var nodeId in nodesData.Keys)
                {
                    var coord = nodesData[nodeId];
                    bool inBounds =
                        (!minLat.HasValue || coord.Latitude >= minLat.Value) &&
                        (!maxLat.HasValue || coord.Latitude <= maxLat.Value) &&
                        (!minLon.HasValue || coord.Longitude >= minLon.Value) &&
                        (!maxLon.HasValue || coord.Longitude <= maxLon.Value);

                    NodesInOriginalBounds[nodeId] = inBounds;
                }

                // עדכון לקונטרולרים אחרים
                KCenterController.SetLatestGraph(graph);
                KCenterController.SetLatestNodes(nodesData);
                KCenterController.SetNodesInOriginalBounds(NodesInOriginalBounds);

                // בדיקת קשירות והחזרת תשובה
                if (graph.IsConnected())
                {
                    GraphToImageConverter.ConvertGraphToImage(graph);
                    return Ok(new
                    {
                        IsConnected = true,
                        Message = "הגרף קשיר, ניתן להמשיך לאלגוריתם פיזור השוטרים",
                        ImagePath = "graph_image.png",
                        ComponentCount = 1,
                        NodeCount = nodesData.Count,
                        WaySegmentsCount = graph.WaySegments.Count // 🆕 מידע נוסף
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
                        NodeCount = nodesData.Count,
                        WaySegmentsCount = graph.WaySegments.Count // 🆕 מידע נוסף
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

        //[HttpPost("upload-osm")]
        //public ActionResult UploadInitialGraph(
        //    IFormFile file,
        //    [FromForm] double? minLat = null,
        //    [FromForm] double? maxLat = null,
        //    [FromForm] double? minLon = null,
        //    [FromForm] double? maxLon = null)
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest("קובץ לא סופק");

        //    var tempOsmPath = Path.GetTempFileName();

        //    try
        //    {
        //        using (var stream = System.IO.File.Create(tempOsmPath))
        //        {
        //            file.CopyTo(stream);
        //        }

        //        string pbfPath = OsmConversionService.ConvertOsmToPbf(tempOsmPath);

        //        if (minLat.HasValue && maxLat.HasValue && minLon.HasValue && maxLon.HasValue)
        //        {
        //            LatestBounds = (minLat.Value, maxLat.Value, minLon.Value, maxLon.Value);
        //        }

        //        var (nodesData, edgesData) = OsmFileReader.LoadOsmData(
        //            pbfPath,
        //            minLat,
        //            maxLat,
        //            minLon,
        //            maxLon);

        //        var graph = _graphService.BuildGraphFromOsm(nodesData, edgesData);

        //        LatestNodes = nodesData;
        //        LatestGraph = graph;
        //        DisplayGraph = graph;

        //        NodesInOriginalBounds.Clear();
        //        foreach (var nodeId in nodesData.Keys)
        //        {
        //            NodesInOriginalBounds[nodeId] = true;
        //        }

        //        KCenterController.SetLatestGraph(graph);
        //        KCenterController.SetLatestNodes(nodesData);
        //        KCenterController.SetNodesInOriginalBounds(NodesInOriginalBounds);

        //        if (graph.IsConnected())
        //        {
        //            GraphToImageConverter.ConvertGraphToImage(graph);
        //            return Ok(new
        //            {
        //                IsConnected = true,
        //                Message = "הגרף קשיר, ניתן להמשיך לאלגוריתם פיזור השוטרים",
        //                ImagePath = "graph_image.png",
        //                ComponentCount = 1,
        //                NodeCount = nodesData.Count
        //            });
        //        }
        //        else
        //        {
        //            var components = graph.GetConnectedComponents();
        //            return Ok(new
        //            {
        //                IsConnected = false,
        //                Message = $"הגרף לא קשיר - נמצאו {components.Count} רכיבים קשירים. נא לטעון קובץ עם תחום רחב יותר",
        //                ComponentCount = components.Count,
        //                NodeCount = nodesData.Count
        //            });
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"שגיאה: {ex.Message}");
        //    }
        //    finally
        //    {
        //        if (System.IO.File.Exists(tempOsmPath))
        //            System.IO.File.Delete(tempOsmPath);
        //    }
        //}

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

                var repairedGraph = _graphService.TryRepairWithExtendedFile(LatestGraph, LatestNodes, pbfPath);

                DisplayGraph = repairedGraph;

                foreach (var nodeId in repairedGraph.Nodes.Keys)
                {
                    if (!NodesInOriginalBounds.ContainsKey(nodeId))
                    {
                        NodesInOriginalBounds[nodeId] = false;
                    }
                }

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

        [HttpGet("event-graphs")]
        public ActionResult GetEventGraphs()
        {
            var eventGraphs = GetAllEventGraphs();
            return Ok(new
            {
                TotalEvents = eventGraphs.Count,
                Events = eventGraphs.Select(kvp => new
                {
                    EventId = kvp.Key,
                    CreatedAt = kvp.Value,
                    NodeCount = _eventGraphs[kvp.Key].Nodes.Count
                }).ToList()
            });
        }

        [HttpDelete("cleanup-old-graphs")]
        public ActionResult CleanupOldEventGraphs([FromQuery] int maxAgeHours = 24)
        {
            var initialCount = _eventGraphs.Count;
            CleanupOldGraphs(TimeSpan.FromHours(maxAgeHours));
            var finalCount = _eventGraphs.Count;

            return Ok(new
            {
                Message = $"נוקו {initialCount - finalCount} גרפים ישנים",
                RemainingGraphs = finalCount
            });
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
