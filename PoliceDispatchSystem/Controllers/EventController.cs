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
using Microsoft.AspNetCore.Mvc;
using DTO;
using DBEntities.Models;
using IBL;
using PoliceDispatchSystem.Controllers;

namespace PoliceDispatchSystem.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventController : ControllerBase
    {
        private readonly IEventService _eventService;
        private readonly IKCenterService _kCenterService;
        private readonly IOfficerAssignmentService _officerAssignmentService; // הוספה חדשה

        public EventController(
            IEventService eventService,
            IKCenterService kCenterService,
            IOfficerAssignmentService officerAssignmentService) // הוספה חדשה
        {
            _eventService = eventService;
            _kCenterService = kCenterService;
            _officerAssignmentService = officerAssignmentService; // הוספה חדשה
        }

        [HttpPost("create")]
        public IActionResult CreateEvent([FromBody] CreateEventRequest request)
        {
            if (GraphController.LatestGraph == null || GraphController.LatestNodes == null)
                return BadRequest("אין גרף טעון במערכת.");

            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
                return BadRequest("נדרשות לפחות 4 נקודות לתחום האירוע.");

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

            // 3. סינון צמתים בתחום
            var nodesInBounds = GraphController.NodesInOriginalBounds
                .Where(kvp => kvp.Value == true)
                .Select(kvp => kvp.Key)
                .ToHashSet();

            // 4. פיזור K-Center
            var result = _kCenterService.DistributePolice(GraphController.LatestGraph, request.RequiredOfficers, nodesInBounds);
            var selectedNodeIds = result.CenterNodes;

            var nodeToCoord = GraphController.LatestNodes;

            // 5. שליפת כל השוטרים הפנויים (עכשיו מחזיר DTO)
            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
                eventDto.EventDate,
                eventDto.StartTime,
                eventDto.EndTime
            );

            // 6. יצירת רשימת DTOs לשיוכים במקום עבודה ישירה על Entity
            var assignmentDtos = new List<OfficerAssignmentDTO>();

            foreach (var nodeId in selectedNodeIds)
            {
                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
                    continue;

                // מציאת השוטר הכי קרוב שלא שובץ עדיין
                var availableOfficer = availableOfficers
                    .Where(o => !assignmentDtos.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
                    .OrderBy(o => GetDistanceFromOfficer(o, coord.lat, coord.lon))
                    .FirstOrDefault();

                if (availableOfficer != null)
                {
                    // יצירת DTO במקום Entity
                    assignmentDtos.Add(new OfficerAssignmentDTO
                    {
                        PoliceOfficerId = availableOfficer.PoliceOfficerId,
                        EventId = eventId,
                        Latitude = coord.lat,
                        Longitude = coord.lon
                    });
                }
            }

            // 7. שמירת השיוכים דרך שכבת השירות (BLL)
            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

            return Ok(new
            {
                EventId = eventId,
                OfficerCount = assignmentDtos.Count,
                Message = "אירוע נוצר בהצלחה ושובצו שוטרים"
            });
        }

        private double GetDistanceFromOfficer(PoliceOfficerDTO officer, double lat, double lon)
        {
            return 0; // כאן תצטרך להוסיף חישוב מרחק אמיתי
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

        public List<List<double>> SelectedArea { get; set; } = new(); // 4 נקודות [lat, lon]
    }
}