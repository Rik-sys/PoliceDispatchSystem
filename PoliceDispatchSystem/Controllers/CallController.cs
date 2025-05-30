// CallController.cs - מעודכן לשימוש בגרפים לפי מזהה אירוע
using DTO;
using IBL;
using Microsoft.AspNetCore.Mvc;
using PoliceDispatchSystem.Controllers;

namespace PoliceDispatchSystem.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CallController : ControllerBase
    {
        private readonly ICallService _callService;
        private readonly ICallAssignmentService _callAssignmentService;
        private readonly IOfficerAssignmentService _officerAssignmentService;
        private readonly IKCenterService _kCenterService;

        public CallController(ICallService callService, ICallAssignmentService callAssignmentService, IOfficerAssignmentService officerAssignmentService, IKCenterService kCenterService)
        {
            _callService = callService;
            _callAssignmentService = callAssignmentService;
            _officerAssignmentService = officerAssignmentService;
            _kCenterService = kCenterService;
        }

        [HttpPost("create")]
        public IActionResult CreateCall([FromBody] CallDTO callDto)
        {
            if (callDto.Latitude == 0 || callDto.Longitude == 0)
                return BadRequest("יש להזין מיקום לקריאה (Latitude/Longitude).");

            if (callDto.EventId == null)
                return BadRequest("EventId נדרש לקריאה.");

            var graphData = GraphController.GetGraphForEvent(callDto.EventId.Value);
            if (graphData == null)
                return BadRequest("לא קיים גרף עבור האירוע המבוקש.");

            int callId = _callService.CreateCall(callDto);

            var allEventOfficers = _officerAssignmentService.GetAssignmentsByEventId(callDto.EventId.Value);
            var assignedToCall = new HashSet<int>(_callAssignmentService
                .GetAssignmentsByCall(callId)
                .Select(ca => ca.PoliceOfficerId));

            var available = allEventOfficers
                .Where(o => !assignedToCall.Contains(o.PoliceOfficerId))
                .ToList();

            var chosen = available
                .OrderBy(o => GetDistance(o.Latitude, o.Longitude, callDto.Latitude, callDto.Longitude))
                .Take(callDto.RequiredOfficers)
                .ToList();

            var callAssignments = chosen.Select(o => new CallAssignmentDTO
            {
                PoliceOfficerId = o.PoliceOfficerId,
                CallId = callId
            }).ToList();

            _callAssignmentService.AssignOfficersToCall(callAssignments);

            var remaining = available
                .Where(o => !chosen.Any(c => c.PoliceOfficerId == o.PoliceOfficerId))
                .ToList();

            var graph = graphData.Graph;
            var nodesInBounds = graphData.NodesInOriginalBounds
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToHashSet();

            var result = _kCenterService.DistributePolice(graph, remaining.Count, nodesInBounds);
            var selectedNodes = result.CenterNodes;

            var nodeToCoord = graphData.Nodes;
            var reassigned = new List<OfficerAssignmentDTO>();

            foreach (var nodeId in selectedNodes)
            {
                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
                    continue;

                var officer = remaining
                    .Where(o => !reassigned.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
                    .OrderBy(o => GetDistance(o.Latitude, o.Longitude, coord.lat, coord.lon))
                    .FirstOrDefault();

                if (officer != null)
                {
                    reassigned.Add(new OfficerAssignmentDTO
                    {
                        PoliceOfficerId = officer.PoliceOfficerId,
                        EventId = callDto.EventId.Value,
                        Latitude = coord.lat,
                        Longitude = coord.lon
                    });
                }
            }

            _officerAssignmentService.AddOfficerAssignments(reassigned);

            return Ok(new
            {
                CallId = callId,
                Assigned = callAssignments.Count,
                Reassigned = reassigned.Count
            });
        }

        private double GetDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var R = 6371000;
            var dLat = Math.PI / 180 * (lat2 - lat1);
            var dLon = Math.PI / 180 * (lon2 - lon1);

            var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                    Math.Cos(Math.PI / 180 * lat1) * Math.Cos(Math.PI / 180 * lat2) *
                    Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}
