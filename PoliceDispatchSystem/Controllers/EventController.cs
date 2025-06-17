////using Microsoft.AspNetCore.Mvc;
////using DTO;
////using DBEntities.Models;
////using IBL;
////using PoliceDispatchSystem.Controllers;

////namespace PoliceDispatchSystem.API
////{
////    [Route("api/[controller]")]
////    [ApiController]
////    public class EventController : ControllerBase
////    {
////        private readonly IEventService _eventService;
////        private readonly IKCenterService _kCenterService;
////        private readonly PoliceDispatchSystemContext _context;

////        public EventController(IEventService eventService, IKCenterService kCenterService, PoliceDispatchSystemContext context)
////        {
////            _eventService = eventService;
////            _kCenterService = kCenterService;
////            _context = context;
////        }

////        [HttpPost("create")]
////        public IActionResult CreateEvent([FromBody] CreateEventRequest request)
////        {
////            if (GraphController.LatestGraph == null || GraphController.LatestNodes == null)
////                return BadRequest("אין גרף טעון במערכת.");

////            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
////                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע.");

////            // 1. המרת הבקשה ל־DTO
////            var eventDto = new EventDTO
////            {
////                EventName = request.Name,
////                Description = request.Description,
////                Priority = request.Priority,
////                EventDate = DateOnly.FromDateTime(DateTime.Parse(request.StartDate)),
////                StartTime = TimeOnly.Parse(request.StartTime),
////                EndTime = TimeOnly.Parse(request.EndTime),
////                RequiredOfficers = request.RequiredOfficers
////            };

////            var zoneDto = new EventZoneDTO
////            {
////                Latitude1 = request.SelectedArea[0][0],
////                Longitude1 = request.SelectedArea[0][1],
////                Latitude2 = request.SelectedArea[1][0],
////                Longitude2 = request.SelectedArea[1][1],
////                Latitude3 = request.SelectedArea[2][0],
////                Longitude3 = request.SelectedArea[2][1],
////                Latitude4 = request.SelectedArea[3][0],
////                Longitude4 = request.SelectedArea[3][1]
////            };

////            // 2. שמירה במסד
////            int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

////            // 3. סינון צמתים בתחום
////            var nodesInBounds = GraphController.NodesInOriginalBounds
////                .Where(kvp => kvp.Value == true)
////                .Select(kvp => kvp.Key)
////                .ToHashSet();

////            // 4. פיזור K-Center
////            var result = _kCenterService.DistributePolice(GraphController.LatestGraph, request.RequiredOfficers, nodesInBounds);
////            var selectedNodeIds = result.CenterNodes;

////            var nodeToCoord = GraphController.LatestNodes;

////            // 5. שליפת כל השוטרים הפנויים (נניח כולם זמינים לצורך הפשטות)
////            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
////              eventDto.EventDate,
////              eventDto.StartTime,
////              eventDto.EndTime
////);


////            var assignedOfficers = new List<OfficerAssignment>();

////            foreach (var nodeId in selectedNodeIds)
////            {
////                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
////                    continue;

////                // מציאת השוטר הכי קרוב שלא שובץ עדיין
////                var availableOfficer = availableOfficers
////                .Where(o => !assignedOfficers.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
////                .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
////                .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
////                .FirstOrDefault();


////                if (availableOfficer != null)
////                {
////                    assignedOfficers.Add(new OfficerAssignment
////                    {
////                        PoliceOfficerId = availableOfficer.PoliceOfficerId,
////                        EventId = eventId,
////                        Latitude = coord.lat,
////                        Longitude = coord.lon
////                    });
////                }
////            }

////            _context.OfficerAssignments.AddRange(assignedOfficers);
////            _context.SaveChanges();

////            return Ok(new
////            {
////                EventId = eventId,
////                OfficerCount = assignedOfficers.Count,
////                Message = "אירוע נוצר בהצלחה ושובצו שוטרים"
////            });
////        }

////        private double GetDistanceFromOfficer(PoliceOfficer officer, double lat, double lon)
////        {            
////            return 0;
////        }
////    }

////    public class CreateEventRequest
////    {
////        public string Name { get; set; } = "";
////        public string Description { get; set; } = "";
////        public string Priority { get; set; } = "";
////        public string StartDate { get; set; } = "";
////        public string EndDate { get; set; } = "";
////        public string StartTime { get; set; } = "";
////        public string EndTime { get; set; } = "";
////        public int RequiredOfficers { get; set; }

////        public List<List<double>> SelectedArea { get; set; } = new(); // 4 נקודות [lat, lon]
////    }
////}


////אחרון
////using Microsoft.AspNetCore.Mvc;
////using DTO;
////using DBEntities.Models;
////using IBL;
////using PoliceDispatchSystem.Controllers;

////namespace PoliceDispatchSystem.API
////{
////    [Route("api/[controller]")]
////    [ApiController]
////    public class EventController : ControllerBase
////    {
////        private readonly IEventService _eventService;
////        private readonly IKCenterService _kCenterService;
////        private readonly IOfficerAssignmentService _officerAssignmentService; // הוספה חדשה

////        public EventController(
////            IEventService eventService,
////            IKCenterService kCenterService,
////            IOfficerAssignmentService officerAssignmentService) // הוספה חדשה
////        {
////            _eventService = eventService;
////            _kCenterService = kCenterService;
////            _officerAssignmentService = officerAssignmentService; // הוספה חדשה
////        }

////        [HttpPost("create")]
////        public IActionResult CreateEvent([FromBody] CreateEventRequest request)
////        {
////            if (GraphController.LatestGraph == null || GraphController.LatestNodes == null)
////                return BadRequest("אין גרף טעון במערכת.");

////            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
////                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע.");

////            // 1. המרת הבקשה ל־DTO
////            var eventDto = new EventDTO
////            {
////                EventName = request.Name,
////                Description = request.Description,
////                Priority = request.Priority,
////                EventDate = DateOnly.FromDateTime(DateTime.Parse(request.StartDate)),
////                StartTime = TimeOnly.Parse(request.StartTime),
////                EndTime = TimeOnly.Parse(request.EndTime),
////                RequiredOfficers = request.RequiredOfficers
////            };

////            var zoneDto = new EventZoneDTO
////            {
////                Latitude1 = request.SelectedArea[0][0],
////                Longitude1 = request.SelectedArea[0][1],
////                Latitude2 = request.SelectedArea[1][0],
////                Longitude2 = request.SelectedArea[1][1],
////                Latitude3 = request.SelectedArea[2][0],
////                Longitude3 = request.SelectedArea[2][1],
////                Latitude4 = request.SelectedArea[3][0],
////                Longitude4 = request.SelectedArea[3][1]
////            };

////            // 2. שמירה במסד
////            int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

////            // 3. סינון צמתים בתחום
////            var nodesInBounds = GraphController.NodesInOriginalBounds
////                .Where(kvp => kvp.Value == true)
////                .Select(kvp => kvp.Key)
////                .ToHashSet();

////            // 4. פיזור K-Center
////            var result = _kCenterService.DistributePolice(GraphController.LatestGraph, request.RequiredOfficers, nodesInBounds);
////            var selectedNodeIds = result.CenterNodes;

////            var nodeToCoord = GraphController.LatestNodes;

////            // 5. שליפת כל השוטרים הפנויים (עכשיו מחזיר DTO)
////            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
////                eventDto.EventDate,
////                eventDto.StartTime,
////                eventDto.EndTime
////            );

////            // 6. יצירת רשימת DTOs לשיוכים במקום עבודה ישירה על Entity
////            var assignmentDtos = new List<OfficerAssignmentDTO>();

////            foreach (var nodeId in selectedNodeIds)
////            {
////                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
////                    continue;

////                // מציאת השוטר הכי קרוב שלא שובץ עדיין
////                var availableOfficer = availableOfficers
////                    .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
////                    .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
////                    .FirstOrDefault();

////                if (availableOfficer != null)
////                {
////                    // יצירת DTO במקום Entity
////                    assignmentDtos.Add(new OfficerAssignmentDTO
////                    {
////                        PoliceOfficerId = availableOfficer.PoliceOfficerId,
////                        EventId = eventId,
////                        Latitude = coord.lat,
////                        Longitude = coord.lon
////                    });
////                }
////            }

////            // 7. שמירת השיוכים דרך שכבת השירות (BLL)
////            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

////            return Ok(new
////            {
////                EventId = eventId,
////                OfficerCount = assignmentDtos.Count,
////                Message = "אירוע נוצר בהצלחה ושובצו שוטרים"
////            });
////        }

////        private double GetDistanceFromOfficer(PoliceOfficerDTO officer, double lat, double lon)
////        {
////            return 0; // כאן תצטרך להוסיף חישוב מרחק אמיתי
////        }
////    }

////    public class CreateEventRequest
////    {
////        public string Name { get; set; } = "";
////        public string Description { get; set; } = "";
////        public string Priority { get; set; } = "";
////        public string StartDate { get; set; } = "";
////        public string EndDate { get; set; } = "";
////        public string StartTime { get; set; } = "";
////        public string EndTime { get; set; } = "";
////        public int RequiredOfficers { get; set; }

////        public List<List<double>> SelectedArea { get; set; } = new(); // 4 נקודות [lat, lon]
////    }
////}

////בלי אזורים אסטרטגיים
////using Microsoft.AspNetCore.Mvc;
////using DTO;
////using DBEntities.Models;
////using IBL;
////using PoliceDispatchSystem.Controllers;
////using BLL;

////namespace PoliceDispatchSystem.API
////{
////    [Route("api/[controller]")]
////    [ApiController]
////    public class EventController : ControllerBase
////    {
////        private readonly IEventService _eventService;
////        private readonly IKCenterService _kCenterService;
////        private readonly IOfficerAssignmentService _officerAssignmentService;

////        public EventController(
////            IEventService eventService,
////            IKCenterService kCenterService,
////            IOfficerAssignmentService officerAssignmentService)
////        {
////            _eventService = eventService;
////            _kCenterService = kCenterService;
////            _officerAssignmentService = officerAssignmentService;
////        }

////        [HttpPost("create")]
////        public IActionResult CreateEvent([FromBody] CreateEventRequest request)
////        {
////            if (GraphController.LatestGraph == null || GraphController.LatestNodes == null)
////                return BadRequest("אין גרף טעון במערכת.");

////            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
////                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע.");

////            // 1. המרת הבקשה ל־DTO
////            var eventDto = new EventDTO
////            {
////                EventName = request.Name,
////                Description = request.Description,
////                Priority = request.Priority,
////                EventDate = DateOnly.FromDateTime(DateTime.Parse(request.StartDate)),
////                StartTime = TimeOnly.Parse(request.StartTime),
////                EndTime = TimeOnly.Parse(request.EndTime),
////                RequiredOfficers = request.RequiredOfficers
////            };

////            var zoneDto = new EventZoneDTO
////            {
////                Latitude1 = request.SelectedArea[0][0],
////                Longitude1 = request.SelectedArea[0][1],
////                Latitude2 = request.SelectedArea[1][0],
////                Longitude2 = request.SelectedArea[1][1],
////                Latitude3 = request.SelectedArea[2][0],
////                Longitude3 = request.SelectedArea[2][1],
////                Latitude4 = request.SelectedArea[3][0],
////                Longitude4 = request.SelectedArea[3][1]
////            };

////            // 2. שמירה במסד
////            int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

////            // 3. שמירת הגרף והצמתים לפי מזהה האירוע
////            GraphController.SaveGraphForEvent(eventId, GraphController.LatestGraph, GraphController.LatestNodes, GraphController.NodesInOriginalBounds);

////            // 4. סינון צמתים בתחום
////            var nodesInBounds = GraphController.NodesInOriginalBounds
////                .Where(kvp => kvp.Value == true)
////                .Select(kvp => kvp.Key)
////                .ToHashSet();

////            // 5. פיזור K-Center
////            var result = _kCenterService.DistributePolice(GraphController.LatestGraph, request.RequiredOfficers, nodesInBounds);
////            var selectedNodeIds = result.CenterNodes;

////            var nodeToCoord = GraphController.LatestNodes;

////            // 6. שליפת כל השוטרים הפנויים
////            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
////                eventDto.EventDate,
////                eventDto.StartTime,
////                eventDto.EndTime
////            );

////            // 7. יצירת רשימת DTOs לשיוכים
////            var assignmentDtos = new List<OfficerAssignmentDTO>();

////            foreach (var nodeId in selectedNodeIds)
////            {
////                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
////                    continue;

////                var availableOfficer = availableOfficers
////                    .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
////                    .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
////                    .FirstOrDefault();

////                if (availableOfficer != null)
////                {
////                    assignmentDtos.Add(new OfficerAssignmentDTO
////                    {
////                        PoliceOfficerId = availableOfficer.PoliceOfficerId,
////                        EventId = eventId,
////                        Latitude = coord.lat,
////                        Longitude = coord.lon
////                    });
////                }
////            }

////            // 8. שמירת השיוכים דרך שכבת השירות
////            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

////            return Ok(new
////            {
////                EventId = eventId,
////                OfficerCount = assignmentDtos.Count,
////                Message = "אירוע נוצר בהצלחה ושובצו שוטרים"
////            });
////        }

////        [HttpDelete("{eventId}")]
////        public IActionResult DeleteEvent(int eventId)
////        {
////            try
////            {
////                // מחיקת האירוע מהמסד
////                _eventService.DeleteEvent(eventId);

////                // מחיקת הגרף השמור עבור האירוע
////                GraphController.RemoveGraphForEvent(eventId);

////                return Ok(new { Message = "האירוע נמחק בהצלחה" });
////            }
////            catch (Exception ex)
////            {
////                return BadRequest($"שגיאה במחיקת האירוע: {ex.Message}");
////            }
////        }

////        private double GetDistanceFromOfficer(PoliceOfficerDTO officer, double lat, double lon)
////        {
////            return 0; // כאן תצטרך להוסיף חישוב מרחק אמיתי
////        }

////        [HttpGet("allEvents")]
////        public IActionResult GetAllEvents()
////        {
////            var allEvents = _eventService.GetEvents();
////            return Ok(allEvents);
////    }
////    }




////    public class CreateEventRequest
////    {
////        public string Name { get; set; } = "";
////        public string Description { get; set; } = "";
////        public string Priority { get; set; } = "";
////        public string StartDate { get; set; } = "";
////        public string EndDate { get; set; } = "";
////        public string StartTime { get; set; } = "";
////        public string EndTime { get; set; } = "";
////        public int RequiredOfficers { get; set; }
////        public List<List<double>> SelectedArea { get; set; } = new();
////    }
////}

////ניסוי קלוד
////using Microsoft.AspNetCore.Mvc;
////using DTO;
////using DBEntities.Models;
////using IBL;
////using PoliceDispatchSystem.Controllers;
////using BLL;

////namespace PoliceDispatchSystem.API
////{
////    [Route("api/[controller]")]
////    [ApiController]
////    public class EventController : ControllerBase
////    {
////        private readonly IEventService _eventService;
////        private readonly IKCenterService _kCenterService;
////        private readonly IOfficerAssignmentService _officerAssignmentService;
////        private readonly IStrategicZoneBL _strategicZoneBL;

////        public EventController(
////            IEventService eventService,
////            IKCenterService kCenterService,
////            IOfficerAssignmentService officerAssignmentService,
////            IStrategicZoneBL strategicZoneBL)
////        {
////            _eventService = eventService;
////            _kCenterService = kCenterService;
////            _officerAssignmentService = officerAssignmentService;
////            _strategicZoneBL = strategicZoneBL;
////        }

////        [HttpPost("create")]
////        public IActionResult CreateEvent([FromBody] CreateEventRequest request)
////        {
////            if (GraphController.LatestGraph == null || GraphController.LatestNodes == null)
////                return BadRequest("אין גרף טעון במערכת.");

////            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
////                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע.");

////            // 1. המרת הבקשה ל־DTO
////            var eventDto = new EventDTO
////            {
////                EventName = request.Name,
////                Description = request.Description,
////                Priority = request.Priority,
////                EventDate = DateOnly.FromDateTime(DateTime.Parse(request.StartDate)),
////                StartTime = TimeOnly.Parse(request.StartTime),
////                EndTime = TimeOnly.Parse(request.EndTime),
////                RequiredOfficers = request.RequiredOfficers
////            };

////            var zoneDto = new EventZoneDTO
////            {
////                Latitude1 = request.SelectedArea[0][0],
////                Longitude1 = request.SelectedArea[0][1],
////                Latitude2 = request.SelectedArea[1][0],
////                Longitude2 = request.SelectedArea[1][1],
////                Latitude3 = request.SelectedArea[2][0],
////                Longitude3 = request.SelectedArea[2][1],
////                Latitude4 = request.SelectedArea[3][0],
////                Longitude4 = request.SelectedArea[3][1]
////            };

////            // 2. שמירה במסד
////            int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

////            // 3. שמירת הגרף והצמתים לפי מזהה האירוע
////            GraphController.SaveGraphForEvent(eventId, GraphController.LatestGraph, GraphController.LatestNodes, GraphController.NodesInOriginalBounds);

////            // 4. שמירת אזורים אסטרטגיים אם נשלחו
////            if (request.StrategicZones != null && request.StrategicZones.Any())
////            {
////                foreach (var zone in request.StrategicZones)
////                    zone.EventId = eventId;

////                _strategicZoneBL.AddStrategicZones(request.StrategicZones);
////            }

////            // 5. סינון צמתים בתחום
////            var nodesInBounds = GraphController.NodesInOriginalBounds
////                .Where(kvp => kvp.Value == true)
////                .Select(kvp => kvp.Key)
////                .ToHashSet();

////            // 6. המרת האזורים האסטרטגיים לצמתים קרובים
////            List<long> strategicNodeIds = request.StrategicZones?
////                .Select(z => FindClosestNode(GraphController.LatestGraph, z.Latitude, z.Longitude))
////                .Where(id => id != -1)
////                .Distinct()
////                .ToList() ?? new List<long>();

////            // 7. פיזור K-Center עם או בלי אזורים
////            var result = _kCenterService.DistributePolice(GraphController.LatestGraph, request.RequiredOfficers, nodesInBounds, strategicNodeIds);
////            var selectedNodeIds = result.CenterNodes;
////            var nodeToCoord = GraphController.LatestNodes;

////            // 8. שליפת כל השוטרים הפנויים
////            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
////                eventDto.EventDate,
////                eventDto.StartTime,
////                eventDto.EndTime
////            );

////            // 9. שיוך שוטרים לנקודות
////            var assignmentDtos = new List<OfficerAssignmentDTO>();

////            foreach (var nodeId in selectedNodeIds)
////            {
////                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
////                    continue;

////                var availableOfficer = availableOfficers
////                    .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
////                    .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
////                    .FirstOrDefault();

////                if (availableOfficer != null)
////                {
////                    assignmentDtos.Add(new OfficerAssignmentDTO
////                    {
////                        PoliceOfficerId = availableOfficer.PoliceOfficerId,
////                        EventId = eventId,
////                        Latitude = coord.lat,
////                        Longitude = coord.lon
////                    });
////                }
////            }

////            // 10. שמירה במסד
////            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

////            return Ok(new
////            {
////                EventId = eventId,
////                OfficerCount = assignmentDtos.Count,
////                Message = "אירוע נוצר בהצלחה ושובצו שוטרים"
////            });
////        }

////        [HttpDelete("{eventId}")]
////        public IActionResult DeleteEvent(int eventId)
////        {
////            try
////            {
////                _eventService.DeleteEvent(eventId);
////                GraphController.RemoveGraphForEvent(eventId);
////                return Ok(new { Message = "האירוע נמחק בהצלחה" });
////            }
////            catch (Exception ex)
////            {
////                return BadRequest($"שגיאה במחיקת האירוע: {ex.Message}");
////            }
////        }

////        [HttpGet("allEvents")]
////        public IActionResult GetAllEvents()
////        {
////            var allEvents = _eventService.GetEvents();
////            return Ok(allEvents);
////        }

////        private double GetDistanceFromOfficer(PoliceOfficerDTO officer, double lat, double lon)
////        {
////            // כאן תוכל להכניס חישוב Haversine אם תרצי
////            return 0;
////        }

////        private long FindClosestNode(Graph graph, double latitude, double longitude)
////        {
////            long closestNodeId = -1;
////            double minDistance = double.MaxValue;

////            foreach (var node in graph.Nodes.Values)
////            {
////                double distance = Math.Sqrt(
////                    Math.Pow(node.Latitude - latitude, 2) +
////                    Math.Pow(node.Longitude - longitude, 2)
////                );

////                if (distance < minDistance)
////                {
////                    minDistance = distance;
////                    closestNodeId = node.Id;
////                }
////            }

////            return closestNodeId;
////        }
////    }

////    public class CreateEventRequest
////    {
////        public string Name { get; set; } = "";
////        public string Description { get; set; } = "";
////        public string Priority { get; set; } = "";
////        public string StartDate { get; set; } = "";
////        public string EndDate { get; set; } = "";
////        public string StartTime { get; set; } = "";
////        public string EndTime { get; set; } = "";
////        public int RequiredOfficers { get; set; }
////        public List<List<double>> SelectedArea { get; set; } = new();
////        public List<StrategicZoneDTO> StrategicZones { get; set; } = new();
////    }
////}
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

//            // בדיקה שמספר האזורים האסטרטגיים לא עולה על מספר השוטרים
//            if (request.StrategicZones != null && request.StrategicZones.Count > request.RequiredOfficers)
//                return BadRequest($"לא ניתן להציב {request.StrategicZones.Count} אזורים אסטרטגיים עם {request.RequiredOfficers} שוטרים בלבד.");

//            // יצירת DTO לאירוע ואזור
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

//            // שמירה במסד נתונים
//            int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

//            // שמירת הגרף
//            GraphController.SaveGraphForEvent(eventId, GraphController.LatestGraph, GraphController.LatestNodes, GraphController.NodesInOriginalBounds);

//            // שמירת אזורים אסטרטגיים
//            if (request.StrategicZones != null && request.StrategicZones.Any())
//            {
//                foreach (var zone in request.StrategicZones)
//                    zone.EventId = eventId;
//                _strategicZoneBL.AddStrategicZones(request.StrategicZones);
//            }

//            // קבלת צמתים בתחום
//            var nodesInBounds = GraphController.NodesInOriginalBounds
//                .Where(kvp => kvp.Value == true)
//                .Select(kvp => kvp.Key)
//                .ToHashSet();

//            Console.WriteLine($"🔍 מספר צמתים בתחום: {nodesInBounds.Count}");

//            // המרת אזורים אסטרטגיים לצמתים - עם דיבוג מפורט
//            List<long> strategicNodeIds = new List<long>();

//            if (request.StrategicZones != null && request.StrategicZones.Any())
//            {
//                Console.WriteLine($"🎯 מעבד {request.StrategicZones.Count} אזורים אסטרטגיים:");

//                foreach (var zone in request.StrategicZones)
//                {
//                    Console.WriteLine($"   אזור: ({zone.Latitude}, {zone.Longitude})");

//                    var closestNode = FindClosestNodeInBounds(GraphController.LatestGraph, zone.Latitude, zone.Longitude, nodesInBounds);

//                    if (closestNode != -1)
//                    {
//                        strategicNodeIds.Add(closestNode);

//                        // הדפסת פרטי הצומת שנמצא
//                        if (GraphController.LatestNodes.TryGetValue(closestNode, out var nodeCoord))
//                        {
//                            var distance = Math.Sqrt(
//                                Math.Pow(nodeCoord.lat - zone.Latitude, 2) +
//                                Math.Pow(nodeCoord.lon - zone.Longitude, 2)
//                            );
//                            Console.WriteLine($"   ✅ נמצא צומת {closestNode} במיקום ({nodeCoord.lat}, {nodeCoord.lon}), מרחק: {distance:F6}");
//                        }
//                    }
//                    else
//                    {
//                        Console.WriteLine($"   ❌ לא נמצא צומת קרוב לאזור ({zone.Latitude}, {zone.Longitude})");
//                    }
//                }

//                strategicNodeIds = strategicNodeIds.Distinct().ToList();
//                Console.WriteLine($"🎯 סה\"כ צמתים אסטרטגיים ייחודיים: {strategicNodeIds.Count}");

//                if (strategicNodeIds.Count != request.StrategicZones.Count)
//                {
//                    Console.WriteLine($"⚠️  אזהרה: היו {request.StrategicZones.Count} אזורים אבל נמצאו רק {strategicNodeIds.Count} צמתים");
//                }
//            }

//            // קריאה לאלגוריתם עם הצמתים האסטרטגיים
//            Console.WriteLine($"🚀 מפעיל K-Center עם {request.RequiredOfficers} שוטרים ו-{strategicNodeIds.Count} צמתים אסטרטגיים");

//            var result = _kCenterService.DistributePolice(GraphController.LatestGraph, request.RequiredOfficers, nodesInBounds, strategicNodeIds);
//            var selectedNodeIds = result.CenterNodes;

//            Console.WriteLine($"📍 האלגוריתם בחר {selectedNodeIds.Count} צמתים:");
//            foreach (var nodeId in selectedNodeIds)
//            {
//                if (GraphController.LatestNodes.TryGetValue(nodeId, out var coord))
//                {
//                    var isStrategic = strategicNodeIds.Contains(nodeId) ? "🎯 אסטרטגי" : "👮 רגיל";
//                    Console.WriteLine($"   {isStrategic}: צומת {nodeId} במיקום ({coord.lat}, {coord.lon})");
//                }
//            }

//            // בדיקה קריטית: האם כל הצמתים האסטרטגיים נכללו?
//            var missingStrategic = strategicNodeIds.Where(id => !selectedNodeIds.Contains(id)).ToList();
//            if (missingStrategic.Any())
//            {
//                Console.WriteLine($"❌ צמתים אסטרטגיים שלא נכללו: {string.Join(", ", missingStrategic)}");
//                return BadRequest($"האלגוריתם לא הצליח לכלול את כל האזורים האסטרטגיים. חסרים: {string.Join(", ", missingStrategic)}");
//            }

//            // המשך הקוד - שיוך שוטרים וכו'...
//            var nodeToCoord = GraphController.LatestNodes;
//            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
//                eventDto.EventDate,
//                eventDto.StartTime,
//                eventDto.EndTime
//            );

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

//            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

//            var strategicCount = strategicNodeIds.Count;
//            var regularCount = selectedNodeIds.Count - strategicCount;

//            return Ok(new
//            {
//                EventId = eventId,
//                OfficerCount = assignmentDtos.Count,
//                StrategicOfficers = strategicCount,
//                RegularOfficers = regularCount,
//                Message = strategicCount > 0
//                    ? $"אירוע נוצר בהצלחה. שובצו {strategicCount} שוטרים באזורים אסטרטגיים ו-{regularCount} שוטרים נוספים"
//                    : "אירוע נוצר בהצלחה ושובצו שוטרים",
//                DebugInfo = new
//                {
//                    OriginalStrategicZones = request.StrategicZones?.Count ?? 0,
//                    FoundStrategicNodes = strategicNodeIds.Count,
//                    TotalNodesInBounds = nodesInBounds.Count,
//                    SelectedNodes = selectedNodeIds.Count
//                }
//            });
//        }

//        //[HttpPost("create")]
//        //public IActionResult CreateEvent([FromBody] CreateEventRequest request)
//        //{
//        //    if (GraphController.LatestGraph == null || GraphController.LatestNodes == null)
//        //        return BadRequest("אין גרף טעון במערכת.");

//        //    if (request.SelectedArea == null || request.SelectedArea.Count < 4)
//        //        return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע.");

//        //    // בדיקה שמספר האזורים האסטרטגיים לא עולה על מספר השוטרים
//        //    if (request.StrategicZones != null && request.StrategicZones.Count > request.RequiredOfficers)
//        //        return BadRequest($"לא ניתן להציב {request.StrategicZones.Count} אזורים אסטרטגיים עם {request.RequiredOfficers} שוטרים בלבד.");

//        //    // 1. המרת הבקשה ל־DTO
//        //    var eventDto = new EventDTO
//        //    {
//        //        EventName = request.Name,
//        //        Description = request.Description,
//        //        Priority = request.Priority,
//        //        EventDate = DateOnly.FromDateTime(DateTime.Parse(request.StartDate)),
//        //        StartTime = TimeOnly.Parse(request.StartTime),
//        //        EndTime = TimeOnly.Parse(request.EndTime),
//        //        RequiredOfficers = request.RequiredOfficers
//        //    };

//        //    var zoneDto = new EventZoneDTO
//        //    {
//        //        Latitude1 = request.SelectedArea[0][0],
//        //        Longitude1 = request.SelectedArea[0][1],
//        //        Latitude2 = request.SelectedArea[1][0],
//        //        Longitude2 = request.SelectedArea[1][1],
//        //        Latitude3 = request.SelectedArea[2][0],
//        //        Longitude3 = request.SelectedArea[2][1],
//        //        Latitude4 = request.SelectedArea[3][0],
//        //        Longitude4 = request.SelectedArea[3][1]
//        //    };

//        //    // 2. שמירה במסד
//        //    int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

//        //    // 3. שמירת הגרף והצמתים לפי מזהה האירוע
//        //    GraphController.SaveGraphForEvent(eventId, GraphController.LatestGraph, GraphController.LatestNodes, GraphController.NodesInOriginalBounds);

//        //    // 4. שמירת אזורים אסטרטגיים אם נשלחו
//        //    if (request.StrategicZones != null && request.StrategicZones.Any())
//        //    {
//        //        foreach (var zone in request.StrategicZones)
//        //            zone.EventId = eventId;

//        //        _strategicZoneBL.AddStrategicZones(request.StrategicZones);
//        //    }

//        //    // 5. סינון צמתים בתחום
//        //    var nodesInBounds = GraphController.NodesInOriginalBounds
//        //        .Where(kvp => kvp.Value == true)
//        //        .Select(kvp => kvp.Key)
//        //        .ToHashSet();

//        //    // 6. המרת האזורים האסטרטגיים לצמתים קרובים - רק מבין הצמתים בתחום!
//        //    List<long> strategicNodeIds = new List<long>();
//        //    if (request.StrategicZones != null && request.StrategicZones.Any())
//        //    {
//        //        foreach (var zone in request.StrategicZones)
//        //        {
//        //            // חיפוש הצומת הקרוב ביותר רק מבין הצמתים שבתחום
//        //            var closestNode = FindClosestNodeInBounds(GraphController.LatestGraph, zone.Latitude, zone.Longitude, nodesInBounds);
//        //            if (closestNode != -1)
//        //            {
//        //                strategicNodeIds.Add(closestNode);
//        //            }
//        //        }

//        //        // הסרת כפילויות
//        //        strategicNodeIds = strategicNodeIds.Distinct().ToList();

//        //        // בדיקה נוספת שמספר האזורים האסטרטגיים התקבלו כראוי
//        //        if (strategicNodeIds.Count > request.RequiredOfficers)
//        //        {
//        //            return BadRequest($"נמצאו {strategicNodeIds.Count} צמתים אסטרטגיים אבל יש רק {request.RequiredOfficers} שוטרים.");
//        //        }
//        //    }

//        //    // 7. פיזור K-Center עם או בלי אזורים - כאן האזורים יחושבו כחלק מה-k
//        //    var result = _kCenterService.DistributePolice(GraphController.LatestGraph, request.RequiredOfficers, nodesInBounds, strategicNodeIds);
//        //    var selectedNodeIds = result.CenterNodes;
//        //    var nodeToCoord = GraphController.LatestNodes;

//        //    // 8. אימות שהאזורים האסטרטגיים אכן נכללו בפתרון
//        //    if (strategicNodeIds.Any())
//        //    {
//        //        var missingStrategic = strategicNodeIds.Where(id => !selectedNodeIds.Contains(id)).ToList();
//        //        if (missingStrategic.Any())
//        //        {
//        //            return BadRequest($"האלגוריתם לא הצליח לכלול את כל האזורים האסטרטגיים בפתרון. חסרים: {string.Join(", ", missingStrategic)}");
//        //        }
//        //    }

//        //    // 9. שליפת כל השוטרים הפנויים
//        //    var availableOfficers = _eventService.GetAvailableOfficersForEvent(
//        //        eventDto.EventDate,
//        //        eventDto.StartTime,
//        //        eventDto.EndTime
//        //    );

//        //    // 10. שיוך שוטרים לנקודות
//        //    var assignmentDtos = new List<OfficerAssignmentDTO>();

//        //    foreach (var nodeId in selectedNodeIds)
//        //    {
//        //        if (!nodeToCoord.TryGetValue(nodeId, out var coord))
//        //            continue;

//        //        var availableOfficer = availableOfficers
//        //            .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
//        //            .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
//        //            .FirstOrDefault();

//        //        if (availableOfficer != null)
//        //        {
//        //            assignmentDtos.Add(new OfficerAssignmentDTO
//        //            {
//        //                PoliceOfficerId = availableOfficer.PoliceOfficerId,
//        //                EventId = eventId,
//        //                Latitude = coord.lat,
//        //                Longitude = coord.lon
//        //            });
//        //        }
//        //    }

//        //    // 11. שמירה במסד
//        //    _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

//        //    // 12. הכנת תגובה מפורטת
//        //    var strategicCount = strategicNodeIds.Count;
//        //    var regularCount = selectedNodeIds.Count - strategicCount;

//        //    return Ok(new
//        //    {
//        //        EventId = eventId,
//        //        OfficerCount = assignmentDtos.Count,
//        //        StrategicOfficers = strategicCount,
//        //        RegularOfficers = regularCount,
//        //        Message = strategicCount > 0
//        //            ? $"אירוע נוצר בהצלחה. שובצו {strategicCount} שוטרים באזורים אסטרטגיים ו-{regularCount} שוטרים נוספים"
//        //            : "אירוع נוצר בהצלחה ושובצו שוטרים"
//        //    });
//        //}

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
//        [HttpPost("create-with-positions")]
//        public IActionResult CreateEventWithPositions([FromBody] CreateEventWithPositionsRequest request)
//        {
//            if (request.PreCalculatedPositions == null || !request.PreCalculatedPositions.Any())
//                return BadRequest("לא נמצא פיזור מוכן של שוטרים");

//            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
//                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע");

//            try
//            {
//                // יצירת DTO לאירוע
//                var eventDto = new EventDTO
//                {
//                    EventName = request.Name,
//                    Description = request.Description,
//                    Priority = request.Priority,
//                    EventDate = DateOnly.FromDateTime(DateTime.Parse(request.StartDate)),
//                    StartTime = TimeOnly.Parse(request.StartTime),
//                    EndTime = TimeOnly.Parse(request.EndTime),
//                    RequiredOfficers = request.RequiredOfficers
//                };

//                var zoneDto = new EventZoneDTO
//                {
//                    Latitude1 = request.SelectedArea[0][0],
//                    Longitude1 = request.SelectedArea[0][1],
//                    Latitude2 = request.SelectedArea[1][0],
//                    Longitude2 = request.SelectedArea[1][1],
//                    Latitude3 = request.SelectedArea[2][0],
//                    Longitude3 = request.SelectedArea[2][1],
//                    Latitude4 = request.SelectedArea[3][0],
//                    Longitude4 = request.SelectedArea[3][1]
//                };

//                // שמירה במסד נתונים
//                int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

//                // שמירת גרף עבור האירוע
//                if (GraphController.LatestGraph != null && GraphController.LatestNodes != null)
//                {
//                    GraphController.SaveGraphForEvent(eventId, GraphController.LatestGraph, GraphController.LatestNodes, GraphController.NodesInOriginalBounds);
//                }

//                // שמירת אזורים אסטרטגיים
//                if (request.StrategicZones != null && request.StrategicZones.Any())
//                {
//                    foreach (var zone in request.StrategicZones)
//                        zone.EventId = eventId;
//                    _strategicZoneBL.AddStrategicZones(request.StrategicZones);
//                }

//                // שליפת שוטרים זמינים
//                var availableOfficers = _eventService.GetAvailableOfficersForEvent(
//                    eventDto.EventDate,
//                    eventDto.StartTime,
//                    eventDto.EndTime
//                );

//                // 🎯 שימוש בפיזור המוכן מראש
//                var assignmentDtos = new List<OfficerAssignmentDTO>();
//                int strategicCount = 0;

//                Console.WriteLine($"💾 משתמש בפיזור מוכן עם {request.PreCalculatedPositions.Count} מיקומים");

//                foreach (var position in request.PreCalculatedPositions)
//                {
//                    // מציאת שוטר זמין
//                    var availableOfficer = availableOfficers
//                        .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
//                        .OrderBy(o => GetDistanceFromOfficer(o, position.Latitude, position.Longitude))
//                        .FirstOrDefault();

//                    if (availableOfficer != null)
//                    {
//                        assignmentDtos.Add(new OfficerAssignmentDTO
//                        {
//                            PoliceOfficerId = availableOfficer.PoliceOfficerId,
//                            EventId = eventId,
//                            Latitude = position.Latitude,
//                            Longitude = position.Longitude
//                        });

//                        if (position.IsStrategic)
//                        {
//                            strategicCount++;
//                            Console.WriteLine($"🎯 שוטר אסטרטגי הוצב במיקום ({position.Latitude}, {position.Longitude})");
//                        }
//                        else
//                        {
//                            Console.WriteLine($"👮 שוטר רגיל הוצב במיקום ({position.Latitude}, {position.Longitude})");
//                        }
//                    }
//                }

//                // שמירת השיוכים
//                _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

//                var regularCount = assignmentDtos.Count - strategicCount;

//                Console.WriteLine($"✅ נוצר אירוע {eventId} עם {assignmentDtos.Count} שוטרים ({strategicCount} אסטרטגיים, {regularCount} רגילים)");

//                return Ok(new
//                {
//                    EventId = eventId,
//                    OfficerCount = assignmentDtos.Count,
//                    StrategicOfficers = strategicCount,
//                    RegularOfficers = regularCount,
//                    Message = strategicCount > 0
//                        ? $"אירוע נוצר בהצלחה. שובצו {strategicCount} שוטרים באזורים אסטרטגיים ו-{regularCount} שוטרים נוספים"
//                        : "אירוע נוצר בהצלחה ושובצו שוטרים"
//                });
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ שגיאה ביצירת אירוע: {ex.Message}");
//                return BadRequest($"שגיאה ביצירת האירוע: {ex.Message}");
//            }
//        }

//        // מחלקת בקשה חדשה
//        public class CreateEventWithPositionsRequest
//        {
//            public string Name { get; set; } = "";
//            public string Description { get; set; } = "";
//            public string Priority { get; set; } = "";
//            public string StartDate { get; set; } = "";
//            public string EndDate { get; set; } = "";
//            public string StartTime { get; set; } = "";
//            public string EndTime { get; set; } = "";
//            public int RequiredOfficers { get; set; }
//            public List<List<double>> SelectedArea { get; set; } = new();
//            public List<StrategicZoneDTO> StrategicZones { get; set; } = new();
//            public List<PreCalculatedPosition> PreCalculatedPositions { get; set; } = new();  // 🆕
//        }

//        public class PreCalculatedPosition
//        {
//            public double Latitude { get; set; }
//            public double Longitude { get; set; }
//            public bool IsStrategic { get; set; }
//            public long NodeId { get; set; }
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
//        /// <summary>
//        /// מוצא את הצומת הקרוב ביותר לקואורדינטה נתונה - רק מבין הצמתים שבתחום
//        /// </summary>
//        private long FindClosestNodeInBounds(Graph graph, double latitude, double longitude, HashSet<long> allowedNodes)
//        {
//            if (allowedNodes == null || !allowedNodes.Any())
//            {
//                Console.WriteLine("❌ אין צמתים מותרים לחיפוש");
//                return -1;
//            }

//            long closestNodeId = -1;
//            double minDistance = double.MaxValue;

//            Console.WriteLine($"🔍 מחפש צומת קרוב ל-({latitude}, {longitude}) מבין {allowedNodes.Count} צמתים");

//            foreach (var nodeId in allowedNodes)
//            {
//                if (graph.Nodes.TryGetValue(nodeId, out var node))
//                {
//                    // חישוב מרחק Euclidean פשוט
//                    double distance = Math.Sqrt(
//                        Math.Pow(node.Latitude - latitude, 2) +
//                        Math.Pow(node.Longitude - longitude, 2)
//                    );

//                    if (distance < minDistance)
//                    {
//                        minDistance = distance;
//                        closestNodeId = node.Id;
//                    }
//                }
//            }

//            if (closestNodeId != -1)
//            {
//                Console.WriteLine($"✅ נמצא צומת {closestNodeId} במרחק {minDistance:F6}");

//                // בדיקה אם המרחק סביר (פחות מ-0.01 מעלות ≈ 1ק"מ)
//                if (minDistance > 0.01)
//                {
//                    Console.WriteLine($"⚠️  אזהרה: המרחק גדול יחסית ({minDistance:F6}), ייתכן שהאזור האסטרטגי רחוק מהגרף");
//                }
//            }
//            else
//            {
//                Console.WriteLine("❌ לא נמצא צומת מתאים");
//            }

//            return closestNodeId;
//        }

//        /// <summary>
//        /// פונקציה נוספת - מציאת כמה צמתים הכי קרובים (עבור דיבוג)
//        /// </summary>
//        private List<(long nodeId, double distance)> FindClosestNodesDebug(Graph graph, double latitude, double longitude, HashSet<long> allowedNodes, int count = 5)
//        {
//            var distances = new List<(long nodeId, double distance)>();

//            foreach (var nodeId in allowedNodes)
//            {
//                if (graph.Nodes.TryGetValue(nodeId, out var node))
//                {
//                    double distance = Math.Sqrt(
//                        Math.Pow(node.Latitude - latitude, 2) +
//                        Math.Pow(node.Longitude - longitude, 2)
//                    );
//                    distances.Add((nodeId, distance));
//                }
//            }

//            return distances.OrderBy(x => x.distance).Take(count).ToList();
//        }
//        /// <summary>
//        /// מוצא את הצומת הקרוב ביותר רק מבין הצמתים שבתחום הנבחר
//        /// </summary>
//        //private long FindClosestNodeInBounds(Graph graph, double latitude, double longitude, HashSet<long> allowedNodes)
//        //{
//        //    long closestNodeId = -1;
//        //    double minDistance = double.MaxValue;

//        //    // חיפוש רק בין הצמתים שבתחום
//        //    foreach (var nodeId in allowedNodes)
//        //    {
//        //        if (graph.Nodes.TryGetValue(nodeId, out var node))
//        //        {
//        //            double distance = Math.Sqrt(
//        //                Math.Pow(node.Latitude - latitude, 2) +
//        //                Math.Pow(node.Longitude - longitude, 2)
//        //            );

//        //            if (distance < minDistance)
//        //            {
//        //                minDistance = distance;
//        //                closestNodeId = node.Id;
//        //            }
//        //        }
//        //    }

//        //    return closestNodeId;
//        //}

//        /// <summary>
//        /// פונקציה ישנה - נשארת לתאימות אחורה
//        /// </summary>
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


////16.06
//using Microsoft.AspNetCore.Mvc;
//using PoliceDispatchSystem.API;
//using BLL;
//using DAL;
//using DTO;
//using IBL;
//using PoliceDispatchSystem.Controllers;
//// ==== תיקון 1: הוספת מחלקת CreateEventRequest ====
//namespace PoliceDispatchSystem.API
//{
//    // מחלקת בקשה ליצירת אירוע - צריכה להיות בתוך ה-namespace
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

//            // בדיקה שמספר האזורים האסטרטגיים לא עולה על מספר השוטרים
//            if (request.StrategicZones != null && request.StrategicZones.Count > request.RequiredOfficers)
//                return BadRequest($"לא ניתן להציב {request.StrategicZones.Count} אזורים אסטרטגיים עם {request.RequiredOfficers} שוטרים בלבד.");

//            // יצירת DTO לאירוע ואזור
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

//            // שמירה במסד נתונים
//            int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

//            // שמירת הגרף
//            GraphController.SaveGraphForEvent(eventId, GraphController.LatestGraph, GraphController.LatestNodes, GraphController.NodesInOriginalBounds);

//            // שמירת אזורים אסטרטגיים
//            if (request.StrategicZones != null && request.StrategicZones.Any())
//            {
//                foreach (var zone in request.StrategicZones)
//                    zone.EventId = eventId;
//                _strategicZoneBL.AddStrategicZones(request.StrategicZones);
//            }

//            // קבלת צמתים בתחום
//            var nodesInBounds = GraphController.NodesInOriginalBounds
//                .Where(kvp => kvp.Value == true)
//                .Select(kvp => kvp.Key)
//                .ToHashSet();

//            Console.WriteLine($"🔍 מספר צמתים בתחום: {nodesInBounds.Count}");

//            // **יצירת צמתים אסטרטגיים על דרכים אמיתיות**
//            List<long> strategicNodeIds = new List<long>();

//            if (request.StrategicZones != null && request.StrategicZones.Any())
//            {
//                Console.WriteLine($"🎯 יוצר {request.StrategicZones.Count} צמתים אסטרטגיים על דרכים:");

//                foreach (var zone in request.StrategicZones)
//                {
//                    Console.WriteLine($"\n📍 מעבד אזור: ({zone.Latitude}, {zone.Longitude})");

//                    // **שימוש בפיצול Way במקום חיפוש צומת קרוב**
//                    var newStrategicNodeId = GraphController.LatestGraph.CreateStrategicNodeOnWay(
//                        zone.Latitude,
//                        zone.Longitude,
//                        nodesInBounds
//                    );

//                    if (newStrategicNodeId != -1)
//                    {
//                        strategicNodeIds.Add(newStrategicNodeId);

//                        // עדכון המילונים
//                        var actualCoord = GraphController.LatestGraph.Nodes[newStrategicNodeId];
//                        var latestNodes = GraphController.LatestNodes;
//                        latestNodes[newStrategicNodeId] = (actualCoord.Latitude, actualCoord.Longitude);

//                        var nodesInOriginalBounds = GraphController.NodesInOriginalBounds;
//                        nodesInOriginalBounds[newStrategicNodeId] = true;

//                        Console.WriteLine($"✅ נוצר צומת אסטרטגי {newStrategicNodeId} על דרך אמיתית");
//                    }
//                    else
//                    {
//                        Console.WriteLine($"❌ כשל ביצירת צומת - לא נמצא קטע דרך מתאים במיקום ({zone.Latitude}, {zone.Longitude})");
//                        return BadRequest($"לא ניתן ליצור צומת אסטרטגי במיקום ({zone.Latitude}, {zone.Longitude}) - לא נמצא קטע דרך קרוב");
//                    }
//                }

//                strategicNodeIds = strategicNodeIds.Distinct().ToList();
//                Console.WriteLine($"\n🎯 סה\"כ צמתים אסטרטגיים נוצרו על דרכים: {strategicNodeIds.Count}");
//            }

//            // עדכון רשימת הצמתים המותרים
//            var allowedNodesForDistribution = new HashSet<long>(nodesInBounds);
//            foreach (var strategicId in strategicNodeIds)
//            {
//                allowedNodesForDistribution.Add(strategicId);
//            }

//            Console.WriteLine($"📊 סה\"כ צמתים זמינים לפיזור: {allowedNodesForDistribution.Count}");

//            // פיזור K-Center
//            var result = _kCenterService.DistributePolice(
//                GraphController.LatestGraph,
//                request.RequiredOfficers,
//                allowedNodesForDistribution,
//                strategicNodeIds
//            );

//            // בדיקה שכל הצמתים האסטרטגיים נכללו
//            var missingStrategic = strategicNodeIds.Where(id => !result.CenterNodes.Contains(id)).ToList();
//            if (missingStrategic.Any())
//            {
//                Console.WriteLine($"❌ צמתים אסטרטגיים שלא נכללו: {string.Join(", ", missingStrategic)}");
//                return BadRequest($"האלגוריתם לא הצליח לכלול את כל הצמתים האסטרטגיים. חסרים: {string.Join(", ", missingStrategic)}");
//            }

//            // המשך הקוד כרגיל...
//            var selectedNodeIds = result.CenterNodes;
//            var nodeToCoord = GraphController.LatestNodes;

//            // שליפת שוטרים זמינים ושיוך
//            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
//                eventDto.EventDate,
//                eventDto.StartTime,
//                eventDto.EndTime
//            );

//            var assignmentDtos = new List<OfficerAssignmentDTO>();
//            foreach (var nodeId in selectedNodeIds)
//            {
//                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
//                    continue;

//                var availableOfficer = availableOfficers
//                    .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
//                    .OrderBy(o => CalculateDistanceFromOfficer(o, coord.lat, coord.lon)) // 🔧 שינוי שם הפונקציה
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

//            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

//            var strategicCount = strategicNodeIds.Count;
//            var regularCount = selectedNodeIds.Count - strategicCount;

//            return Ok(new
//            {
//                EventId = eventId,
//                OfficerCount = assignmentDtos.Count,
//                StrategicOfficers = strategicCount,
//                RegularOfficers = regularCount,
//                NodesCreatedOnRealRoads = strategicNodeIds.Count,
//                Message = strategicCount > 0
//                    ? $"אירוע נוצר בהצלחה. נוצרו {strategicNodeIds.Count} צמתים אסטרטגיים על דרכים אמיתיות ושובצו {strategicCount} שוטרים באזורים אסטרטגיים ו-{regularCount} שוטרים נוספים"
//                    : "אירוע נוצר בהצלחה ושובצו שוטרים",
//                DebugInfo = new
//                {
//                    OriginalStrategicZones = request.StrategicZones?.Count ?? 0,
//                    FoundStrategicNodes = strategicNodeIds.Count,
//                    TotalNodesInBounds = nodesInBounds.Count,
//                    TotalWaySegments = GraphController.LatestGraph.WaySegments.Count,
//                    SelectedNodes = selectedNodeIds.Count
//                }
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

//        [HttpPost("create-with-positions")]
//        public IActionResult CreateEventWithPositions([FromBody] CreateEventWithPositionsRequest request)
//        {
//            if (request.PreCalculatedPositions == null || !request.PreCalculatedPositions.Any())
//                return BadRequest("לא נמצא פיזור מוכן של שוטרים");

//            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
//                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע");

//            try
//            {
//                // יצירת DTO לאירוע
//                var eventDto = new EventDTO
//                {
//                    EventName = request.Name,
//                    Description = request.Description,
//                    Priority = request.Priority,
//                    EventDate = DateOnly.FromDateTime(DateTime.Parse(request.StartDate)),
//                    StartTime = TimeOnly.Parse(request.StartTime),
//                    EndTime = TimeOnly.Parse(request.EndTime),
//                    RequiredOfficers = request.RequiredOfficers
//                };

//                var zoneDto = new EventZoneDTO
//                {
//                    Latitude1 = request.SelectedArea[0][0],
//                    Longitude1 = request.SelectedArea[0][1],
//                    Latitude2 = request.SelectedArea[1][0],
//                    Longitude2 = request.SelectedArea[1][1],
//                    Latitude3 = request.SelectedArea[2][0],
//                    Longitude3 = request.SelectedArea[2][1],
//                    Latitude4 = request.SelectedArea[3][0],
//                    Longitude4 = request.SelectedArea[3][1]
//                };

//                // שמירה במסד נתונים
//                int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

//                // שמירת גרף עבור האירוע
//                if (GraphController.LatestGraph != null && GraphController.LatestNodes != null)
//                {
//                    GraphController.SaveGraphForEvent(eventId, GraphController.LatestGraph, GraphController.LatestNodes, GraphController.NodesInOriginalBounds);
//                }

//                // שמירת אזורים אסטרטגיים
//                if (request.StrategicZones != null && request.StrategicZones.Any())
//                {
//                    foreach (var zone in request.StrategicZones)
//                        zone.EventId = eventId;
//                    _strategicZoneBL.AddStrategicZones(request.StrategicZones);
//                }

//                // שליפת שוטרים זמינים
//                var availableOfficers = _eventService.GetAvailableOfficersForEvent(
//                    eventDto.EventDate,
//                    eventDto.StartTime,
//                    eventDto.EndTime
//                );

//                // שימוש בפיזור המוכן מראש
//                var assignmentDtos = new List<OfficerAssignmentDTO>();
//                int strategicCount = 0;

//                Console.WriteLine($"💾 משתמש בפיזור מוכן עם {request.PreCalculatedPositions.Count} מיקומים");

//                foreach (var position in request.PreCalculatedPositions)
//                {
//                    // מציאת שוטר זמין
//                    var availableOfficer = availableOfficers
//                        .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
//                        .OrderBy(o => CalculateDistanceFromOfficer(o, position.Latitude, position.Longitude)) // 🔧 שינוי שם הפונקציה
//                        .FirstOrDefault();

//                    if (availableOfficer != null)
//                    {
//                        assignmentDtos.Add(new OfficerAssignmentDTO
//                        {
//                            PoliceOfficerId = availableOfficer.PoliceOfficerId,
//                            EventId = eventId,
//                            Latitude = position.Latitude,
//                            Longitude = position.Longitude
//                        });

//                        if (position.IsStrategic)
//                        {
//                            strategicCount++;
//                            Console.WriteLine($"🎯 שוטר אסטרטגי הוצב במיקום ({position.Latitude}, {position.Longitude})");
//                        }
//                        else
//                        {
//                            Console.WriteLine($"👮 שוטר רגיל הוצב במיקום ({position.Latitude}, {position.Longitude})");
//                        }
//                    }
//                }

//                // שמירת השיוכים
//                _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

//                var regularCount = assignmentDtos.Count - strategicCount;

//                Console.WriteLine($"✅ נוצר אירוע {eventId} עם {assignmentDtos.Count} שוטרים ({strategicCount} אסטרטגיים, {regularCount} רגילים)");

//                return Ok(new
//                {
//                    EventId = eventId,
//                    OfficerCount = assignmentDtos.Count,
//                    StrategicOfficers = strategicCount,
//                    RegularOfficers = regularCount,
//                    Message = strategicCount > 0
//                        ? $"אירוע נוצר בהצלחה. שובצו {strategicCount} שוטרים באזורים אסטרטגיים ו-{regularCount} שוטרים נוספים"
//                        : "אירוע נוצר בהצלחה ושובצו שוטרים"
//                });
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"❌ שגיאה ביצירת אירוע: {ex.Message}");
//                return BadRequest($"שגיאה ביצירת האירוע: {ex.Message}");
//            }
//        }

//        [HttpGet("allEvents")]
//        public IActionResult GetAllEvents()
//        {
//            var allEvents = _eventService.GetEvents();
//            return Ok(allEvents);
//        }

//        // ==== תיקון 2: הוספת הפונקציה החסרה ====
//        /// <summary>
//        /// מחשב מרחק משוטר למיקום נתון
//        /// </summary>
//        private double CalculateDistanceFromOfficer(PoliceOfficerDTO officer, double lat, double lon)
//        {
//            // כאן תוכל להוסיף חישוב מרחק אמיתי אם יש לך מיקום של השוטר
//            // לעת עתה מחזיר 0 כדי שהקוד יעבוד
//            // ניתן להוסיף מיקום השוטר ל-DTO ולחשב מרחק Haversine

//            // דוגמה לחישוב אם יש מיקום שוטר:
//            // if (officer.CurrentLatitude.HasValue && officer.CurrentLongitude.HasValue)
//            // {
//            //     return CalculateHaversineDistance(
//            //         officer.CurrentLatitude.Value, officer.CurrentLongitude.Value,
//            //         lat, lon
//            //     );
//            // }

//            return 0; // מחזיר 0 - כל השוטרים שווים במרחק
//        }

//        /// <summary>
//        /// חישוב מרחק Haversine בין שתי נקודות גיאוגרפיות
//        /// </summary>
//        private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
//        {
//            const double R = 6371000; // רדיוס כדור הארץ במטרים

//            double lat1Rad = lat1 * Math.PI / 180;
//            double lat2Rad = lat2 * Math.PI / 180;
//            double deltaLat = (lat2 - lat1) * Math.PI / 180;
//            double deltaLon = (lon2 - lon1) * Math.PI / 180;

//            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
//                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
//                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

//            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

//            return R * c;
//        }
//    }

//    // מחלקת בקשה עבור יצירת אירוע עם מיקומים מוכנים
//    public class CreateEventWithPositionsRequest
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
//        public List<PreCalculatedPosition> PreCalculatedPositions { get; set; } = new();
//    }

//    public class PreCalculatedPosition
//    {
//        public double Latitude { get; set; }
//        public double Longitude { get; set; }
//        public bool IsStrategic { get; set; }
//        public long NodeId { get; set; }
//    }
//}

// EventController.cs - גרסה מתוקנת לחלוטין ללא קריאות לקונטרולרים
using Microsoft.AspNetCore.Mvc;
using DTO;
using IBL;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly IGraphManagerService _graphManager;

        public EventController(
            IEventService eventService,
            IKCenterService kCenterService,
            IOfficerAssignmentService officerAssignmentService,
            IStrategicZoneBL strategicZoneBL,
            IGraphManagerService graphManager)
        {
            _eventService = eventService;
            _kCenterService = kCenterService;
            _officerAssignmentService = officerAssignmentService;
            _strategicZoneBL = strategicZoneBL;
            _graphManager = graphManager;
        }

        [HttpPost("create")]
        public IActionResult CreateEvent([FromBody] CreateEventRequest request)
        {
            if (!_graphManager.HasCurrentGraph())
                return BadRequest("אין גרף טעון במערכת.");

            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע.");

            if (request.StrategicZones != null && request.StrategicZones.Count > request.RequiredOfficers)
                return BadRequest($"לא ניתן להציב {request.StrategicZones.Count} אזורים אסטרטגיים עם {request.RequiredOfficers} שוטרים בלבד.");

            try
            {
                // יצירת DTO לאירוע ואזור
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

                // שמירה במסד נתונים
                int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

                // שמירת הגרף דרך GraphManager במקום קריאה ישירה לקונטרולר
                var currentGraph = _graphManager.GetCurrentGraph();
                var currentNodes = _graphManager.GetCurrentNodes();
                var currentBounds = _graphManager.GetNodesInOriginalBounds();

                _graphManager.SaveGraphForEvent(eventId, currentGraph, currentNodes, currentBounds);

                // שמירת אזורים אסטרטגיים
                if (request.StrategicZones != null && request.StrategicZones.Any())
                {
                    foreach (var zone in request.StrategicZones)
                        zone.EventId = eventId;
                    _strategicZoneBL.AddStrategicZones(request.StrategicZones);
                }

                // קבלת צמתים בתחום
                var nodesInBounds = currentBounds
                    .Where(kvp => kvp.Value == true)
                    .Select(kvp => kvp.Key)
                    .ToHashSet();

                Console.WriteLine($"🔍 מספר צמתים בתחום: {nodesInBounds.Count}");

                // יצירת צמתים אסטרטגיים על דרכים אמיתיות
                List<long> strategicNodeIds = new List<long>();

                if (request.StrategicZones != null && request.StrategicZones.Any())
                {
                    Console.WriteLine($"🎯 יוצר {request.StrategicZones.Count} צמתים אסטרטגיים על דרכים:");

                    foreach (var zone in request.StrategicZones)
                    {
                        Console.WriteLine($"\n📍 מעבד אזור: ({zone.Latitude}, {zone.Longitude})");

                        // שימוש בפיצול Way במקום חיפוש צומת קרוב
                        var newStrategicNodeId = currentGraph.CreateStrategicNodeOnWay(
                            zone.Latitude,
                            zone.Longitude,
                            nodesInBounds
                        );

                        if (newStrategicNodeId != -1)
                        {
                            strategicNodeIds.Add(newStrategicNodeId);

                            // עדכון המילונים
                            var actualCoord = currentGraph.Nodes[newStrategicNodeId];
                            currentNodes[newStrategicNodeId] = (actualCoord.Latitude, actualCoord.Longitude);
                            currentBounds[newStrategicNodeId] = true;

                            Console.WriteLine($"✅ נוצר צומת אסטרטגי {newStrategicNodeId} על דרך אמיתית");
                        }
                        else
                        {
                            Console.WriteLine($"❌ כשל ביצירת צומת - לא נמצא קטע דרך מתאים במיקום ({zone.Latitude}, {zone.Longitude})");
                            return BadRequest($"לא ניתן ליצור צומת אסטרטגי במיקום ({zone.Latitude}, {zone.Longitude}) - לא נמצא קטע דרך קרוב");
                        }
                    }

                    strategicNodeIds = strategicNodeIds.Distinct().ToList();
                    Console.WriteLine($"\n🎯 סה\"כ צמתים אסטרטגיים נוצרו על דרכים: {strategicNodeIds.Count}");
                }

                // עדכון רשימת הצמתים המותרים
                var allowedNodesForDistribution = new HashSet<long>(nodesInBounds);
                foreach (var strategicId in strategicNodeIds)
                {
                    allowedNodesForDistribution.Add(strategicId);
                }

                Console.WriteLine($"📊 סה\"כ צמתים זמינים לפיזור: {allowedNodesForDistribution.Count}");

                // פיזור K-Center
                var result = _kCenterService.DistributePolice(
                    currentGraph,
                    request.RequiredOfficers,
                    allowedNodesForDistribution,
                    strategicNodeIds
                );

                // בדיקה שכל הצמתים האסטרטגיים נכללו
                var missingStrategic = strategicNodeIds.Where(id => !result.CenterNodes.Contains(id)).ToList();
                if (missingStrategic.Any())
                {
                    Console.WriteLine($"❌ צמתים אסטרטגיים שלא נכללו: {string.Join(", ", missingStrategic)}");
                    return BadRequest($"האלגוריתם לא הצליח לכלול את כל הצמתים האסטרטגיים. חסרים: {string.Join(", ", missingStrategic)}");
                }

                // שליפת שוטרים זמינים ושיוך
                var selectedNodeIds = result.CenterNodes;
                var availableOfficers = _eventService.GetAvailableOfficersForEvent(
                    eventDto.EventDate,
                    eventDto.StartTime,
                    eventDto.EndTime
                );

                var assignmentDtos = new List<OfficerAssignmentDTO>();
                foreach (var nodeId in selectedNodeIds)
                {
                    if (!currentNodes.TryGetValue(nodeId, out var coord))
                        continue;

                    var availableOfficer = availableOfficers
                        .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
                        .OrderBy(o => CalculateDistanceFromOfficer(o, coord.lat, coord.lon))
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

                _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

                var strategicCount = strategicNodeIds.Count;
                var regularCount = selectedNodeIds.Count - strategicCount;

                return Ok(new
                {
                    EventId = eventId,
                    OfficerCount = assignmentDtos.Count,
                    StrategicOfficers = strategicCount,
                    RegularOfficers = regularCount,
                    NodesCreatedOnRealRoads = strategicNodeIds.Count,
                    Message = strategicCount > 0
                        ? $"אירוע נוצר בהצלחה. נוצרו {strategicNodeIds.Count} צמתים אסטרטגיים על דרכים אמיתיות ושובצו {strategicCount} שוטרים באזורים אסטרטגיים ו-{regularCount} שוטרים נוספים"
                        : "אירוע נוצר בהצלחה ושובצו שוטרים",
                    DebugInfo = new
                    {
                        OriginalStrategicZones = request.StrategicZones?.Count ?? 0,
                        FoundStrategicNodes = strategicNodeIds.Count,
                        TotalNodesInBounds = nodesInBounds.Count,
                        TotalWaySegments = currentGraph.WaySegments.Count,
                        SelectedNodes = selectedNodeIds.Count
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ שגיאה ביצירת אירוע: {ex.Message}");
                return BadRequest($"שגיאה ביצירת האירוע: {ex.Message}");
            }
        }

        [HttpPost("create-with-positions")]
        public IActionResult CreateEventWithPositions([FromBody] CreateEventWithPositionsRequest request)
        {
            if (request.PreCalculatedPositions == null || !request.PreCalculatedPositions.Any())
                return BadRequest("לא נמצא פיזור מוכן של שוטרים");

            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע");

            try
            {
                // יצירת DTO לאירוע
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

                // שמירה במסד נתונים
                int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);

                // שמירת גרף עבור האירוע
                if (_graphManager.HasCurrentGraph())
                {
                    var currentGraph = _graphManager.GetCurrentGraph();
                    var currentNodes = _graphManager.GetCurrentNodes();
                    var currentBounds = _graphManager.GetNodesInOriginalBounds();

                    _graphManager.SaveGraphForEvent(eventId, currentGraph, currentNodes, currentBounds);
                }

                // שמירת אזורים אסטרטגיים
                if (request.StrategicZones != null && request.StrategicZones.Any())
                {
                    foreach (var zone in request.StrategicZones)
                        zone.EventId = eventId;
                    _strategicZoneBL.AddStrategicZones(request.StrategicZones);
                }

                // שליפת שוטרים זמינים
                var availableOfficers = _eventService.GetAvailableOfficersForEvent(
                    eventDto.EventDate,
                    eventDto.StartTime,
                    eventDto.EndTime
                );

                // שימוש בפיזור המוכן מראש
                var assignmentDtos = new List<OfficerAssignmentDTO>();
                int strategicCount = 0;

                Console.WriteLine($"💾 משתמש בפיזור מוכן עם {request.PreCalculatedPositions.Count} מיקומים");

                foreach (var position in request.PreCalculatedPositions)
                {
                    // מציאת שוטר זמין
                    var availableOfficer = availableOfficers
                        .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
                        .OrderBy(o => CalculateDistanceFromOfficer(o, position.Latitude, position.Longitude))
                        .FirstOrDefault();

                    if (availableOfficer != null)
                    {
                        assignmentDtos.Add(new OfficerAssignmentDTO
                        {
                            PoliceOfficerId = availableOfficer.PoliceOfficerId,
                            EventId = eventId,
                            Latitude = position.Latitude,
                            Longitude = position.Longitude
                        });

                        if (position.IsStrategic)
                        {
                            strategicCount++;
                            Console.WriteLine($"🎯 שוטר אסטרטגי הוצב במיקום ({position.Latitude}, {position.Longitude})");
                        }
                        else
                        {
                            Console.WriteLine($"👮 שוטר רגיל הוצב במיקום ({position.Latitude}, {position.Longitude})");
                        }
                    }
                }

                // שמירת השיוכים
                _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

                var regularCount = assignmentDtos.Count - strategicCount;

                Console.WriteLine($"✅ נוצר אירוע {eventId} עם {assignmentDtos.Count} שוטרים ({strategicCount} אסטרטגיים, {regularCount} רגילים)");

                return Ok(new
                {
                    EventId = eventId,
                    OfficerCount = assignmentDtos.Count,
                    StrategicOfficers = strategicCount,
                    RegularOfficers = regularCount,
                    Message = strategicCount > 0
                        ? $"אירוע נוצר בהצלחה. שובצו {strategicCount} שוטרים באזורים אסטרטגיים ו-{regularCount} שוטרים נוספים"
                        : "אירוע נוצר בהצלחה ושובצו שוטרים"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ שגיאה ביצירת אירוע: {ex.Message}");
                return BadRequest($"שגיאה ביצירת האירוע: {ex.Message}");
            }
        }

        [HttpDelete("{eventId}")]
        public IActionResult DeleteEvent(int eventId)
        {
            try
            {
                _eventService.DeleteEvent(eventId);
                _graphManager.RemoveGraphForEvent(eventId);
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

        /// <summary>
        /// מחשב מרחק משוטר למיקום נתון
        /// </summary>
        private double CalculateDistanceFromOfficer(PoliceOfficerDTO officer, double lat, double lon)
        {
            // כאן תוכל להוסיף חישוב מרחק אמיתי אם יש לך מיקום של השוטר
            // לעת עתה מחזיר 0 כדי שהקוד יעבוד
            // ניתן להוסיף מיקום השוטר ל-DTO ולחשב מרחק Haversine

            // דוגמה לחישוב אם יש מיקום שוטר:
            // if (officer.CurrentLatitude.HasValue && officer.CurrentLongitude.HasValue)
            // {
            //     return CalculateHaversineDistance(
            //         officer.CurrentLatitude.Value, officer.CurrentLongitude.Value,
            //         lat, lon
            //     );
            // }

            return 0; // מחזיר 0 - כל השוטרים שווים במרחק
        }

        /// <summary>
        /// חישוב מרחק Haversine בין שתי נקודות גיאוגרפיות
        /// </summary>
        private double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371000; // רדיוס כדור הארץ במטרים

            double lat1Rad = lat1 * Math.PI / 180;
            double lat2Rad = lat2 * Math.PI / 180;
            double deltaLat = (lat2 - lat1) * Math.PI / 180;
            double deltaLon = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }
    }

    // מחלקות Request
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

    public class CreateEventWithPositionsRequest
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
        public List<PreCalculatedPosition> PreCalculatedPositions { get; set; } = new();
    }

    public class PreCalculatedPosition
    {
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public bool IsStrategic { get; set; }
        public long NodeId { get; set; }
    }
}