using Microsoft.AspNetCore.Mvc;
using DTO;
using IBL;
using System.Collections.Generic;

namespace PoliceDispatchSystem.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class OfficersController : ControllerBase
    {
        private readonly IOfficerAssignmentService _officerAssignmentService;

        public OfficersController(IOfficerAssignmentService officerAssignmentService)
        {
            _officerAssignmentService = officerAssignmentService;
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
    }
}
