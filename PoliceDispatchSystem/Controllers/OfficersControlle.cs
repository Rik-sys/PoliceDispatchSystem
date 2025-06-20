using Microsoft.AspNetCore.Mvc;
using DTO;
using IBL;

namespace PoliceDispatchSystem.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficersController : ControllerBase
    {
        private readonly IOfficerAssignmentService _officerAssignmentService;
        private readonly IPoliceOfficerService _officerService;
        public OfficersController(IOfficerAssignmentService officerAssignmentService, IPoliceOfficerService officerService)
        {
            _officerAssignmentService = officerAssignmentService;
            _officerService = officerService;
        }

        [HttpGet("locations")]
        public ActionResult<List<OfficerAssignmentDTO>> GetAllOfficerLocations()
        {
            try
            {
                var allAssignments = _officerAssignmentService.GetAllAssignments();
                return Ok(allAssignments);
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בשליפת מיקומי שוטרים: {ex.Message}");
            }
        }
        [HttpGet("{officerId}/status")]
        public ActionResult<OfficerStatusDTO> GetOfficerStatus(int officerId)
        {
            try
            {
                var status = _officerService.GetOfficerStatus(officerId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בשליפת סטטוס שוטר: {ex.Message}");
            }
        }

    }
}
