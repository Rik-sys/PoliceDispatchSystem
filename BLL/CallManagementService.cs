using DTO;
using IBL;
using Utilities;
using static DTO.CallResponsesDTO;

namespace BLL
{
    public class CallManagementService : ICallManagementService
    {
        private readonly ICallService _callService;
        private readonly ICallAssignmentService _callAssignmentService;
        private readonly IOfficerAssignmentService _officerAssignmentService;
        private readonly IKCenterService _kCenterService;
        private readonly IGraphManagerService _graphManager;

        public CallManagementService(
            ICallService callService,
            ICallAssignmentService callAssignmentService,
            IOfficerAssignmentService officerAssignmentService,
            IKCenterService kCenterService,
            IGraphManagerService graphManager)
        {
            _callService = callService;
            _callAssignmentService = callAssignmentService;
            _officerAssignmentService = officerAssignmentService;
            _kCenterService = kCenterService;
            _graphManager = graphManager;
        }

        public CallCreationResponse CreateCall(CallDTO request)
        {
            // בדיקות תקינות
            if (request.Latitude == 0 || request.Longitude == 0)
                throw new ArgumentException("יש להזין מיקום לקריאה (Latitude/Longitude).");

            if (request.EventId <= 0)
                throw new ArgumentException("EventId נדרש לקריאה.");

            // שליפת גרף האירוע
            if (!request.EventId.HasValue)
                throw new ArgumentException("EventId נדרש לקריאה.");

            var graphData = _graphManager.GetGraphForEvent(request.EventId.Value);
            if (graphData == null)
                throw new InvalidOperationException("לא קיים גרף עבור האירוע המבוקש.");

            // יצירת הקריאה
            var callDto = new CallDTO
            {
                EventId = request.EventId,
                RequiredOfficers = request.RequiredOfficers,
                ContactPhone = request.ContactPhone,
                UrgencyLevel = request.UrgencyLevel,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                CallTime = DateTime.UtcNow,
                Status = "Open"
            };

            int callId = _callService.CreateCall(callDto);

            // מציאת שוטרים זמינים
            if (!request.EventId.HasValue)
                throw new ArgumentException("EventId נדרש לקריאה.");

            var availableOfficers = GetAvailableOfficersForEvent(request.EventId.Value);
            Console.WriteLine($" קריאה חדשה: זמינים {availableOfficers.Count} שוטרים, דרושים {request.RequiredOfficers}");

            // בחירת השוטרים הקרובים ביותר
            var chosenOfficers = SelectClosestOfficers(availableOfficers, request, callId);

            // השוטרים שנשארו לפיזור מחדש
            var remainingOfficers = availableOfficers
                .Where(o => !chosenOfficers.Any(c => c.PoliceOfficerId == o.PoliceOfficerId))
                .ToList();

            // פיזור מחדש של השוטרים הנותרים
            var reassignedOfficers = RedistributeRemainingOfficers(remainingOfficers, graphData, request.EventId.Value);

            return CreateCallResponse(callId, request, chosenOfficers, reassignedOfficers, availableOfficers.Count);
        }

        private List<OfficerAssignmentDTO> GetAvailableOfficersForEvent(int eventId)
        {
            // שליפת כל השוטרים של האירוע
            var allEventOfficers = _officerAssignmentService.GetAssignmentsByEventId(eventId);

            // שליפת השוטרים העסוקים בקריאות
            var busyOfficers = GetOfficersBusyInEvent(eventId);

            // החזרת הזמינים
            return allEventOfficers
                .Where(o => !busyOfficers.Contains(o.PoliceOfficerId))
                .ToList();
        }

        private HashSet<int> GetOfficersBusyInEvent(int eventId)
        {
            var activeCalls = _callService.GetCallsByEvent(eventId)
                .Where(c => c.Status == "Active")
                .ToList();

            var busyOfficers = new HashSet<int>();
            foreach (var call in activeCalls)
            {
                var assignments = _callAssignmentService.GetAssignmentsByCall(call.CallId);
                foreach (var assignment in assignments)
                {
                    busyOfficers.Add(assignment.PoliceOfficerId);
                }
            }
            return busyOfficers;
        }

        private List<CallAssignmentDTO> SelectClosestOfficers(
            List<OfficerAssignmentDTO> availableOfficers,
            CallDTO request,
            int callId)
        {
            var chosen = availableOfficers
                .OrderBy(o => GeoUtils.CalculateDistance(o.Latitude, o.Longitude, request.Latitude, request.Longitude))
                .Take(request.RequiredOfficers)
                .ToList();

            var callAssignments = chosen.Select(o => new CallAssignmentDTO
            {
                PoliceOfficerId = o.PoliceOfficerId,
                CallId = callId
            }).ToList();

            _callAssignmentService.AssignOfficersToCall(callAssignments);
            return callAssignments;
        }

        private List<OfficerAssignmentDTO> RedistributeRemainingOfficers(
            List<OfficerAssignmentDTO> remainingOfficers,
            GraphData graphData,
            int eventId)
        {
            if (!remainingOfficers.Any())
                return new List<OfficerAssignmentDTO>();

            var graph = graphData.Graph;
            var nodesInBounds = graphData.NodesInOriginalBounds
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToHashSet();

            // הרצת K-Center
            var result = _kCenterService.DistributePolice(graph, remainingOfficers.Count, nodesInBounds);

            // שיוך השוטרים למיקומים החדשים
            var nodeToCoord = graphData.Nodes;
            var reassigned = new List<OfficerAssignmentDTO>();

            foreach (var nodeId in result.CenterNodes)
            {
                if (!nodeToCoord.TryGetValue(nodeId, out var coord))
                    continue;

                var officer = remainingOfficers
                    .Where(o => !reassigned.Any(a => a.PoliceOfficerId == o.PoliceOfficerId))
                    .OrderBy(o => GeoUtils.CalculateDistance(o.Latitude, o.Longitude, coord.lat, coord.lon))
                    .FirstOrDefault();

                if (officer != null)
                {
                    reassigned.Add(new OfficerAssignmentDTO
                    {
                        PoliceOfficerId = officer.PoliceOfficerId,
                        EventId = eventId,
                        Latitude = coord.lat,
                        Longitude = coord.lon
                    });
                }
            }

            if (reassigned.Any())
            {
                _officerAssignmentService.AddOfficerAssignments(reassigned);
            }

            return reassigned;
        }

        private CallCreationResponse CreateCallResponse(
            int callId,
            CallDTO request,
            List<CallAssignmentDTO> assignedOfficers,
            List<OfficerAssignmentDTO> reassignedOfficers,
            int totalAvailable)
        {
            return new CallCreationResponse
            {
                CallId = callId,
                AssignedToCall = assignedOfficers.Count,
                ReassignedOfficers = reassignedOfficers.Count,
                TotalAvailableOfficers = totalAvailable,
                Message = reassignedOfficers.Any()
                    ? $"קריאה נוצרה בהצלחה. שויכו {assignedOfficers.Count} שוטרים לקריאה ו-{reassignedOfficers.Count} שוטרים פוזרו מחדש."
                    : $"קריאה נוצרה בהצלחה. שויכו {assignedOfficers.Count} שוטרים לקריאה.",
                CallInfo = new CallInfoResponse
                {
                    Id = callId,
                    CallLocation = new LocationResponse { Latitude = request.Latitude, Longitude = request.Longitude },
                    RequiredOfficers = request.RequiredOfficers,
                    ActualAssigned = assignedOfficers.Count
                },
                AssignedOfficersList = CreateAssignedOfficersResponse(assignedOfficers, request),
                ReassignedOfficersList = CreateReassignedOfficersResponse(reassignedOfficers)
            };
        }

        private List<AssignedOfficerResponse> CreateAssignedOfficersResponse(
            List<CallAssignmentDTO> assignments,
            CallDTO request)
        {
            var result = new List<AssignedOfficerResponse>();
            foreach (var assignment in assignments)
            {
                var officer = _officerAssignmentService.GetAssignmentsByEventId(request.EventId.Value)
                                    .FirstOrDefault(o => o.PoliceOfficerId == assignment.PoliceOfficerId);

                if (officer != null)
                {
                    result.Add(new AssignedOfficerResponse
                    {
                        OfficerId = officer.PoliceOfficerId,
                        OfficerLocation = new LocationResponse
                        {
                            Latitude = officer.Latitude,
                            Longitude = officer.Longitude
                        },
                        DistanceToCall = GeoUtils.CalculateDistance(
                            officer.Latitude, officer.Longitude,
                            request.Latitude, request.Longitude)
                    });
                }
            }
            return result;
        }

        private List<ReassignedOfficerResponse> CreateReassignedOfficersResponse(
            List<OfficerAssignmentDTO> reassignments)
        {
            return reassignments.Select(r => new ReassignedOfficerResponse
            {
                OfficerId = r.PoliceOfficerId,
                NewOfficerLocation = new LocationResponse
                {
                    Latitude = r.Latitude,
                    Longitude = r.Longitude
                }
            }).ToList();
        }
    }
}