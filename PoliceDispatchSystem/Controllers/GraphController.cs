//using BLL;
//using DTO;
//using IBL;
//using Microsoft.AspNetCore.Mvc;
//using PoliceDispatchSystem.Services;
//using System.Diagnostics;
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




//        [HttpPost("upload-osm")]
//        public ActionResult<bool> UploadAndConvertGraph(IFormFile file)
//        {
//            if (file == null || file.Length == 0)
//                return BadRequest("קובץ לא סופק");

//            var tempPath = Path.GetTempFileName();

//            using (var stream = System.IO.File.Create(tempPath))
//            {
//                file.CopyTo(stream);
//            }

//            try
//            {
//                // בניית הגרף
//                var graph = _graphService.BuildGraphFromOsm(tempPath);

//                // המרת הגרף לתמונה
//                var imagePath = Path.Combine(Path.GetTempPath(), "graph_image.png");

//                // הנח שהשתמשת בקוד להמיר גרף לתמונה (כפי שציינת קודם)
//                GraphToImageConverter.ConvertGraphToImage(graph); // הכנס את הקריאה המתאימה שלך כאן

//                // תחזיר את הגרף למקרה שצריך אותו לבדיקות
//                return Ok(new { IsConnected = graph.IsConnected(), ImagePath = imagePath });
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה: {ex.Message}");
//            }
//            finally
//            {
//                if (System.IO.File.Exists(tempPath))
//                    System.IO.File.Delete(tempPath);
//            }
//        }



//    }
//}

//האחרון
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

        // מחזיק את הנתונים האחרונים שהועלו לשימוש בריפוי
        private static Dictionary<long, (double lat, double lon)> latestNodes = null;
        private static Graph latestGraph = null;

        [HttpPost("upload-osm")]
        public ActionResult UploadInitialGraph(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("קובץ לא סופק");

            var tempPath = Path.GetTempFileName();
            try
            {
                using (var stream = System.IO.File.Create(tempPath))
                {
                    file.CopyTo(stream);
                }

                var (nodesData, edgesData) = OsmFileReader.LoadOsmData(tempPath);
                var graph = _graphService.BuildGraphFromOsm(tempPath);

                // שמירת הנתונים לשימוש מאוחר יותר
                latestNodes = nodesData;
                latestGraph = graph;

                if (graph.IsConnected())
                {
                    // מציג את התוצאה במפה
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
                    // מחזיר מידע על חוסר קשירות
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
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }

        [HttpPost("repair-osm")]
        public ActionResult UploadExtendedOsm(IFormFile file)
        {
            if (latestGraph == null || latestNodes == null)
                return BadRequest("לא הועלה קובץ בסיסי קודם");

            var tempPath = Path.GetTempFileName();
            try
            {
                using (var stream = System.IO.File.Create(tempPath))
                {
                    file.CopyTo(stream);
                }

                // תיקון הגרף באמצעות קובץ מורחב
                var repairedGraph = _graphService.TryRepairWithExtendedFile(latestGraph, latestNodes, tempPath);

                // בדיקה אם התיקון הצליח
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
                        Message = $"עדיין לא הצלחנו לחבר את הגרף. נמצאו {components.Count} רכיבים קשירים. יתכן שהתחימה לא כוללת את הדרכים המחברות.",
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
                if (System.IO.File.Exists(tempPath))
                    System.IO.File.Delete(tempPath);
            }
        }

        // אפשרי להוסיף נקודת קצה נוספת לקבלת הרכיבים הקשירים
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
