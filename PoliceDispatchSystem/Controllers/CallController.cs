// CallController.cs - מעודכן לשימוש בגרפים לפי מזהה אירוע
//using DTO;
//using IBL;
//using Microsoft.AspNetCore.Mvc;
//using PoliceDispatchSystem.Controllers;

//namespace PoliceDispatchSystem.API
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class CallController : ControllerBase
//    {
//        private readonly ICallService _callService;
//        private readonly ICallAssignmentService _callAssignmentService;
//        private readonly IOfficerAssignmentService _officerAssignmentService;
//        private readonly IKCenterService _kCenterService;

//        public CallController(ICallService callService, ICallAssignmentService callAssignmentService, IOfficerAssignmentService officerAssignmentService, IKCenterService kCenterService)
//        {
//            _callService = callService;
//            _callAssignmentService = callAssignmentService;
//            _officerAssignmentService = officerAssignmentService;
//            _kCenterService = kCenterService;
//        }

//        [HttpPost("create")]
//        public IActionResult CreateCall([FromBody] CallDTO callDto)
//        {
//            if (callDto.Latitude == 0 || callDto.Longitude == 0)
//                return BadRequest("יש להזין מיקום לקריאה (Latitude/Longitude).");

//            if (callDto.EventId == null)
//                return BadRequest("EventId נדרש לקריאה.");

//            var graphData = GraphController.GetGraphForEvent(callDto.EventId.Value);
//            if (graphData == null)
//                return BadRequest("לא קיים גרף עבור האירוע המבוקש.");

//            int callId = _callService.CreateCall(callDto);

//            var allEventOfficers = _officerAssignmentService.GetAssignmentsByEventId(callDto.EventId.Value);
//            var assignedToCall = new HashSet<int>(_callAssignmentService
//                .GetAssignmentsByCall(callId)
//                .Select(ca => ca.PoliceOfficerId));

//            var available = allEventOfficers
//                .Where(o => !assignedToCall.Contains(o.PoliceOfficerId))
//                .ToList();

//            var chosen = available
//                .OrderBy(o => GetDistance(o.Latitude, o.Longitude, callDto.Latitude, callDto.Longitude))
//                .Take(callDto.RequiredOfficers)
//                .ToList();

//            var callAssignments = chosen.Select(o => new CallAssignmentDTO
//            {
//                PoliceOfficerId = o.PoliceOfficerId,
//                CallId = callId
//            }).ToList();

//            _callAssignmentService.AssignOfficersToCall(callAssignments);

//            var remaining = available
//                .Where(o => !chosen.Any(c => c.PoliceOfficerId == o.PoliceOfficerId))
//                .ToList();

//            var graph = graphData.Graph;
//            var nodesInBounds = graphData.NodesInOriginalBounds
//                .Where(kvp => kvp.Value)
//                .Select(kvp => kvp.Key)
//                .ToHashSet();

//            var result = _kCenterService.DistributePolice(graph, remaining.Count, nodesInBounds);
//            var selectedNodes = result.CenterNodes;

//            var nodeToCoord = graphData.Nodes;
//            var reassigned = new List<OfficerAssignmentDTO>();

//            foreach (var nodeId in selectedNodes)
//            {
//                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
//                    continue;

//                var officer = remaining
//                    .Where(o => !reassigned.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
//                    .OrderBy(o => GetDistance(o.Latitude, o.Longitude, coord.lat, coord.lon))
//                    .FirstOrDefault();

//                if (officer != null)
//                {
//                    reassigned.Add(new OfficerAssignmentDTO
//                    {
//                        PoliceOfficerId = officer.PoliceOfficerId,
//                        EventId = callDto.EventId.Value,
//                        Latitude = coord.lat,
//                        Longitude = coord.lon
//                    });
//                }
//            }

//            _officerAssignmentService.AddOfficerAssignments(reassigned);

//            return Ok(new
//            {
//                CallId = callId,
//                Assigned = callAssignments.Count,
//                Reassigned = reassigned.Count
//            });
//        }

//        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
//        {
//            var R = 6371000;
//            var dLat = Math.PI / 180 * (lat2 - lat1);
//            var dLon = Math.PI / 180 * (lon2 - lon1);

//            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
//                    Math.Cos(Math.PI / 180 * lat1) * Math.Cos(Math.PI / 180 * lat2) *
//                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

//            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
//            return R * c;
//        }
//    }
//}
// CallController.cs - גרסה מתוקנת לחלוטין ללא קריאות לקונטרולרים
//using BLL;
//using DTO;
//using IBL;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;

//namespace PoliceDispatchSystem.API
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class CallController : ControllerBase
//    {
//        private readonly ICallService _callService;
//        private readonly ICallAssignmentService _callAssignmentService;
//        private readonly IOfficerAssignmentService _officerAssignmentService;
//        private readonly IKCenterService _kCenterService;
//        private readonly IGraphManagerService _graphManager;

//        public CallController(
//            ICallService callService,
//            ICallAssignmentService callAssignmentService,
//            IOfficerAssignmentService officerAssignmentService,
//            IKCenterService kCenterService,
//            IGraphManagerService graphManager)
//        {
//            _callService = callService;
//            _callAssignmentService = callAssignmentService;
//            _officerAssignmentService = officerAssignmentService;
//            _kCenterService = kCenterService;
//            _graphManager = graphManager;
//        }

//        [HttpPost("create")]
//        public IActionResult CreateCall([FromBody] CallDTO callDto)
//        {
//            if (callDto.Latitude == 0 || callDto.Longitude == 0)
//                return BadRequest("יש להזין מיקום לקריאה (Latitude/Longitude).");

//            if (callDto.EventId == null)
//                return BadRequest("EventId נדרש לקריאה.");

//            try
//            {
//                // שליפת גרף האירוע דרך GraphManager במקום קריאה ישירה לקונטרולר
//                var graphData = _graphManager.GetGraphForEvent(callDto.EventId.Value);
//                if (graphData == null)
//                    return BadRequest("לא קיים גרף עבור האירוע המבוקש.");

//                // יצירת הקריאה
//                int callId = _callService.CreateCall(callDto);

//                // שליפת כל השוטרים שמשויכים לאירוע
//                var allEventOfficers = _officerAssignmentService.GetAssignmentsByEventId(callDto.EventId.Value);

//                // שליפת השוטרים שכבר משויכים לקריאה זו
//                var assignedToCall = new HashSet<int>(_callAssignmentService
//                    .GetAssignmentsByCall(callId)
//                    .Select(ca => ca.PoliceOfficerId));

//                // שוטרים זמינים = כל השוטרים של האירוע פחות אלה שכבר משויכים לקריאה
//                var available = allEventOfficers
//                    .Where(o => !assignedToCall.Contains(o.PoliceOfficerId))
//                    .ToList();

//                Console.WriteLine($"📞 קריאה חדשה: זמינים {available.Count} שוטרים, דרושים {callDto.RequiredOfficers}");

//                // בחירת השוטרים הקרובים ביותר לקריאה
//                var chosen = available
//                    .OrderBy(o => GetDistance(o.Latitude, o.Longitude, callDto.Latitude, callDto.Longitude))
//                    .Take(callDto.RequiredOfficers)
//                    .ToList();

//                // שיוך השוטרים לקריאה
//                var callAssignments = chosen.Select(o => new CallAssignmentDTO
//                {
//                    PoliceOfficerId = o.PoliceOfficerId,
//                    CallId = callId
//                }).ToList();

//                _callAssignmentService.AssignOfficersToCall(callAssignments);

//                Console.WriteLine($"✅ שויכו {chosen.Count} שוטרים לקריאה {callId}");

//                // השוטרים שנשארו זמינים (לא שויכו לקריאה זו)
//                var remaining = available
//                    .Where(o => !chosen.Any(c => c.PoliceOfficerId == o.PoliceOfficerId))
//                    .ToList();

//                Console.WriteLine($"🔄 מפזר מחדש {remaining.Count} שוטרים נותרים");

//                // פיזור מחדש של השוטרים הנותרים באמצעות K-Center
//                if (remaining.Any())
//                {
//                    var graph = graphData.Graph;
//                    var nodesInBounds = graphData.NodesInOriginalBounds
//                        .Where(kvp => kvp.Value)
//                        .Select(kvp => kvp.Key)
//                        .ToHashSet();

//                    // הרצת אלגוריתם K-Center עבור השוטרים הנותרים
//                    var result = _kCenterService.DistributePolice(graph, remaining.Count, nodesInBounds);
//                    var selectedNodes = result.CenterNodes;

//                    var nodeToCoord = graphData.Nodes;
//                    var reassigned = new List<OfficerAssignmentDTO>();

//                    // שיוך כל שוטר נותר לצומת הקרוב ביותר מהפתרון
//                    foreach (var nodeId in selectedNodes)
//                    {
//                        if (!nodeToCoord.TryGetValue(nodeId, out var coord))
//                            continue;

//                        // מציאת השוטר הקרוב ביותר שעדיין לא שובץ
//                        var officer = remaining
//                            .Where(o => !reassigned.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
//                            .OrderBy(o => GetDistance(o.Latitude, o.Longitude, coord.lat, coord.lon))
//                            .FirstOrDefault();

//                        if (officer != null)
//                        {
//                            reassigned.Add(new OfficerAssignmentDTO
//                            {
//                                PoliceOfficerId = officer.PoliceOfficerId,
//                                EventId = callDto.EventId.Value,
//                                Latitude = coord.lat,
//                                Longitude = coord.lon
//                            });

//                            Console.WriteLine($"👮 שוטר {officer.PoliceOfficerId} הועבר למיקום ({coord.lat:F6}, {coord.lon:F6})");
//                        }
//                    }

//                    // עדכון מיקומי השוטרים במסד הנתונים
//                    if (reassigned.Any())
//                    {
//                        _officerAssignmentService.AddOfficerAssignments(reassigned);
//                        Console.WriteLine($"✅ עודכנו מיקומים של {reassigned.Count} שוטרים");
//                    }

//                    return Ok(new
//                    {
//                        CallId = callId,
//                        AssignedToCall = callAssignments.Count,
//                        ReassignedOfficers = reassigned.Count,
//                        TotalAvailableOfficers = available.Count,
//                        Message = $"קריאה נוצרה בהצלחה. שויכו {callAssignments.Count} שוטרים לקריאה ו-{reassigned.Count} שוטרים פוזרו מחדש.",
//                        CallInfo = new
//                        {
//                            Id = callId,
//                            CallLocation = new { Latitude = callDto.Latitude, Longitude = callDto.Longitude },
//                            RequiredOfficers = callDto.RequiredOfficers,
//                            ActualAssigned = callAssignments.Count
//                        },
//                        AssignedOfficersList = chosen.Select(o => new
//                        {
//                            OfficerId = o.PoliceOfficerId,
//                            OfficerLocation = new { Latitude = o.Latitude, Longitude = o.Longitude },
//                            DistanceToCall = GetDistance(o.Latitude, o.Longitude, callDto.Latitude, callDto.Longitude)
//                        }).ToList(),
//                        ReassignedOfficersList = reassigned.Select(r => new
//                        {
//                            OfficerId = r.PoliceOfficerId,
//                            NewOfficerLocation = new { Latitude = r.Latitude, Longitude = r.Longitude }
//                        }).ToList()
//                    });
//                }
//                else
//                {
//                    // אין שוטרים נותרים לפיזור מחדש
//                    return Ok(new
//                    {
//                        CallId = callId,
//                        AssignedToCall = callAssignments.Count,
//                        ReassignedOfficersCount = 0,
//                        TotalAvailableOfficers = available.Count,
//                        Message = $"קריאה נוצרה בהצלחה. שויכו {callAssignments.Count} שוטרים לקריאה.",
//                        CallInfo = new
//                        {
//                            Id = callId,
//                            CallLocation = new { Latitude = callDto.Latitude, Longitude = callDto.Longitude },
//                            RequiredOfficers = callDto.RequiredOfficers,
//                            ActualAssigned = callAssignments.Count
//                        },
//                        AssignedOfficersList = chosen.Select(o => new
//                        {
//                            OfficerId = o.PoliceOfficerId,
//                            OfficerLocation = new { Latitude = o.Latitude, Longitude = o.Longitude },
//                            DistanceToCall = GetDistance(o.Latitude, o.Longitude, callDto.Latitude, callDto.Longitude)
//                        }).ToList()
//                    });
//                }
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ שגיאה ביצירת קריאה: {ex.Message}");
//                return BadRequest($"שגיאה ביצירת הקריאה: {ex.Message}");
//            }
//        }
//[HttpGet("call/{callId}")]
//public IActionResult GetCallDetails(int callId)
//{
//    try
//    {
//        // משתמש בפונקציה קיימת במקום GetCallById שלא קיימת
//        var assignments = _callAssignmentService.GetAssignmentsByCall(callId);

//        return Ok(new
//        {
//            CallId = callId,
//            AssignedOfficers = assignments.Select(a => new
//            {
//                OfficerId = a.PoliceOfficerId,
//                CallId = a.CallId
//                // הסרתי AssignedAt כי לא קיים ב-DTO
//            }).ToList(),
//            TotalOfficers = assignments.Count
//        });
//    }
//    catch (Exception ex)
//    {
//        return BadRequest($"שגיאה בשליפת פרטי הקריאה: {ex.Message}");
//    }
//}

//[HttpPost("assign-officer")]
//public IActionResult AssignOfficerToCall([FromBody] AssignOfficerRequest request)
//{
//    try
//    {
//        var assignment = new CallAssignmentDTO
//        {
//            CallId = request.CallId,
//            PoliceOfficerId = request.OfficerId
//        };

//        _callAssignmentService.AssignOfficersToCall(new List<CallAssignmentDTO> { assignment });

//        return Ok(new
//        {
//            Message = $"שוטר {request.OfficerId} שויך בהצלחה לקריאה {request.CallId}",
//            Assignment = assignment
//        });
//    }
//    catch (Exception ex)
//    {
//        return BadRequest($"שגיאה בשיוך שוטר לקריאה: {ex.Message}");
//    }
//}

//[HttpDelete("unassign-officer")]
//public IActionResult UnassignOfficerFromCall([FromBody] UnassignOfficerRequest request)
//{
//    try
//    {
//        // כאן תצטרכי להוסיף פונקציה למחיקת שיוך ב-CallAssignmentService
//        // _callAssignmentService.RemoveAssignment(request.CallId, request.OfficerId);

//        return Ok(new
//        {
//            Message = $"שוטר {request.OfficerId} הוסר מקריאה {request.CallId}"
//        });
//    }
//    catch (Exception ex)
//    {
//        return BadRequest($"שגיאה בהסרת שיוך שוטר: {ex.Message}");
//    }
//}

//[HttpGet("event/{eventId}/calls")]
//public IActionResult GetCallsByEvent(int eventId)
//{
//    try
//    {
//        // כאן תצטרכי להוסיף פונקציה ב-CallService לשליפת קריאות לפי אירוע
//        // var calls = _callService.GetCallsByEventId(eventId);

//        return Ok(new
//        {
//            EventId = eventId,
//            Message = "פונקציה זו דורשת הוספה ב-CallService"
//            // Calls = calls
//        });
//    }
//    catch (Exception ex)
//    {
//        return BadRequest($"שגיאה בשליפת קריאות: {ex.Message}");
//    }
//}

///// <summary>
///// חישוב מרחק בין שתי נקודות גיאוגרפיות באמצעות נוסחת Haversine
///// </summary>
//private double GetDistance(double lat1, double lon1, double lat2, double lon2)
//{
//    var R = 6371000; // רדיוס כדור הארץ במטרים
//    var dLat = Math.PI / 180 * (lat2 - lat1);
//    var dLon = Math.PI / 180 * (lon2 - lon1);

//    var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
//            Math.Cos(Math.PI / 180 * lat1) * Math.Cos(Math.PI / 180 * lat2) *
//            Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

//    var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
//    return R * c;
//}
//    }

//    //// מחלקות Request
//    //public class AssignOfficerRequest
//    //{
//    //    public int CallId { get; set; }
//    //    public int OfficerId { get; set; }
//    //}

//    //public class UnassignOfficerRequest
//    //{
//    //    public int CallId { get; set; }
//    //    public int OfficerId { get; set; }
//    //}
//}

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
                Console.WriteLine($"❌ שגיאה ביצירת קריאה: {ex.Message}");
                return BadRequest($"שגיאה ביצירת הקריאה: {ex.Message}");
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
                // כאן תצטרכי להוסיף פונקציה למחיקת שיוך ב-CallAssignmentService
                // אבל זמנית נחזיר הודעה
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