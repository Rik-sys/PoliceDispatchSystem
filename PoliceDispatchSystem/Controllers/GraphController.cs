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

        [HttpPost("upload-osm-and-get-graph")]
        public ActionResult<Graph> UploadAndReturnGraph(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("קובץ לא סופק");

            var tempPath = Path.GetTempFileName();

            using (var stream = System.IO.File.Create(tempPath))
            {
                file.CopyTo(stream);
            }

            try
            {
                var graph = _graphService.BuildGraphFromOsm(tempPath);
                return Ok(graph); // מחזירים את הגרף כולו
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה: {ex.Message}");
            }
        }

        

        [HttpPost("upload-osm")]
        public ActionResult<bool> UploadAndConvertGraph(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("קובץ לא סופק");

            var tempPath = Path.GetTempFileName();

            using (var stream = System.IO.File.Create(tempPath))
            {
                file.CopyTo(stream);
            }

            try
            {
                // בניית הגרף
                var graph = _graphService.BuildGraphFromOsm(tempPath);

                // המרת הגרף לתמונה
                var imagePath = Path.Combine(Path.GetTempPath(), "graph_image.png");

                // הנח שהשתמשת בקוד להמיר גרף לתמונה (כפי שציינת קודם)
                GraphToImageConverter.ConvertGraphToImage(graph); // הכנס את הקריאה המתאימה שלך כאן

                // תחזיר את הגרף למקרה שצריך אותו לבדיקות
                return Ok(new { IsConnected = graph.IsConnected(), ImagePath = imagePath });
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


        //[HttpPost("upload-osm")]
        //public ActionResult<bool> UploadAndConvertGraph(IFormFile file)
        //{
        //    if (file == null || file.Length == 0)
        //        return BadRequest("קובץ לא סופק");

        //    var tempPath = Path.GetTempFileName();

        //    using (var stream = System.IO.File.Create(tempPath))
        //    {
        //        file.CopyTo(stream);
        //    }

        //    try
        //    {
        //        var graph = _graphService.BuildGraphFromOsm(tempPath);
        //        return Ok(graph.IsConnected());
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"שגיאה: {ex.Message}");
        //    }
        //    finally
        //    {
        //        if (System.IO.File.Exists(tempPath))
        //            System.IO.File.Delete(tempPath);
        //    }
        //}
    }
}
