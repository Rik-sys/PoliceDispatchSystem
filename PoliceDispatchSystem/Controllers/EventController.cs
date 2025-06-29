
using Microsoft.AspNetCore.Mvc;
using DTO;
using IBL;
using Microsoft.Extensions.Logging;
using static DTO.EventRequestsDTO;

namespace PoliceDispatchSystem.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventManagementService _eventManagementService;
        private readonly IEventService _eventService;
        private readonly ILogger<EventController> _logger;

        public EventController(
            IEventManagementService eventManagementService,
            IEventService eventService,
            ILogger<EventController> logger)
        {
            _eventManagementService = eventManagementService;
            _eventService = eventService;
            _logger = logger;
        }

        /// <summary>
        /// יוצר אירוע חדש עם פיזור אוטומטי של שוטרים
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequestDTO request)
        {
            try
            {
                _logger.LogInformation($"Creating event: {request.Name}");

                var result = await _eventManagementService.CreateEventWithAutoDistribution(request);

                if (!result.Success)
                {
                    _logger.LogWarning($"Event creation failed: {string.Join(", ", result.Errors)}");
                    return BadRequest(new
                    {
                        Errors = result.Errors,
                        Message = "יצירת האירוע נכשלה"
                    });
                }

                _logger.LogInformation($"Event {result.EventId} created successfully");

                return Ok(new
                {
                    EventId = result.EventId,
                    OfficerCount = result.OfficerCount,
                    StrategicOfficers = result.StrategicOfficers,
                    RegularOfficers = result.RegularOfficers,
                    NodesCreatedOnRealRoads = result.NodesCreatedOnRealRoads,
                    Message = result.Message,
                    DebugInfo = result.DebugInfo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateEvent endpoint");
                return StatusCode(500, new { Message = "שגיאה פנימית בשרת" });
            }
        }

        /// <summary>
        /// יוצר אירוע עם מיקומים מחושבים מראש
        /// </summary>
        [HttpPost("create-with-positions")]
        public async Task<IActionResult> CreateEventWithPositions([FromBody] CreateEventWithPositionsRequestDTO request)
        {
            try
            {
                _logger.LogInformation($"Creating event with pre-calculated positions: {request.Name}");

                var result = await _eventManagementService.CreateEventWithPreCalculatedPositions(request);

                if (!result.Success)
                {
                    _logger.LogWarning($"Event creation with positions failed: {string.Join(", ", result.Errors)}");
                    return BadRequest(new
                    {
                        Errors = result.Errors,
                        Message = "יצירת האירוע נכשלה"
                    });
                }

                _logger.LogInformation($"Event {result.EventId} created successfully with pre-calculated positions");

                return Ok(new
                {
                    EventId = result.EventId,
                    OfficerCount = result.OfficerCount,
                    StrategicOfficers = result.StrategicOfficers,
                    RegularOfficers = result.RegularOfficers,
                    Message = result.Message
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in CreateEventWithPositions endpoint");
                return StatusCode(500, new { Message = "שגיאה פנימית בשרת" });
            }
        }

        /// <summary>
        /// מוחק אירוע ומנקה את כל הקשרים הקשורים
        /// </summary>
        [HttpDelete("{eventId}")]
        public async Task<IActionResult> DeleteEvent(int eventId)
        {
            try
            {
                _logger.LogInformation($"Deleting event {eventId}");

                var success = await _eventManagementService.DeleteEventComplete(eventId);

                if (!success)
                {
                    _logger.LogWarning($"Failed to delete event {eventId}");
                    return BadRequest(new { Message = "שגיאה במחיקת האירוע" });
                }

                _logger.LogInformation($"Event {eventId} deleted successfully");
                return Ok(new { Message = "האירוע נמחק בהצלחה" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting event {eventId}");
                return StatusCode(500, new { Message = "שגיאה פנימית בשרת" });
            }
        }

        /// <summary>
        /// מחזיר את כל האירועים
        /// </summary>
        [HttpGet("allEvents")]
        public IActionResult GetAllEvents()
        {
            try
            {
                var allEvents = _eventService.GetEvents();
                return Ok(allEvents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all events");
                return StatusCode(500, new { Message = "שגיאה בשליפת האירועים" });
            }
        }

        /// <summary>
        /// מחזיר את כל האירועים עם פרטים מלאים
        /// </summary>
        [HttpGet("allEventsWithDetails")]
        public IActionResult GetAllEventsWithDetails()
        {
            try
            {
                var eventsWithDetails = _eventManagementService.GetAllEventsWithDetails();
                return Ok(eventsWithDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all events with details");
                return StatusCode(500, new { Message = "שגיאה בשליפת האירועים" });
            }
        }

        /// <summary>
        /// מחזיר אירוע עם כל הפרטים הקשורים
        /// </summary>
        [HttpGet("{eventId}/details")]
        public IActionResult GetEventWithDetails(int eventId)
        {
            try
            {
                var eventWithDetails = _eventManagementService.GetEventWithDetails(eventId);
                if (eventWithDetails?.Event == null)
                {
                    return NotFound(new { Message = $"לא נמצא אירוע עם מזהה {eventId}" });
                }

                return Ok(eventWithDetails);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting event {eventId} with details");
                return StatusCode(500, new { Message = "שגיאה בשליפת פרטי האירוע" });
            }
        }
        //  endpoint לבדיקה מהירה
        [HttpGet("debug/test-officers")]
        public IActionResult TestOfficersAvailability()
        {
            try
            {
                var testDate = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
                var startTime = TimeOnly.Parse("10:00");
                var endTime = TimeOnly.Parse("18:00");

                _logger.LogInformation($" Testing officer availability for {testDate} {startTime}-{endTime}");

                // בדיקה ישירה של מספר שוטרים במסד
                // צריך גישה ל-DAL - אבל בינתיים נבדוק דרך השירות
                var availableOfficers = _eventService.GetAvailableOfficersForEvent(testDate, startTime, endTime);

                return Ok(new
                {
                    TestDate = testDate.ToString(),
                    StartTime = startTime.ToString(),
                    EndTime = endTime.ToString(),
                    AvailableOfficersCount = availableOfficers.Count,
                    Officers = availableOfficers.Take(5).Select(o => new
                    {
                        o.PoliceOfficerId,
                        Username = o.User?.Username ?? "No Username",
                        o.VehicleTypeId
                    }).ToList(),
                    Message = availableOfficers.Count == 0 ? " No officers found!" : $"Found {availableOfficers.Count} officers"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in test officers endpoint");
                return StatusCode(500, new
                {
                    Error = ex.Message,
                    StackTrace = ex.StackTrace?.Split('\n').Take(5).ToArray()
                });
            }
        }
        /// <summary>
        /// מחזיר את האזור של אירוע
        /// </summary>
        [HttpGet("{eventId}/zone")]
        public IActionResult GetZoneForEvent(int eventId)
        {
            try
            {
                var zone = _eventService.GetEventZoneByEventId(eventId);
                if (zone == null)
                {
                    return NotFound(new { Message = $"לא נמצא אזור לאירוע {eventId}" });
                }

                return Ok(zone);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting zone for event {eventId}");
                return StatusCode(500, new { Message = "שגיאה בשליפת אזור האירוע" });
            }
        }

        /// <summary>
        /// מחזיר את כל האזורים של כל האירועים
        /// </summary>
        [HttpGet("allZones")]
        public IActionResult GetAllEventZones()
        {
            try
            {
                var allZones = _eventService.GetAllEventZones();
                return Ok(allZones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all event zones");
                return StatusCode(500, new { Message = "שגיאה בשליפת האזורים" });
            }
        }

        /// <summary>
        /// בודק אם שוטר זמין לאירוע
        /// </summary>
        [HttpGet("officer/{officerId}/availability")]
        public IActionResult CheckOfficerAvailability(int officerId, [FromQuery] string date, [FromQuery] string startTime, [FromQuery] string endTime)
        {
            try
            {
                if (!DateOnly.TryParse(date, out var eventDate) ||
                    !TimeOnly.TryParse(startTime, out var start) ||
                    !TimeOnly.TryParse(endTime, out var end))
                {
                    return BadRequest(new { Message = "פורמט תאריך או שעה לא תקין" });
                }

                var isAvailable = _eventManagementService.IsOfficerAvailableForEvent(officerId, eventDate, start, end);
                return Ok(new
                {
                    OfficerId = officerId,
                    IsAvailable = isAvailable,
                    Date = date,
                    StartTime = startTime,
                    EndTime = endTime
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking officer {officerId} availability");
                return StatusCode(500, new { Message = "שגיאה בבדיקת זמינות השוטר" });
            }
        }
    }
}