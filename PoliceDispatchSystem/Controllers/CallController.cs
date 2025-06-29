

using DTO;
using IBL;
using Microsoft.AspNetCore.Mvc;
using System;

namespace PoliceDispatchSystem.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallController : ControllerBase
    {
        private readonly ICallManagementService _callManagementService;
        private readonly ICallAssignmentService _callAssignmentService;
        private readonly ICallService _callService;

        public CallController(
            ICallManagementService callManagementService,
            ICallAssignmentService callAssignmentService,
            ICallService callService)
        {
            _callManagementService = callManagementService;
            _callAssignmentService = callAssignmentService;
            _callService = callService;
        }

        [HttpPost("create")]
        public IActionResult CreateCall([FromBody] CallDTO request)
        {
            try
            {
                var response = _callManagementService.CreateCall(request);
                return Ok(response);
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
                Console.WriteLine($" שגיאה ביצירת קריאה: {ex.Message}");
                return BadRequest($"שגיאה ביצירת הקריאה: {ex.Message}");
            }
        }
        [HttpGet("all")]
        public IActionResult GetAllCalls()
        {
            try
            {
                var calls = _callService.GetAllCalls();

                return Ok(calls.Select(c => new
                {
                    CallId = c.CallId,
                    RequiredOfficers = c.RequiredOfficers,
                    ContactPhone = c.ContactPhone,
                    UrgencyLevel = c.UrgencyLevel,
                    CallTime = c.CallTime,
                    Status = c.Status,
                    Latitude = c.Latitude,
                    Longitude = c.Longitude
                }));
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בשליפת כל הקריאות: {ex.Message}");
            }
        }

        [HttpGet("call/{callId}")]
        public IActionResult GetCallDetails(int callId)
        {
            try
            {
                var assignments = _callAssignmentService.GetAssignmentsByCall(callId);

                return Ok(new
                {
                    CallId = callId,
                    AssignedOfficers = assignments.Select(a => new
                    {
                        OfficerId = a.PoliceOfficerId,
                        CallId = a.CallId,
                        AssignedAt = a.AssignmentTime
                    }).ToList(),
                    TotalOfficers = assignments.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בשליפת פרטי הקריאה: {ex.Message}");
            }
        }

        [HttpPost("assign-officer")]
        public IActionResult AssignOfficerToCall([FromBody] AssignOfficerRequestDTO request)
        {
            try
            {
                var assignment = new CallAssignmentDTO
                {
                    CallId = request.CallId,
                    PoliceOfficerId = request.OfficerId,
                    AssignmentTime = DateTime.UtcNow
                };

                _callAssignmentService.AssignOfficersToCall(new List<CallAssignmentDTO> { assignment });

                return Ok(new
                {
                    Message = $"שוטר {request.OfficerId} שויך בהצלחה לקריאה {request.CallId}",
                    Assignment = new
                    {
                        CallId = assignment.CallId,
                        OfficerId = assignment.PoliceOfficerId,
                        AssignedAt = assignment.AssignmentTime
                    }
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בשיוך שוטר לקריאה: {ex.Message}");
            }
        }

        [HttpDelete("unassign-officer")]
        public IActionResult UnassignOfficerFromCall([FromBody] UnassignOfficerRequestDTO request)
        {
            try
            {
               
                return Ok(new
                {
                    Message = $"פונקציה זו דורשת הוספה של RemoveAssignment ב-CallAssignmentService",
                    CallId = request.CallId,
                    OfficerId = request.OfficerId
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בהסרת שיוך שוטר: {ex.Message}");
            }
        }

        [HttpGet("event/{eventId}/calls")]
        public IActionResult GetCallsByEvent(int eventId)
        {
            try
            {
                var calls = _callService.GetCallsByEvent(eventId);

                return Ok(new
                {
                    EventId = eventId,
                    Calls = calls.Select(c => new
                    {
                        CallId = c.CallId,
                        RequiredOfficers = c.RequiredOfficers,
                        ContactPhone = c.ContactPhone,
                        UrgencyLevel = c.UrgencyLevel,
                        CallTime = c.CallTime,
                        Status = c.Status,
                        Location = new
                        {
                            Latitude = c.Latitude,
                            Longitude = c.Longitude
                        }
                    }).ToList(),
                    TotalCalls = calls.Count
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה בשליפת קריאות: {ex.Message}");
            }
        }
    }
}