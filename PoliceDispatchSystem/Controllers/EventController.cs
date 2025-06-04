//using Microsoft.AspNetCore.Mvc;
//using DTO;
//using DBEntities.Models;
//using IBL;
//using PoliceDispatchSystem.Controllers;

//namespace PoliceDispatchSystem.API
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class EventController : ControllerBase
//    {
//        private readonly IEventService _eventService;
//        private readonly IKCenterService _kCenterService;
//        private readonly PoliceDispatchSystemContext _context;

//        public EventController(IEventService eventService, IKCenterService kCenterService, PoliceDispatchSystemContext context)
//        {
//            _eventService = eventService;
//            _kCenterService = kCenterService;
//            _context = context;
//        }

//        [HttpPost("create")]
//        public IActionResult CreateEvent([FromBody] CreateEventRequest request)
//        {
//            if (GraphController.LatestGraph == null || GraphController.LatestNodes == null)
//                return BadRequest("אין גרף טעון במערכת.");

//            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
//                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע.");

//            // 1. המרת הבקשה ל־DTO
//            var eventDto = new EventDTO
//            {
//                EventName = request.Name,
//                Description = request.Description,
//                Priority = request.Priority,
//                EventDate = DateOnly.FromDateTime(DateTime.Parse(request.StartDate)),
//                StartTime = TimeOnly.Parse(request.StartTime),
//                EndTime = TimeOnly.Parse(request.EndTime),
//                RequiredOfficers = request.RequiredOfficers
//            };

//            var zoneDto = new EventZoneDTO
//            {
//                Latitude1 = request.SelectedArea[0][0],
//                Longitude1 = request.SelectedArea[0][1],
//                Latitude2 = request.SelectedArea[1][0],
//                Longitude2 = request.SelectedArea[1][1],
//                Latitude3 = request.SelectedArea[2][0],
//                Longitude3 = request.SelectedArea[2][1],
//                Latitude4 = request.SelectedArea[3][0],
//                Longitude4 = request.SelectedArea[3][1]
//            };

//            // 2. שמירה במסד
//            int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

//            // 3. סינון צמתים בתחום
//            var nodesInBounds = GraphController.NodesInOriginalBounds
//                .Where(kvp => kvp.Value == true)
//                .Select(kvp => kvp.Key)
//                .ToHashSet();

//            // 4. פיזור K-Center
//            var result = _kCenterService.DistributePolice(GraphController.LatestGraph, request.RequiredOfficers, nodesInBounds);
//            var selectedNodeIds = result.CenterNodes;

//            var nodeToCoord = GraphController.LatestNodes;

//            // 5. שליפת כל השוטרים הפנויים (נניח כולם זמינים לצורך הפשטות)
//            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
//              eventDto.EventDate,
//              eventDto.StartTime,
//              eventDto.EndTime
//);


//            var assignedOfficers = new List<OfficerAssignment>();

//            foreach (var nodeId in selectedNodeIds)
//            {
//                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
//                    continue;

//                // מציאת השוטר הכי קרוב שלא שובץ עדיין
//                var availableOfficer = availableOfficers
//                .Where(o => !assignedOfficers.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
//                .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
//                .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
//                .FirstOrDefault();


//                if (availableOfficer != null)
//                {
//                    assignedOfficers.Add(new OfficerAssignment
//                    {
//                        PoliceOfficerId = availableOfficer.PoliceOfficerId,
//                        EventId = eventId,
//                        Latitude = coord.lat,
//                        Longitude = coord.lon
//                    });
//                }
//            }

//            _context.OfficerAssignments.AddRange(assignedOfficers);
//            _context.SaveChanges();

//            return Ok(new
//            {
//                EventId = eventId,
//                OfficerCount = assignedOfficers.Count,
//                Message = "אירוע נוצר בהצלחה ושובצו שוטרים"
//            });
//        }

//        private double GetDistanceFromOfficer(PoliceOfficer officer, double lat, double lon)
//        {            
//            return 0;
//        }
//    }

//    public class CreateEventRequest
//    {
//        public string Name { get; set; } = "";
//        public string Description { get; set; } = "";
//        public string Priority { get; set; } = "";
//        public string StartDate { get; set; } = "";
//        public string EndDate { get; set; } = "";
//        public string StartTime { get; set; } = "";
//        public string EndTime { get; set; } = "";
//        public int RequiredOfficers { get; set; }

//        public List<List<double>> SelectedArea { get; set; } = new(); // 4 נקודות [lat, lon]
//    }
//}


//אחרון
//using Microsoft.AspNetCore.Mvc;
//using DTO;
//using DBEntities.Models;
//using IBL;
//using PoliceDispatchSystem.Controllers;

//namespace PoliceDispatchSystem.API
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class EventController : ControllerBase
//    {
//        private readonly IEventService _eventService;
//        private readonly IKCenterService _kCenterService;
//        private readonly IOfficerAssignmentService _officerAssignmentService; // הוספה חדשה

//        public EventController(
//            IEventService eventService,
//            IKCenterService kCenterService,
//            IOfficerAssignmentService officerAssignmentService) // הוספה חדשה
//        {
//            _eventService = eventService;
//            _kCenterService = kCenterService;
//            _officerAssignmentService = officerAssignmentService; // הוספה חדשה
//        }

//        [HttpPost("create")]
//        public IActionResult CreateEvent([FromBody] CreateEventRequest request)
//        {
//            if (GraphController.LatestGraph == null || GraphController.LatestNodes == null)
//                return BadRequest("אין גרף טעון במערכת.");

//            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
//                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע.");

//            // 1. המרת הבקשה ל־DTO
//            var eventDto = new EventDTO
//            {
//                EventName = request.Name,
//                Description = request.Description,
//                Priority = request.Priority,
//                EventDate = DateOnly.FromDateTime(DateTime.Parse(request.StartDate)),
//                StartTime = TimeOnly.Parse(request.StartTime),
//                EndTime = TimeOnly.Parse(request.EndTime),
//                RequiredOfficers = request.RequiredOfficers
//            };

//            var zoneDto = new EventZoneDTO
//            {
//                Latitude1 = request.SelectedArea[0][0],
//                Longitude1 = request.SelectedArea[0][1],
//                Latitude2 = request.SelectedArea[1][0],
//                Longitude2 = request.SelectedArea[1][1],
//                Latitude3 = request.SelectedArea[2][0],
//                Longitude3 = request.SelectedArea[2][1],
//                Latitude4 = request.SelectedArea[3][0],
//                Longitude4 = request.SelectedArea[3][1]
//            };

//            // 2. שמירה במסד
//            int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

//            // 3. סינון צמתים בתחום
//            var nodesInBounds = GraphController.NodesInOriginalBounds
//                .Where(kvp => kvp.Value == true)
//                .Select(kvp => kvp.Key)
//                .ToHashSet();

//            // 4. פיזור K-Center
//            var result = _kCenterService.DistributePolice(GraphController.LatestGraph, request.RequiredOfficers, nodesInBounds);
//            var selectedNodeIds = result.CenterNodes;

//            var nodeToCoord = GraphController.LatestNodes;

//            // 5. שליפת כל השוטרים הפנויים (עכשיו מחזיר DTO)
//            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
//                eventDto.EventDate,
//                eventDto.StartTime,
//                eventDto.EndTime
//            );

//            // 6. יצירת רשימת DTOs לשיוכים במקום עבודה ישירה על Entity
//            var assignmentDtos = new List<OfficerAssignmentDTO>();

//            foreach (var nodeId in selectedNodeIds)
//            {
//                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
//                    continue;

//                // מציאת השוטר הכי קרוב שלא שובץ עדיין
//                var availableOfficer = availableOfficers
//                    .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
//                    .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
//                    .FirstOrDefault();

//                if (availableOfficer != null)
//                {
//                    // יצירת DTO במקום Entity
//                    assignmentDtos.Add(new OfficerAssignmentDTO
//                    {
//                        PoliceOfficerId = availableOfficer.PoliceOfficerId,
//                        EventId = eventId,
//                        Latitude = coord.lat,
//                        Longitude = coord.lon
//                    });
//                }
//            }

//            // 7. שמירת השיוכים דרך שכבת השירות (BLL)
//            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

//            return Ok(new
//            {
//                EventId = eventId,
//                OfficerCount = assignmentDtos.Count,
//                Message = "אירוע נוצר בהצלחה ושובצו שוטרים"
//            });
//        }

//        private double GetDistanceFromOfficer(PoliceOfficerDTO officer, double lat, double lon)
//        {
//            return 0; // כאן תצטרך להוסיף חישוב מרחק אמיתי
//        }
//    }

//    public class CreateEventRequest
//    {
//        public string Name { get; set; } = "";
//        public string Description { get; set; } = "";
//        public string Priority { get; set; } = "";
//        public string StartDate { get; set; } = "";
//        public string EndDate { get; set; } = "";
//        public string StartTime { get; set; } = "";
//        public string EndTime { get; set; } = "";
//        public int RequiredOfficers { get; set; }

//        public List<List<double>> SelectedArea { get; set; } = new(); // 4 נקודות [lat, lon]
//    }
//}

//בלי אזורים אסטרטגיים
//using Microsoft.AspNetCore.Mvc;
//using DTO;
//using DBEntities.Models;
//using IBL;
//using PoliceDispatchSystem.Controllers;
//using BLL;

//namespace PoliceDispatchSystem.API
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class EventController : ControllerBase
//    {
//        private readonly IEventService _eventService;
//        private readonly IKCenterService _kCenterService;
//        private readonly IOfficerAssignmentService _officerAssignmentService;

//        public EventController(
//            IEventService eventService,
//            IKCenterService kCenterService,
//            IOfficerAssignmentService officerAssignmentService)
//        {
//            _eventService = eventService;
//            _kCenterService = kCenterService;
//            _officerAssignmentService = officerAssignmentService;
//        }

//        [HttpPost("create")]
//        public IActionResult CreateEvent([FromBody] CreateEventRequest request)
//        {
//            if (GraphController.LatestGraph == null || GraphController.LatestNodes == null)
//                return BadRequest("אין גרף טעון במערכת.");

//            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
//                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע.");

//            // 1. המרת הבקשה ל־DTO
//            var eventDto = new EventDTO
//            {
//                EventName = request.Name,
//                Description = request.Description,
//                Priority = request.Priority,
//                EventDate = DateOnly.FromDateTime(DateTime.Parse(request.StartDate)),
//                StartTime = TimeOnly.Parse(request.StartTime),
//                EndTime = TimeOnly.Parse(request.EndTime),
//                RequiredOfficers = request.RequiredOfficers
//            };

//            var zoneDto = new EventZoneDTO
//            {
//                Latitude1 = request.SelectedArea[0][0],
//                Longitude1 = request.SelectedArea[0][1],
//                Latitude2 = request.SelectedArea[1][0],
//                Longitude2 = request.SelectedArea[1][1],
//                Latitude3 = request.SelectedArea[2][0],
//                Longitude3 = request.SelectedArea[2][1],
//                Latitude4 = request.SelectedArea[3][0],
//                Longitude4 = request.SelectedArea[3][1]
//            };

//            // 2. שמירה במסד
//            int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

//            // 3. שמירת הגרף והצמתים לפי מזהה האירוע
//            GraphController.SaveGraphForEvent(eventId, GraphController.LatestGraph, GraphController.LatestNodes, GraphController.NodesInOriginalBounds);

//            // 4. סינון צמתים בתחום
//            var nodesInBounds = GraphController.NodesInOriginalBounds
//                .Where(kvp => kvp.Value == true)
//                .Select(kvp => kvp.Key)
//                .ToHashSet();

//            // 5. פיזור K-Center
//            var result = _kCenterService.DistributePolice(GraphController.LatestGraph, request.RequiredOfficers, nodesInBounds);
//            var selectedNodeIds = result.CenterNodes;

//            var nodeToCoord = GraphController.LatestNodes;

//            // 6. שליפת כל השוטרים הפנויים
//            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
//                eventDto.EventDate,
//                eventDto.StartTime,
//                eventDto.EndTime
//            );

//            // 7. יצירת רשימת DTOs לשיוכים
//            var assignmentDtos = new List<OfficerAssignmentDTO>();

//            foreach (var nodeId in selectedNodeIds)
//            {
//                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
//                    continue;

//                var availableOfficer = availableOfficers
//                    .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
//                    .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
//                    .FirstOrDefault();

//                if (availableOfficer != null)
//                {
//                    assignmentDtos.Add(new OfficerAssignmentDTO
//                    {
//                        PoliceOfficerId = availableOfficer.PoliceOfficerId,
//                        EventId = eventId,
//                        Latitude = coord.lat,
//                        Longitude = coord.lon
//                    });
//                }
//            }

//            // 8. שמירת השיוכים דרך שכבת השירות
//            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

//            return Ok(new
//            {
//                EventId = eventId,
//                OfficerCount = assignmentDtos.Count,
//                Message = "אירוע נוצר בהצלחה ושובצו שוטרים"
//            });
//        }

//        [HttpDelete("{eventId}")]
//        public IActionResult DeleteEvent(int eventId)
//        {
//            try
//            {
//                // מחיקת האירוע מהמסד
//                _eventService.DeleteEvent(eventId);

//                // מחיקת הגרף השמור עבור האירוע
//                GraphController.RemoveGraphForEvent(eventId);

//                return Ok(new { Message = "האירוע נמחק בהצלחה" });
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה במחיקת האירוע: {ex.Message}");
//            }
//        }

//        private double GetDistanceFromOfficer(PoliceOfficerDTO officer, double lat, double lon)
//        {
//            return 0; // כאן תצטרך להוסיף חישוב מרחק אמיתי
//        }

//        [HttpGet("allEvents")]
//        public IActionResult GetAllEvents()
//        {
//            var allEvents = _eventService.GetEvents();
//            return Ok(allEvents);
//    }
//    }




//    public class CreateEventRequest
//    {
//        public string Name { get; set; } = "";
//        public string Description { get; set; } = "";
//        public string Priority { get; set; } = "";
//        public string StartDate { get; set; } = "";
//        public string EndDate { get; set; } = "";
//        public string StartTime { get; set; } = "";
//        public string EndTime { get; set; } = "";
//        public int RequiredOfficers { get; set; }
//        public List<List<double>> SelectedArea { get; set; } = new();
//    }
//}

//ניסוי קלוד
//using Microsoft.AspNetCore.Mvc;
//using DTO;
//using DBEntities.Models;
//using IBL;
//using PoliceDispatchSystem.Controllers;
//using BLL;

//namespace PoliceDispatchSystem.API
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class EventController : ControllerBase
//    {
//        private readonly IEventService _eventService;
//        private readonly IKCenterService _kCenterService;
//        private readonly IOfficerAssignmentService _officerAssignmentService;
//        private readonly IStrategicZoneBL _strategicZoneBL;

//        public EventController(
//            IEventService eventService,
//            IKCenterService kCenterService,
//            IOfficerAssignmentService officerAssignmentService,
//            IStrategicZoneBL strategicZoneBL)
//        {
//            _eventService = eventService;
//            _kCenterService = kCenterService;
//            _officerAssignmentService = officerAssignmentService;
//            _strategicZoneBL = strategicZoneBL;
//        }

//        [HttpPost("create")]
//        public IActionResult CreateEvent([FromBody] CreateEventRequest request)
//        {
//            if (GraphController.LatestGraph == null || GraphController.LatestNodes == null)
//                return BadRequest("אין גרף טעון במערכת.");

//            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
//                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע.");

//            // 1. המרת הבקשה ל־DTO
//            var eventDto = new EventDTO
//            {
//                EventName = request.Name,
//                Description = request.Description,
//                Priority = request.Priority,
//                EventDate = DateOnly.FromDateTime(DateTime.Parse(request.StartDate)),
//                StartTime = TimeOnly.Parse(request.StartTime),
//                EndTime = TimeOnly.Parse(request.EndTime),
//                RequiredOfficers = request.RequiredOfficers
//            };

//            var zoneDto = new EventZoneDTO
//            {
//                Latitude1 = request.SelectedArea[0][0],
//                Longitude1 = request.SelectedArea[0][1],
//                Latitude2 = request.SelectedArea[1][0],
//                Longitude2 = request.SelectedArea[1][1],
//                Latitude3 = request.SelectedArea[2][0],
//                Longitude3 = request.SelectedArea[2][1],
//                Latitude4 = request.SelectedArea[3][0],
//                Longitude4 = request.SelectedArea[3][1]
//            };

//            // 2. שמירה במסד
//            int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

//            // 3. שמירת הגרף והצמתים לפי מזהה האירוע
//            GraphController.SaveGraphForEvent(eventId, GraphController.LatestGraph, GraphController.LatestNodes, GraphController.NodesInOriginalBounds);

//            // 4. שמירת אזורים אסטרטגיים אם נשלחו
//            if (request.StrategicZones != null && request.StrategicZones.Any())
//            {
//                foreach (var zone in request.StrategicZones)
//                    zone.EventId = eventId;

//                _strategicZoneBL.AddStrategicZones(request.StrategicZones);
//            }

//            // 5. סינון צמתים בתחום
//            var nodesInBounds = GraphController.NodesInOriginalBounds
//                .Where(kvp => kvp.Value == true)
//                .Select(kvp => kvp.Key)
//                .ToHashSet();

//            // 6. המרת האזורים האסטרטגיים לצמתים קרובים
//            List<long> strategicNodeIds = request.StrategicZones?
//                .Select(z => FindClosestNode(GraphController.LatestGraph, z.Latitude, z.Longitude))
//                .Where(id => id != -1)
//                .Distinct()
//                .ToList() ?? new List<long>();

//            // 7. פיזור K-Center עם או בלי אזורים
//            var result = _kCenterService.DistributePolice(GraphController.LatestGraph, request.RequiredOfficers, nodesInBounds, strategicNodeIds);
//            var selectedNodeIds = result.CenterNodes;
//            var nodeToCoord = GraphController.LatestNodes;

//            // 8. שליפת כל השוטרים הפנויים
//            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
//                eventDto.EventDate,
//                eventDto.StartTime,
//                eventDto.EndTime
//            );

//            // 9. שיוך שוטרים לנקודות
//            var assignmentDtos = new List<OfficerAssignmentDTO>();

//            foreach (var nodeId in selectedNodeIds)
//            {
//                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
//                    continue;

//                var availableOfficer = availableOfficers
//                    .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
//                    .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
//                    .FirstOrDefault();

//                if (availableOfficer != null)
//                {
//                    assignmentDtos.Add(new OfficerAssignmentDTO
//                    {
//                        PoliceOfficerId = availableOfficer.PoliceOfficerId,
//                        EventId = eventId,
//                        Latitude = coord.lat,
//                        Longitude = coord.lon
//                    });
//                }
//            }

//            // 10. שמירה במסד
//            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

//            return Ok(new
//            {
//                EventId = eventId,
//                OfficerCount = assignmentDtos.Count,
//                Message = "אירוע נוצר בהצלחה ושובצו שוטרים"
//            });
//        }

//        [HttpDelete("{eventId}")]
//        public IActionResult DeleteEvent(int eventId)
//        {
//            try
//            {
//                _eventService.DeleteEvent(eventId);
//                GraphController.RemoveGraphForEvent(eventId);
//                return Ok(new { Message = "האירוע נמחק בהצלחה" });
//            }
//            catch (Exception ex)
//            {
//                return BadRequest($"שגיאה במחיקת האירוע: {ex.Message}");
//            }
//        }

//        [HttpGet("allEvents")]
//        public IActionResult GetAllEvents()
//        {
//            var allEvents = _eventService.GetEvents();
//            return Ok(allEvents);
//        }

//        private double GetDistanceFromOfficer(PoliceOfficerDTO officer, double lat, double lon)
//        {
//            // כאן תוכל להכניס חישוב Haversine אם תרצי
//            return 0;
//        }

//        private long FindClosestNode(Graph graph, double latitude, double longitude)
//        {
//            long closestNodeId = -1;
//            double minDistance = double.MaxValue;

//            foreach (var node in graph.Nodes.Values)
//            {
//                double distance = Math.Sqrt(
//                    Math.Pow(node.Latitude - latitude, 2) +
//                    Math.Pow(node.Longitude - longitude, 2)
//                );

//                if (distance < minDistance)
//                {
//                    minDistance = distance;
//                    closestNodeId = node.Id;
//                }
//            }

//            return closestNodeId;
//        }
//    }

//    public class CreateEventRequest
//    {
//        public string Name { get; set; } = "";
//        public string Description { get; set; } = "";
//        public string Priority { get; set; } = "";
//        public string StartDate { get; set; } = "";
//        public string EndDate { get; set; } = "";
//        public string StartTime { get; set; } = "";
//        public string EndTime { get; set; } = "";
//        public int RequiredOfficers { get; set; }
//        public List<List<double>> SelectedArea { get; set; } = new();
//        public List<StrategicZoneDTO> StrategicZones { get; set; } = new();
//    }
//}
using Microsoft.AspNetCore.Mvc;
using DTO;
using DBEntities.Models;
using IBL;
using PoliceDispatchSystem.Controllers;
using BLL;

namespace PoliceDispatchSystem.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IKCenterService _kCenterService;
        private readonly IOfficerAssignmentService _officerAssignmentService;
        private readonly IStrategicZoneBL _strategicZoneBL;

        public EventController(
            IEventService eventService,
            IKCenterService kCenterService,
            IOfficerAssignmentService officerAssignmentService,
            IStrategicZoneBL strategicZoneBL)
        {
            _eventService = eventService;
            _kCenterService = kCenterService;
            _officerAssignmentService = officerAssignmentService;
            _strategicZoneBL = strategicZoneBL;
        }

        [HttpPost("create")]
        public IActionResult CreateEvent([FromBody] CreateEventRequest request)
        {
            if (GraphController.LatestGraph == null || GraphController.LatestNodes == null)
                return BadRequest("אין גרף טעון במערכת.");

            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע.");

            // בדיקה שמספר האזורים האסטרטגיים לא עולה על מספר השוטרים
            if (request.StrategicZones != null && request.StrategicZones.Count > request.RequiredOfficers)
                return BadRequest($"לא ניתן להציב {request.StrategicZones.Count} אזורים אסטרטגיים עם {request.RequiredOfficers} שוטרים בלבד.");

            // 1. המרת הבקשה ל־DTO
            var eventDto = new EventDTO
            {
                EventName = request.Name,
                Description = request.Description,
                Priority = request.Priority,
                EventDate = DateOnly.FromDateTime(DateTime.Parse(request.StartDate)),
                StartTime = TimeOnly.Parse(request.StartTime),
                EndTime = TimeOnly.Parse(request.EndTime),
                RequiredOfficers = request.RequiredOfficers
            };

            var zoneDto = new EventZoneDTO
            {
                Latitude1 = request.SelectedArea[0][0],
                Longitude1 = request.SelectedArea[0][1],
                Latitude2 = request.SelectedArea[1][0],
                Longitude2 = request.SelectedArea[1][1],
                Latitude3 = request.SelectedArea[2][0],
                Longitude3 = request.SelectedArea[2][1],
                Latitude4 = request.SelectedArea[3][0],
                Longitude4 = request.SelectedArea[3][1]
            };

            // 2. שמירה במסד
            int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

            // 3. שמירת הגרף והצמתים לפי מזהה האירוע
            GraphController.SaveGraphForEvent(eventId, GraphController.LatestGraph, GraphController.LatestNodes, GraphController.NodesInOriginalBounds);

            // 4. שמירת אזורים אסטרטגיים אם נשלחו
            if (request.StrategicZones != null && request.StrategicZones.Any())
            {
                foreach (var zone in request.StrategicZones)
                    zone.EventId = eventId;

                _strategicZoneBL.AddStrategicZones(request.StrategicZones);
            }

            // 5. סינון צמתים בתחום
            var nodesInBounds = GraphController.NodesInOriginalBounds
                .Where(kvp => kvp.Value == true)
                .Select(kvp => kvp.Key)
                .ToHashSet();

            // 6. המרת האזורים האסטרטגיים לצמתים קרובים - רק מבין הצמתים בתחום!
            List<long> strategicNodeIds = new List<long>();
            if (request.StrategicZones != null && request.StrategicZones.Any())
            {
                foreach (var zone in request.StrategicZones)
                {
                    // חיפוש הצומת הקרוב ביותר רק מבין הצמתים שבתחום
                    var closestNode = FindClosestNodeInBounds(GraphController.LatestGraph, zone.Latitude, zone.Longitude, nodesInBounds);
                    if (closestNode != -1)
                    {
                        strategicNodeIds.Add(closestNode);
                    }
                }

                // הסרת כפילויות
                strategicNodeIds = strategicNodeIds.Distinct().ToList();

                // בדיקה נוספת שמספר האזורים האסטרטגיים התקבלו כראוי
                if (strategicNodeIds.Count > request.RequiredOfficers)
                {
                    return BadRequest($"נמצאו {strategicNodeIds.Count} צמתים אסטרטגיים אבל יש רק {request.RequiredOfficers} שוטרים.");
                }
            }

            // 7. פיזור K-Center עם או בלי אזורים - כאן האזורים יחושבו כחלק מה-k
            var result = _kCenterService.DistributePolice(GraphController.LatestGraph, request.RequiredOfficers, nodesInBounds, strategicNodeIds);
            var selectedNodeIds = result.CenterNodes;
            var nodeToCoord = GraphController.LatestNodes;

            // 8. אימות שהאזורים האסטרטגיים אכן נכללו בפתרון
            if (strategicNodeIds.Any())
            {
                var missingStrategic = strategicNodeIds.Where(id => !selectedNodeIds.Contains(id)).ToList();
                if (missingStrategic.Any())
                {
                    return BadRequest($"האלגוריתם לא הצליח לכלול את כל האזורים האסטרטגיים בפתרון. חסרים: {string.Join(", ", missingStrategic)}");
                }
            }

            // 9. שליפת כל השוטרים הפנויים
            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
                eventDto.EventDate,
                eventDto.StartTime,
                eventDto.EndTime
            );

            // 10. שיוך שוטרים לנקודות
            var assignmentDtos = new List<OfficerAssignmentDTO>();

            foreach (var nodeId in selectedNodeIds)
            {
                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
                    continue;

                var availableOfficer = availableOfficers
                    .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
                    .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
                    .FirstOrDefault();

                if (availableOfficer != null)
                {
                    assignmentDtos.Add(new OfficerAssignmentDTO
                    {
                        PoliceOfficerId = availableOfficer.PoliceOfficerId,
                        EventId = eventId,
                        Latitude = coord.lat,
                        Longitude = coord.lon
                    });
                }
            }

            // 11. שמירה במסד
            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

            // 12. הכנת תגובה מפורטת
            var strategicCount = strategicNodeIds.Count;
            var regularCount = selectedNodeIds.Count - strategicCount;

            return Ok(new
            {
                EventId = eventId,
                OfficerCount = assignmentDtos.Count,
                StrategicOfficers = strategicCount,
                RegularOfficers = regularCount,
                Message = strategicCount > 0
                    ? $"אירוע נוצר בהצלחה. שובצו {strategicCount} שוטרים באזורים אסטרטגיים ו-{regularCount} שוטרים נוספים"
                    : "אירוع נוצר בהצלחה ושובצו שוטרים"
            });
        }

        [HttpDelete("{eventId}")]
        public IActionResult DeleteEvent(int eventId)
        {
            try
            {
                _eventService.DeleteEvent(eventId);
                GraphController.RemoveGraphForEvent(eventId);
                return Ok(new { Message = "האירוע נמחק בהצלחה" });
            }
            catch (Exception ex)
            {
                return BadRequest($"שגיאה במחיקת האירוע: {ex.Message}");
            }
        }

        [HttpGet("allEvents")]
        public IActionResult GetAllEvents()
        {
            var allEvents = _eventService.GetEvents();
            return Ok(allEvents);
        }

        private double GetDistanceFromOfficer(PoliceOfficerDTO officer, double lat, double lon)
        {
            // כאן תוכל להכניס חישוב Haversine אם תרצי
            return 0;
        }

        /// <summary>
        /// מוצא את הצומת הקרוב ביותר רק מבין הצמתים שבתחום הנבחר
        /// </summary>
        private long FindClosestNodeInBounds(Graph graph, double latitude, double longitude, HashSet<long> allowedNodes)
        {
            long closestNodeId = -1;
            double minDistance = double.MaxValue;

            // חיפוש רק בין הצמתים שבתחום
            foreach (var nodeId in allowedNodes)
            {
                if (graph.Nodes.TryGetValue(nodeId, out var node))
                {
                    double distance = Math.Sqrt(
                        Math.Pow(node.Latitude - latitude, 2) +
                        Math.Pow(node.Longitude - longitude, 2)
                    );

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestNodeId = node.Id;
                    }
                }
            }

            return closestNodeId;
        }

        /// <summary>
        /// פונקציה ישנה - נשארת לתאימות אחורה
        /// </summary>
        private long FindClosestNode(Graph graph, double latitude, double longitude)
        {
            long closestNodeId = -1;
            double minDistance = double.MaxValue;

            foreach (var node in graph.Nodes.Values)
            {
                double distance = Math.Sqrt(
                    Math.Pow(node.Latitude - latitude, 2) +
                    Math.Pow(node.Longitude - longitude, 2)
                );

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestNodeId = node.Id;
                }
            }

            return closestNodeId;
        }
    }

    public class CreateEventRequest
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Priority { get; set; } = "";
        public string StartDate { get; set; } = "";
        public string EndDate { get; set; } = "";
        public string StartTime { get; set; } = "";
        public string EndTime { get; set; } = "";
        public int RequiredOfficers { get; set; }
        public List<List<double>> SelectedArea { get; set; } = new();
        public List<StrategicZoneDTO> StrategicZones { get; set; } = new();
    }
}