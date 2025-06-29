

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
        private readonly IStrategicZoneBL _strategicZoneBL;

        public CallManagementService(
            ICallService callService,
            ICallAssignmentService callAssignmentService,
            IOfficerAssignmentService officerAssignmentService,
            IKCenterService kCenterService,
            IGraphManagerService graphManager,
            IStrategicZoneBL strategicZoneBL) 
        {
            _callService = callService;
            _callAssignmentService = callAssignmentService;
            _officerAssignmentService = officerAssignmentService;
            _kCenterService = kCenterService;
            _graphManager = graphManager;
            _strategicZoneBL = strategicZoneBL;
        }

        public CallCreationResponse CreateCall(CallDTO request)
        {
            // בדיקות תקינות
            if (request.Latitude == 0 || request.Longitude == 0)
                throw new ArgumentException("יש להזין מיקום לקריאה (Latitude/Longitude).");

            if (request.EventId <= 0)
                throw new ArgumentException("EventId נדרש לקריאה.");

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

            //  מציאת שוטרים זמינים (כולל בדיקת קונפליקט קריאות)
            var availableOfficers = GetAvailableOfficersForEvent(request.EventId.Value);
            Console.WriteLine($"קריאה חדשה: זמינים {availableOfficers.Count} שוטרים, דרושים {request.RequiredOfficers}");

            // בחירת השוטרים הקרובים ביותר ועדכון מיקומם
            var chosenOfficers = SelectClosestOfficersAndUpdateLocation(availableOfficers, request, callId);

            // השוטרים שנשארו לפיזור מחדש
            var remainingOfficers = availableOfficers
                .Where(o => !chosenOfficers.Any(c => c.PoliceOfficerId == o.PoliceOfficerId))
                .ToList();

            //  פיזור מחדש של השוטרים הנותרים עם אזורים אסטרטגיים מהגרף
            var reassignedOfficers = RedistributeRemainingOfficersWithExistingStrategicZones(
                remainingOfficers, graphData, request.EventId.Value);

            // עדכון סטטוס הקריאה
            callDto.CallId = callId;
            callDto.Status = chosenOfficers.Any() ? "InTreatment" : "Open";

            return CreateCallResponse(callId, request, chosenOfficers, reassignedOfficers, availableOfficers.Count);
        }

        //  מתודה מתוקנת - כולל בדיקת קונפליקט קריאות
        private List<OfficerAssignmentDTO> GetAvailableOfficersForEvent(int eventId)
        {
            // שליפת כל השוטרים של האירוע
            var allEventOfficers = _officerAssignmentService.GetAssignmentsByEventId(eventId);

            //  שליפת השוטרים העסוקים בקריאות 
            var busyOfficers = GetOfficersBusyInAllCalls(eventId);

            Console.WriteLine($"סה\"כ שוטרים באירוע: {allEventOfficers.Count}");
            Console.WriteLine($"שוטרים עסוקים בקריאות: {busyOfficers.Count}");

            // החזרת הזמינים
            var availableOfficers = allEventOfficers
                .Where(o => !busyOfficers.Contains(o.PoliceOfficerId))
                .ToList();

            Console.WriteLine($"שוטרים זמינים: {availableOfficers.Count}");

            return availableOfficers;
        }

        //  מתודה מתוקנת - בדיקה נרחבת יותר של שוטרים עסוקים
        private HashSet<int> GetOfficersBusyInAllCalls(int eventId)
        {
            // שליפת כל הקריאות הפעילות באירוע
            var activeCalls = _callService.GetCallsByEvent(eventId)
                .Where(c => c.Status == "InTreatment" || c.Status == "Open") 
                .ToList();

            Console.WriteLine($"קריאות פעילות באירוע {eventId}: {activeCalls.Count}");

            var busyOfficers = new HashSet<int>();
            foreach (var call in activeCalls)
            {
                var assignments = _callAssignmentService.GetAssignmentsByCall(call.CallId);
                foreach (var assignment in assignments)
                {
                    busyOfficers.Add(assignment.PoliceOfficerId);
                    Console.WriteLine($"  שוטר {assignment.PoliceOfficerId} עסוק בקריאה {call.CallId}");
                }
            }

            return busyOfficers;
        }

        // בחירת השוטרים הקרובים ועדכון מיקומים
        private List<CallAssignmentDTO> SelectClosestOfficersAndUpdateLocation(
            List<OfficerAssignmentDTO> availableOfficers,
            CallDTO request,
            int callId)
        {
            //מיון המיקום לפי מרחק הנסיעה של כל שוטר
            var chosen = availableOfficers
                .OrderBy(o => GeoUtils.GetDrivingDistance(o.Latitude, o.Longitude, request.Latitude, request.Longitude))
                .Take(request.RequiredOfficers)
                .ToList();

            Console.WriteLine($"נבחרו {chosen.Count} שוטרים קרובים לקריאה:");
            foreach (var officer in chosen)
            {
                var distance = GeoUtils.CalculateDistance(officer.Latitude, officer.Longitude, request.Latitude, request.Longitude);
                Console.WriteLine($"  שוטר {officer.PoliceOfficerId}: מרחק {distance:F2} מטרים");
            }

            var callAssignments = chosen.Select(o => new CallAssignmentDTO
            {
                PoliceOfficerId = o.PoliceOfficerId,
                CallId = callId,
                AssignmentTime = DateTime.UtcNow
            }).ToList();

            _callAssignmentService.AssignOfficersToCall(callAssignments);

            var officerLocationUpdates = chosen.Select(o => new OfficerAssignmentDTO
            {
                PoliceOfficerId = o.PoliceOfficerId,
                EventId = o.EventId,
                Latitude = request.Latitude,
                Longitude = request.Longitude
            }).ToList();

            Console.WriteLine($"מעדכן מיקום {officerLocationUpdates.Count} שוטרים למיקום הקריאה ({request.Latitude}, {request.Longitude})");
            _officerAssignmentService.UpdateOfficerAssignments(officerLocationUpdates);

            return callAssignments;
        }

        // שימוש בצמתים האסטרטגיים הקיימים מהגרף
        private List<OfficerAssignmentDTO> RedistributeRemainingOfficersWithExistingStrategicZones(
            List<OfficerAssignmentDTO> remainingOfficers,
            GraphData graphData,
            int eventId)
        {
            if (!remainingOfficers.Any())
            {
                Console.WriteLine("אין שוטרים נותרים לפיזור מחדש");
                return new List<OfficerAssignmentDTO>();
            }

            Console.WriteLine($"מתחיל פיזור מחדש של {remainingOfficers.Count} שוטרים נותרים");

            var graph = graphData.Graph;
            var nodesInBounds = graphData.NodesInOriginalBounds
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToHashSet();

            //  זיהוי צמתים אסטרטגיים קיימים בגרף
            var strategicNodeIds = FindExistingStrategicNodesInGraph(eventId, graphData);

            Console.WriteLine($"נמצאו {strategicNodeIds.Count} צמתים אסטרטגיים קיימים בגרף האירוע");

            //  הרצת K-Center עם הצמתים האסטרטגיים הקיימים
            var result = _kCenterService.DistributePolice(
                graph,
                remainingOfficers.Count,
                nodesInBounds,
                strategicNodeIds.Any() ? strategicNodeIds : null);

            Console.WriteLine($"K-Center מצא {result.CenterNodes.Count} מיקומים אופטימליים");

            if (strategicNodeIds.Any())
            {
                var strategicIncluded = strategicNodeIds.Count(id => result.CenterNodes.Contains(id));
                Console.WriteLine($"מתוכם {strategicIncluded} אסטרטגיים (מתוך {strategicNodeIds.Count} שהיו)");
            }

            // עדכון המיקומים הקיימים
            var nodeToCoord = graphData.Nodes;
            var updatedAssignments = new List<OfficerAssignmentDTO>();

            for (int i = 0; i < Math.Min(result.CenterNodes.Count, remainingOfficers.Count); i++)
            {
                var nodeId = result.CenterNodes[i];
                if (nodeToCoord.TryGetValue(nodeId, out var coord))
                {
                    var officer = remainingOfficers[i];
                    updatedAssignments.Add(new OfficerAssignmentDTO
                    {
                        PoliceOfficerId = officer.PoliceOfficerId,
                        EventId = eventId,
                        Latitude = coord.lat,
                        Longitude = coord.lon
                    });

                    var isStrategic = strategicNodeIds.Contains(nodeId) ? " (אסטרטגי)" : "";
                    Console.WriteLine($"  שוטר {officer.PoliceOfficerId}: מיקום חדש ({coord.lat:F6}, {coord.lon:F6}){isStrategic}");
                }
            }

            if (updatedAssignments.Any())
            {
                Console.WriteLine($"מעדכן מיקומים במסד עבור {updatedAssignments.Count} שוטרים");
                _officerAssignmentService.UpdateOfficerAssignments(updatedAssignments);
            }

            return updatedAssignments;
        }

        //  מתודה חדשה - זיהוי צמתים אסטרטגיים קיימים בגרף
        private List<long> FindExistingStrategicNodesInGraph(int eventId, GraphData graphData)
        {
            var strategicNodeIds = new List<long>();

            try
            {
                // שליפת האזורים האסטרטגיים מהמסד
                var strategicZones = _strategicZoneBL.GetStrategicZonesForEvent(eventId);

                if (!strategicZones.Any())
                {
                    Console.WriteLine($"לא נמצאו אזורים אסטרטגיים לאירוע {eventId}");
                    return strategicNodeIds;
                }

                Console.WriteLine($"נמצאו {strategicZones.Count} אזורים אסטרטגיים במסד לאירוע {eventId}");

                // חיפוש הצמתים המתאימים בגרף הקיים
                var graphNodes = graphData.Nodes;

                foreach (var zone in strategicZones)
                {
                    // חיפוש הצומת הכי קרוב לאזור האסטרטגי
                    var nearestNodeId = FindNearestNodeInGraphData(
                        zone.Latitude, zone.Longitude, graphData);

                    if (nearestNodeId != -1)
                    {
                        strategicNodeIds.Add(nearestNodeId);
                        Console.WriteLine($"  אזור אסטרטגי ({zone.Latitude:F6}, {zone.Longitude:F6}) → צומת {nearestNodeId}");
                    }
                    else
                    {
                        Console.WriteLine($"  אזור אסטרטגי ({zone.Latitude:F6}, {zone.Longitude:F6}) → לא נמצא צומת מתאים");
                    }
                }

                // הסרת כפילויות
                strategicNodeIds = strategicNodeIds.Distinct().ToList();
                Console.WriteLine($"סה\"כ צמתים אסטרטגיים ייחודיים: {strategicNodeIds.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"שגיאה בזיהוי צמתים אסטרטגיים: {ex.Message}");
            }

            return strategicNodeIds;
        }

        //  מתודת עזר - חיפוש הצומת הקרוב ביותר בגרף
        private long FindNearestNodeInGraphData(
            double latitude, double longitude,
            GraphData graphData)
        {
            double minDistance = double.MaxValue;
            long nearestNodeId = -1;

            var nodesInBounds = graphData.NodesInOriginalBounds
                .Where(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToHashSet();

            foreach (var nodeId in nodesInBounds)
            {
                if (graphData.Nodes.TryGetValue(nodeId, out var nodeCoord))
                {
                    var distance = GeoUtils.CalculateDistance(
                        latitude, longitude,
                        nodeCoord.lat, nodeCoord.lon);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearestNodeId = nodeId;
                    }
                }
            }

            // החזרת הצומת רק אם הוא קרוב מספיק (פחות מ-50 מטר)
            return minDistance < 50 ? nearestNodeId : -1;
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
                result.Add(new AssignedOfficerResponse
                {
                    OfficerId = assignment.PoliceOfficerId,
                    OfficerLocation = new LocationResponse
                    {
                        Latitude = request.Latitude,
                        Longitude = request.Longitude
                    },
                    DistanceToCall = 0
                });
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