
//using DTO;
//using IBL;
//using Microsoft.Extensions.Logging;
//using static DTO.EventRequestsDTO;

//namespace BLL
//{
//    public class EventManagementService : IEventManagementService
//    {
//        private readonly IEventService _eventService;
//        private readonly IKCenterService _kCenterService;
//        private readonly IOfficerAssignmentService _officerAssignmentService;
//        private readonly IStrategicZoneBL _strategicZoneBL;
//        private readonly IGraphManagerService _graphManager;
//        private readonly IPoliceOfficerService _policeOfficerService;
//        private readonly ILogger<EventManagementService> _logger;

//        public EventManagementService(
//            IEventService eventService,
//            IKCenterService kCenterService,
//            IOfficerAssignmentService officerAssignmentService,
//            IStrategicZoneBL strategicZoneBL,
//            IGraphManagerService graphManager,
//            IPoliceOfficerService policeOfficerService,
//            ILogger<EventManagementService> logger)
//        {
//            _eventService = eventService;
//            _kCenterService = kCenterService;
//            _officerAssignmentService = officerAssignmentService;
//            _strategicZoneBL = strategicZoneBL;
//            _graphManager = graphManager;
//            _policeOfficerService = policeOfficerService;
//            _logger = logger;
//        }

//        public async Task<EventCreationResultDTO> CreateEventWithAutoDistribution(CreateEventRequestDTO request)
//        {
//            var result = new EventCreationResultDTO();

//            try
//            {
//                _logger.LogInformation($"🚀 Starting CreateEventWithAutoDistribution: {request.Name}");
//                _logger.LogInformation($"📅 Event details: Date={request.StartDate}, Time={request.StartTime}-{request.EndTime}, Officers={request.RequiredOfficers}");

//                // בדיקות תקינות ראשוניות
//                var validationResult = ValidateEventRequest(request);
//                if (!validationResult.IsValid)
//                {
//                    _logger.LogWarning($"❌ Validation failed: {string.Join(", ", validationResult.Errors)}");
//                    result.Errors = validationResult.Errors;
//                    return result;
//                }

//                _logger.LogInformation($"✅ Validation passed, creating event: {request.Name}");

//                // יצירת DTO לאירוע ואזור
//                var (eventDto, zoneDto) = CreateEventAndZoneDTOs(request);
//                _logger.LogInformation($"📋 Created DTOs - Event: {eventDto.EventName}, Date: {eventDto.EventDate}, Time: {eventDto.StartTime}-{eventDto.EndTime}");

//                // שמירה במסד נתונים
//                int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);
//                result.EventId = eventId;
//                _logger.LogInformation($"💾 Event saved with ID: {eventId}");

//                // שמירת הגרף
//                await SaveGraphForEvent(eventId);
//                _logger.LogInformation($"🗺️ Graph saved for event {eventId}");

//                // טיפול באזורים אסטרטגיים
//                var strategicNodeIds = await HandleStrategicZones(request, eventId);
//                _logger.LogInformation($"🎯 Strategic zones processed: {strategicNodeIds.Count} nodes created");

//                // ביצוע פיזור K-Center
//                _logger.LogInformation($"🎲 Starting K-Center distribution for {request.RequiredOfficers} officers");
//                var distributionResult = await PerformKCenterDistribution(request, strategicNodeIds);
//                if (!distributionResult.Success)
//                {
//                    _logger.LogError($"❌ K-Center distribution failed: {distributionResult.ErrorMessage}");
//                    result.Errors.Add(distributionResult.ErrorMessage);
//                    return result;
//                }
//                _logger.LogInformation($"✅ K-Center distribution successful: {distributionResult.CenterNodes.Count} positions selected");

//                // שיוך שוטרים למיקומים
//                _logger.LogInformation($"👮 Starting officer assignment to {distributionResult.CenterNodes.Count} positions");
//                var assignmentResult = await AssignOfficersToPositions(
//                    eventDto, distributionResult.CenterNodes, eventId, strategicNodeIds);

//                // הכנת התוצאה
//                result.Success = true;
//                result.OfficerCount = assignmentResult.TotalAssigned;
//                result.StrategicOfficers = strategicNodeIds.Count;
//                result.RegularOfficers = assignmentResult.TotalAssigned - strategicNodeIds.Count;
//                result.NodesCreatedOnRealRoads = strategicNodeIds.Count;
//                result.Message = strategicNodeIds.Count > 0
//                    ? $"אירוע נוצר בהצלחה. נוצרו {strategicNodeIds.Count} צמתים אסטרטגיים על דרכים אמיתיות ושובצו {result.StrategicOfficers} שוטרים באזורים אסטרטגיים ו-{result.RegularOfficers} שוטרים נוספים"
//                    : "אירוע נוצר בהצלחה ושובצו שוטרים";

//                result.DebugInfo = CreateDebugInfo(request, strategicNodeIds, distributionResult);

//                _logger.LogInformation($"🎉 Event {eventId} created successfully with {result.OfficerCount} officers");

//                return result;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"❌ Error creating event: {request.Name}");
//                result.Errors.Add($"שגיאה ביצירת האירוע: {ex.Message}");
//                return result;
//            }
//        }

//        public async Task<EventCreationResultDTO> CreateEventWithPreCalculatedPositions(CreateEventWithPositionsRequestDTO request)
//        {
//            var result = new EventCreationResultDTO();

//            try
//            {
//                // בדיקות תקינות
//                if (request.PreCalculatedPositions == null || !request.PreCalculatedPositions.Any())
//                {
//                    result.Errors.Add("לא נמצא פיזור מוכן של שוטרים");
//                    return result;
//                }

//                if (request.SelectedArea == null || request.SelectedArea.Count < 4)
//                {
//                    result.Errors.Add("נדרשות לפחות 4 נקודות לתחום האירוע");
//                    return result;
//                }

//                _logger.LogInformation($"Creating event with pre-calculated positions: {request.Name}");

//                // יצירת אירוע בסיסי
//                var (eventDto, zoneDto) = CreateEventAndZoneDTOsFromPreCalculated(request);
//                int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);
//                result.EventId = eventId;

//                // שמירת גרף אם קיים
//                if (_graphManager.HasCurrentGraph())
//                {
//                    await SaveGraphForEvent(eventId);
//                }

//                // שמירת אזורים אסטרטגיים
//                if (request.StrategicZones != null && request.StrategicZones.Any())
//                {
//                    foreach (var zone in request.StrategicZones)
//                        zone.EventId = eventId;
//                    _strategicZoneBL.AddStrategicZones(request.StrategicZones);
//                }

//                // שיוך שוטרים למיקומים מוכנים
//                var assignmentResult = await AssignOfficersToPreCalculatedPositions(eventDto, request.PreCalculatedPositions, eventId);

//                result.Success = true;
//                result.OfficerCount = assignmentResult.TotalAssigned;
//                result.StrategicOfficers = assignmentResult.StrategicAssigned;
//                result.RegularOfficers = assignmentResult.RegularAssigned;
//                result.Message = result.StrategicOfficers > 0
//                    ? $"אירוע נוצר בהצלחה. שובצו {result.StrategicOfficers} שוטרים באזורים אסטרטגיים ו-{result.RegularOfficers} שוטרים נוספים"
//                    : "אירוע נוצר בהצלחה ושובצו שוטרים";

//                _logger.LogInformation($"Event {eventId} created with pre-calculated positions. Total officers: {result.OfficerCount}");

//                return result;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error creating event with pre-calculated positions: {request.Name}");
//                result.Errors.Add($"שגיאה ביצירת האירוע: {ex.Message}");
//                return result;
//            }
//        }

//        public async Task<bool> DeleteEventComplete(int eventId)
//        {
//            try
//            {
//                _logger.LogInformation($"Deleting event {eventId} completely");

//                // מחיקת שיוכי שוטרים
//                var assignments = _officerAssignmentService.GetAssignmentsByEventId(eventId);
//                // TODO: הוסף מתודה למחיקת assignments ב-IOfficerAssignmentService

//                // מחיקת אזורים אסטרטגיים
//                // TODO: הוסף מתודה למחיקת strategic zones ב-IStrategicZoneBL

//                // מחיקת האירוע והאזור
//                _eventService.DeleteEvent(eventId);

//                // מחיקת הגרף
//                _graphManager.RemoveGraphForEvent(eventId);

//                _logger.LogInformation($"Event {eventId} deleted successfully");
//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error deleting event {eventId}");
//                return false;
//            }
//        }

//        public bool IsOfficerAvailableForEvent(int officerId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
//        {
//            var availableOfficers = _eventService.GetAvailableOfficersForEvent(date, startTime, endTime);
//            return availableOfficers.Any(o => o.PoliceOfficerId == officerId);
//        }

//        public List<EventWithDetailsDTO> GetAllEventsWithDetails()
//        {
//            var events = _eventService.GetEvents();
//            var result = new List<EventWithDetailsDTO>();

//            foreach (var evt in events)
//            {
//                result.Add(GetEventWithDetails(evt.EventId));
//            }

//            return result;
//        }

//        public EventWithDetailsDTO GetEventWithDetails(int eventId)
//        {
//            var eventDto = _eventService.GetEventById(eventId);
//            var zone = _eventService.GetEventZoneByEventId(eventId);
//            var strategicZones = _strategicZoneBL.GetStrategicZonesForEvent(eventId);
//            var assignments = _officerAssignmentService.GetAssignmentsByEventId(eventId);

//            // TODO: שלוף את השוטרים המשויכים עם הפרטים המלאים

//            return new EventWithDetailsDTO
//            {
//                Event = eventDto,
//                Zone = zone,
//                StrategicZones = strategicZones,
//                OfficerAssignments = assignments,
//                AssignedOfficers = new List<PoliceOfficerDTO>() // TODO: מלא את זה
//            };
//        }

//        #region Private Helper Methods

//        private (bool IsValid, List<string> Errors) ValidateEventRequest(CreateEventRequestDTO request)
//        {
//            var errors = new List<string>();

//            if (!_graphManager.HasCurrentGraph())
//                errors.Add("אין גרף טעון במערכת.");

//            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
//                errors.Add("נדרשות לפחות 4 נקודות לתחום האירוע.");

//            if (request.StrategicZones != null && request.StrategicZones.Count > request.RequiredOfficers)
//                errors.Add($"לא ניתן להציב {request.StrategicZones.Count} אזורים אסטרטגיים עם {request.RequiredOfficers} שוטרים בלבד.");

//            return (errors.Count == 0, errors);
//        }

//        private (EventDTO eventDto, EventZoneDTO zoneDto) CreateEventAndZoneDTOs(CreateEventRequestDTO request)
//        {
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

//            return (eventDto, zoneDto);
//        }

//        private (EventDTO eventDto, EventZoneDTO zoneDto) CreateEventAndZoneDTOsFromPreCalculated(CreateEventWithPositionsRequestDTO request)
//        {
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

//            return (eventDto, zoneDto);
//        }

//        private async Task SaveGraphForEvent(int eventId)
//        {
//            var currentGraph = _graphManager.GetCurrentGraph();
//            var currentNodes = _graphManager.GetCurrentNodes();
//            var currentBounds = _graphManager.GetNodesInOriginalBounds();

//            _graphManager.SaveGraphForEvent(eventId, currentGraph, currentNodes, currentBounds);
//        }

//        private async Task<List<long>> HandleStrategicZones(CreateEventRequestDTO request, int eventId)
//        {
//            var strategicNodeIds = new List<long>();

//            if (request.StrategicZones != null && request.StrategicZones.Any())
//            {
//                foreach (var zone in request.StrategicZones)
//                    zone.EventId = eventId;
//                _strategicZoneBL.AddStrategicZones(request.StrategicZones);

//                // יצירת צמתים אסטרטגיים על דרכים
//                strategicNodeIds = await CreateStrategicNodesOnRoads(request.StrategicZones);
//            }

//            return strategicNodeIds;
//        }

//        private async Task<List<long>> CreateStrategicNodesOnRoads(List<StrategicZoneDTO> strategicZones)
//        {
//            var strategicNodeIds = new List<long>();
//            var currentGraph = _graphManager.GetCurrentGraph();
//            var currentNodes = _graphManager.GetCurrentNodes();
//            var currentBounds = _graphManager.GetNodesInOriginalBounds();

//            var nodesInBounds = currentBounds
//                .Where(kvp => kvp.Value == true)
//                .Select(kvp => kvp.Key)
//                .ToHashSet();

//            _logger.LogInformation($"Creating {strategicZones.Count} strategic nodes on roads");

//            foreach (var zone in strategicZones)
//            {
//                _logger.LogDebug($"Processing strategic zone: ({zone.Latitude}, {zone.Longitude})");

//                var newStrategicNodeId = currentGraph.CreateStrategicNodeOnWay(
//                    zone.Latitude,
//                    zone.Longitude,
//                    nodesInBounds
//                );

//                if (newStrategicNodeId != -1)
//                {
//                    strategicNodeIds.Add(newStrategicNodeId);

//                    // עדכון המילונים
//                    var actualCoord = currentGraph.Nodes[newStrategicNodeId];
//                    currentNodes[newStrategicNodeId] = (actualCoord.Latitude, actualCoord.Longitude);
//                    currentBounds[newStrategicNodeId] = true;

//                    _logger.LogDebug($"✅ Created strategic node {newStrategicNodeId} on real road");
//                }
//                else
//                {
//                    _logger.LogWarning($"❌ Failed to create strategic node at ({zone.Latitude}, {zone.Longitude}) - no suitable road segment found");
//                    throw new InvalidOperationException($"לא ניתן ליצור צומת אסטרטגי במיקום ({zone.Latitude}, {zone.Longitude}) - לא נמצא קטע דרך קרוב");
//                }
//            }

//            var distinctNodeIds = strategicNodeIds.Distinct().ToList();
//            _logger.LogInformation($"Total strategic nodes created on roads: {distinctNodeIds.Count}");

//            return distinctNodeIds;
//        }

//        private async Task<KCenterDistributionResultDTO> PerformKCenterDistribution(CreateEventRequestDTO request, List<long> strategicNodeIds)
//        {
//            var currentGraph = _graphManager.GetCurrentGraph();
//            var currentBounds = _graphManager.GetNodesInOriginalBounds();

//            var nodesInBounds = currentBounds
//                .Where(kvp => kvp.Value == true)
//                .Select(kvp => kvp.Key)
//                .ToHashSet();

//            // הוספת הצמתים האסטרטגיים לרשימת הצמתים המותרים
//            var allowedNodesForDistribution = new HashSet<long>(nodesInBounds);
//            foreach (var strategicId in strategicNodeIds)
//            {
//                allowedNodesForDistribution.Add(strategicId);
//            }

//            _logger.LogInformation($"Total nodes available for distribution: {allowedNodesForDistribution.Count}");

//            try
//            {
//                // קריאה למתודה הקיימת שלך DistributePolice
//                var originalResult = _kCenterService.DistributePolice(
//                    currentGraph,
//                    request.RequiredOfficers,
//                    allowedNodesForDistribution,
//                    strategicNodeIds
//                );

//                // המרה לDTO החדש
//                var result = new KCenterDistributionResultDTO
//                {
//                    CenterNodes = originalResult.CenterNodes,
//                    MaxDistance = originalResult.MaxDistance,
//                    Success = true,
//                    ErrorMessage = ""
//                };

//                // בדיקה שכל הצמתים האסטרטגיים נכללו
//                if (!ValidateStrategicNodesIncluded(result.CenterNodes, strategicNodeIds))
//                {
//                    var missingStrategic = strategicNodeIds.Where(id => !result.CenterNodes.Contains(id)).ToList();
//                    _logger.LogError($"Strategic nodes not included: {string.Join(", ", missingStrategic)}");
//                    result.Success = false;
//                    result.ErrorMessage = $"האלגוריתם לא הצליח לכלול את כל הצמתים האסטרטגיים. חסרים: {string.Join(", ", missingStrategic)}";
//                }

//                return result;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error in K-Center distribution");
//                return new KCenterDistributionResultDTO
//                {
//                    Success = false,
//                    ErrorMessage = $"שגיאה בפיזור K-Center: {ex.Message}"
//                };
//            }
//        }

//        /// <summary>
//        /// בדיקה פנימית שכל הצמתים האסטרטגיים נכללו
//        /// </summary>
//        private bool ValidateStrategicNodesIncluded(List<long> centerNodes, List<long> strategicNodeIds)
//        {
//            if (strategicNodeIds == null || !strategicNodeIds.Any())
//                return true;

//            return strategicNodeIds.All(strategicId => centerNodes.Contains(strategicId));
//        }

//        private async Task<(int TotalAssigned, int StrategicAssigned, int RegularAssigned)> AssignOfficersToPositions(
//            EventDTO eventDto, List<long> selectedNodeIds, int eventId, List<long> strategicNodeIds)
//        {
//            _logger.LogInformation($"🔄 Starting AssignOfficersToPositions for event {eventId}");
//            _logger.LogInformation($"📍 Selected nodes: {selectedNodeIds.Count}, Strategic nodes: {strategicNodeIds.Count}");

//            var currentNodes = _graphManager.GetCurrentNodes();

//            // בדיקה שיש לנו nodes
//            if (currentNodes == null || !currentNodes.Any())
//            {
//                _logger.LogError("❌ No current nodes available from graph manager");
//                throw new InvalidOperationException("אין צמתים זמינים מהגרף");
//            }

//            _logger.LogInformation($"🗺️ Current nodes available: {currentNodes.Count}");

//            // שליפת שוטרים זמינים - בדיקה מחדש לפני השיוך!
//            _logger.LogInformation($"🔍 Searching for available officers for {eventDto.EventDate} {eventDto.StartTime}-{eventDto.EndTime}");

//            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
//                eventDto.EventDate,
//                eventDto.StartTime,
//                eventDto.EndTime
//            );

//            _logger.LogInformation($"👮 Found {availableOfficers.Count} available officers for event {eventId}");

//            if (!availableOfficers.Any())
//            {
//                _logger.LogError($"❌ No available officers found for event {eventId} on {eventDto.EventDate} {eventDto.StartTime}-{eventDto.EndTime}");

//                // בדיקה נוספת - כמה שוטרים יש בכלל במסד?
//                _logger.LogInformation("🔍 Checking total officers count for debugging...");

//                throw new InvalidOperationException($"לא נמצאו שוטרים זמינים לאירוע בתאריך {eventDto.EventDate} בשעות {eventDto.StartTime}-{eventDto.EndTime}");
//            }

//            var assignmentDtos = new List<OfficerAssignmentDTO>();
//            int strategicAssigned = 0;
//            var usedOfficerIds = new HashSet<int>(); // מעקב אחרי שוטרים שכבר שויכו

//            _logger.LogInformation($"🎯 Processing {selectedNodeIds.Count} positions for assignment");

//            foreach (var nodeId in selectedNodeIds)
//            {
//                if (!currentNodes.TryGetValue(nodeId, out var coord))
//                {
//                    _logger.LogWarning($"⚠️ Node {nodeId} not found in current nodes, skipping");
//                    continue;
//                }

//                _logger.LogDebug($"📍 Processing node {nodeId} at ({coord.lat}, {coord.lon})");
//                _logger.LogDebug($"🔍 Looking for officer among {availableOfficers.Count - usedOfficerIds.Count} remaining officers");

//                // חיפוש שוטר זמין שלא שויך עדיין
//                var candidateOfficers = availableOfficers
//                    .Where(o => !usedOfficerIds.Contains(o.PoliceOfficerId))
//                    .OrderBy(o => CalculateDistanceFromOfficer(o, coord.lat, coord.lon))
//                    .Take(5) // נבדוק את 5 הראשונים
//                    .ToList();

//                _logger.LogDebug($"🎯 Found {candidateOfficers.Count} candidate officers: {string.Join(", ", candidateOfficers.Select(o => o.PoliceOfficerId))}");

//                PoliceOfficerDTO? selectedOfficer = null;

//                foreach (var candidateOfficer in candidateOfficers)
//                {
//                    _logger.LogDebug($"🔍 Checking if officer {candidateOfficer.PoliceOfficerId} is really available");

//                    // בדיקה נוספת - ודאי שהשוטר באמת זמין
//                    if (IsOfficerReallyAvailable(candidateOfficer.PoliceOfficerId, eventDto))
//                    {
//                        selectedOfficer = candidateOfficer;
//                        _logger.LogDebug($"✅ Officer {candidateOfficer.PoliceOfficerId} is available and selected");
//                        break;
//                    }
//                    else
//                    {
//                        _logger.LogWarning($"⚠️ Officer {candidateOfficer.PoliceOfficerId} is not really available, trying next candidate");
//                    }
//                }

//                if (selectedOfficer != null)
//                {
//                    assignmentDtos.Add(new OfficerAssignmentDTO
//                    {
//                        PoliceOfficerId = selectedOfficer.PoliceOfficerId,
//                        EventId = eventId,
//                        Latitude = coord.lat,
//                        Longitude = coord.lon
//                    });

//                    // סימון השוטר כמשויך
//                    usedOfficerIds.Add(selectedOfficer.PoliceOfficerId);

//                    if (strategicNodeIds.Contains(nodeId))
//                    {
//                        strategicAssigned++;
//                        _logger.LogDebug($"🎯 Strategic officer {selectedOfficer.PoliceOfficerId} assigned to node {nodeId}");
//                    }
//                    else
//                    {
//                        _logger.LogDebug($"👮 Regular officer {selectedOfficer.PoliceOfficerId} assigned to node {nodeId}");
//                    }
//                }
//                else
//                {
//                    _logger.LogWarning($"⚠️ No available officer found for node {nodeId} after checking {candidateOfficers.Count} candidates");
//                }
//            }

//            if (!assignmentDtos.Any())
//            {
//                _logger.LogError($"❌ Failed to assign any officers. Checked {selectedNodeIds.Count} positions, had {availableOfficers.Count} available officers");
//                throw new InvalidOperationException("לא הצליח לשייך אף שוטר למיקומים - אין שוטרים זמינים");
//            }

//            // שמירת השיוכים רק אחרי שוידאנו שהכל תקין
//            _logger.LogInformation($"💾 Saving {assignmentDtos.Count} officer assignments");
//            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

//            _logger.LogInformation($"✅ Successfully assigned {assignmentDtos.Count} officers ({strategicAssigned} strategic, {assignmentDtos.Count - strategicAssigned} regular)");

//            return (assignmentDtos.Count, strategicAssigned, assignmentDtos.Count - strategicAssigned);
//        }

//        private async Task<(int TotalAssigned, int StrategicAssigned, int RegularAssigned)> AssignOfficersToPreCalculatedPositions(
//            EventDTO eventDto, List<PreCalculatedPositionDTO> positions, int eventId)
//        {
//            // שליפת שוטרים זמינים - בדיקה מחדש לפני השיוך!
//            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
//                eventDto.EventDate,
//                eventDto.StartTime,
//                eventDto.EndTime
//            );

//            _logger.LogInformation($"Found {availableOfficers.Count} available officers for pre-calculated positions");

//            if (!availableOfficers.Any())
//            {
//                _logger.LogWarning($"No available officers found for event {eventId}");
//                throw new InvalidOperationException("לא נמצאו שוטרים זמינים לאירוע");
//            }

//            var assignmentDtos = new List<OfficerAssignmentDTO>();
//            int strategicCount = 0;
//            var usedOfficerIds = new HashSet<int>(); // מעקב אחרי שוטרים שכבר שויכו

//            _logger.LogInformation($"Using pre-calculated distribution with {positions.Count} positions");

//            foreach (var position in positions)
//            {
//                // חיפוש שוטר זמין שלא שויך עדיין
//                var availableOfficer = availableOfficers
//                    .Where(o => !usedOfficerIds.Contains(o.PoliceOfficerId))
//                    .OrderBy(o => CalculateDistanceFromOfficer(o, position.Latitude, position.Longitude))
//                    .FirstOrDefault();

//                if (availableOfficer != null)
//                {
//                    // בדיקה נוספת - ודאי שהשוטר באמת זמין
//                    if (IsOfficerReallyAvailable(availableOfficer.PoliceOfficerId, eventDto))
//                    {
//                        assignmentDtos.Add(new OfficerAssignmentDTO
//                        {
//                            PoliceOfficerId = availableOfficer.PoliceOfficerId,
//                            EventId = eventId,
//                            Latitude = position.Latitude,
//                            Longitude = position.Longitude
//                        });

//                        // סימון השוטר כמשויך
//                        usedOfficerIds.Add(availableOfficer.PoliceOfficerId);

//                        if (position.IsStrategic)
//                        {
//                            strategicCount++;
//                            _logger.LogDebug($"🎯 Strategic officer {availableOfficer.PoliceOfficerId} placed at ({position.Latitude}, {position.Longitude})");
//                        }
//                        else
//                        {
//                            _logger.LogDebug($"👮 Regular officer {availableOfficer.PoliceOfficerId} placed at ({position.Latitude}, {position.Longitude})");
//                        }
//                    }
//                    else
//                    {
//                        _logger.LogWarning($"Officer {availableOfficer.PoliceOfficerId} is not really available for position ({position.Latitude}, {position.Longitude})");
//                    }
//                }
//                else
//                {
//                    _logger.LogWarning($"No available officer found for position ({position.Latitude}, {position.Longitude})");
//                }
//            }

//            if (!assignmentDtos.Any())
//            {
//                throw new InvalidOperationException("לא הצליח לשייך אף שוטר למיקומים המחושבים מראש - אין שוטרים זמינים");
//            }

//            // שמירת השיוכים רק אחרי שוידאנו שהכל תקין
//            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

//            var regularCount = assignmentDtos.Count - strategicCount;
//            _logger.LogInformation($"Event {eventId} created with {assignmentDtos.Count} officers ({strategicCount} strategic, {regularCount} regular)");

//            return (assignmentDtos.Count, strategicCount, regularCount);
//        }

//        private double CalculateDistanceFromOfficer(PoliceOfficerDTO officer, double lat, double lon)
//        {
//            // כאן תוכלי להוסיף חישוב מרחק אמיתי אם יש לך מיקום של השוטר
//            // לעת עתה מחזיר מרחק רנדומלי כדי שלא יהיה תמיד אותו שוטר

//            // במקום לחזור תמיד 0, נחזיר מרחק רנדומלי או לפי מזהה השוטר
//            var random = new Random(officer.PoliceOfficerId); // seed קבוע לפי מזהה השוטר
//            return random.NextDouble() * 100; // מרחק רנדומלי בין 0-100

//            // אם יש לך מיקום שוטר אמיתי:
//            // if (officer.CurrentLatitude.HasValue && officer.CurrentLongitude.HasValue)
//            // {
//            //     return CalculateHaversineDistance(
//            //         officer.CurrentLatitude.Value, officer.CurrentLongitude.Value,
//            //         lat, lon
//            //     );
//            // }
//        }

//        /// <summary>
//        /// בדיקה מעמיקה שהשוטר באמת זמין לאירוע - ללא רקורסיה!
//        /// </summary>
//        private bool IsOfficerReallyAvailable(int officerId, EventDTO eventDto)
//        {
//            try
//            {
//                // בדיקה ישירה של סטטוס השוטר
//                var officerStatus = _policeOfficerService.GetOfficerStatus(officerId);
//                if (officerStatus?.Status != "Available")
//                {
//                    _logger.LogDebug($"Officer {officerId} status is {officerStatus?.Status}, not Available");
//                    return false;
//                }

//                // בדיקה נוספת - וודא שהשוטר לא כבר משויך לאירוע אחר
//                var existingAssignments = _officerAssignmentService.GetAssignmentsByOfficerId(officerId);
//                foreach (var assignment in existingAssignments)
//                {
//                    var existingEvent = _eventService.GetEventById(assignment.EventId);
//                    if (existingEvent != null && existingEvent.EventDate == eventDto.EventDate)
//                    {
//                        // בדיקת חפיפת זמנים
//                        bool timeOverlap = existingEvent.StartTime < eventDto.EndTime &&
//                                         existingEvent.EndTime > eventDto.StartTime;

//                        if (timeOverlap)
//                        {
//                            _logger.LogDebug($"Officer {officerId} has time overlap with event {existingEvent.EventId}");
//                            return false;
//                        }
//                    }
//                }

//                return true;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error checking if officer {officerId} is really available");
//                return false;
//            }
//        }

//        private EventCreationDebugInfoDTO CreateDebugInfo(CreateEventRequestDTO request, List<long> strategicNodeIds, KCenterDistributionResultDTO distributionResult)
//        {
//            var currentGraph = _graphManager.GetCurrentGraph();
//            var currentBounds = _graphManager.GetNodesInOriginalBounds();

//            var nodesInBounds = currentBounds
//                .Where(kvp => kvp.Value == true)
//                .Select(kvp => kvp.Key)
//                .ToHashSet();

//            return new EventCreationDebugInfoDTO
//            {
//                OriginalStrategicZones = request.StrategicZones?.Count ?? 0,
//                FoundStrategicNodes = strategicNodeIds.Count,
//                TotalNodesInBounds = nodesInBounds.Count,
//                TotalWaySegments = currentGraph.WaySegments?.Count ?? 0,
//                SelectedNodes = distributionResult.CenterNodes.Count,
//                StrategicNodeIds = strategicNodeIds,
//                MissingStrategicNodes = strategicNodeIds.Where(id => !distributionResult.CenterNodes.Contains(id)).ToList()
//            };
//        }

//        #endregion
//    }
//}
using DTO;
using IBL;
using Microsoft.Extensions.Logging;
using static DTO.EventRequestsDTO;

namespace BLL
{
    public class EventManagementService : IEventManagementService
    {
        private readonly IEventService _eventService;
        private readonly IKCenterService _kCenterService;
        private readonly IOfficerAssignmentService _officerAssignmentService;
        private readonly IStrategicZoneBL _strategicZoneBL;
        private readonly IGraphManagerService _graphManager;
        private readonly IPoliceOfficerService _policeOfficerService;
        private readonly ILogger<EventManagementService> _logger;

        public EventManagementService(
            IEventService eventService,
            IKCenterService kCenterService,
            IOfficerAssignmentService officerAssignmentService,
            IStrategicZoneBL strategicZoneBL,
            IGraphManagerService graphManager,
            IPoliceOfficerService policeOfficerService,
            ILogger<EventManagementService> logger)
        {
            _eventService = eventService;
            _kCenterService = kCenterService;
            _officerAssignmentService = officerAssignmentService;
            _strategicZoneBL = strategicZoneBL;
            _graphManager = graphManager;
            _policeOfficerService = policeOfficerService;
            _logger = logger;
        }

        public async Task<EventCreationResultDTO> CreateEventWithAutoDistribution(CreateEventRequestDTO request)
        {
            var result = new EventCreationResultDTO();

            try
            {
                // בדיקות תקינות ראשוניות
                var validationResult = ValidateEventRequest(request);
                if (!validationResult.IsValid)
                {
                    result.Errors = validationResult.Errors;
                    return result;
                }

                _logger.LogInformation($"Starting event creation: {request.Name}");

                // יצירת DTO לאירוע ואזור
                var (eventDto, zoneDto) = CreateEventAndZoneDTOs(request);

                // שמירה במסד נתונים
                int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);
                result.EventId = eventId;

                // שמירת הגרף
                await SaveGraphForEvent(eventId);

                // טיפול באזורים אסטרטגיים
                var strategicNodeIds = await HandleStrategicZones(request, eventId);

                // ביצוע פיזור K-Center
                var distributionResult = await PerformKCenterDistribution(request, strategicNodeIds);
                if (!distributionResult.Success)
                {
                    result.Errors.Add(distributionResult.ErrorMessage);
                    return result;
                }

                // שיוך שוטרים למיקומים
                var assignmentResult = await AssignOfficersToPositions(
                    eventDto, distributionResult.CenterNodes, eventId, strategicNodeIds);

                // הכנת התוצאה
                result.Success = true;
                result.OfficerCount = assignmentResult.TotalAssigned;
                result.StrategicOfficers = strategicNodeIds.Count;
                result.RegularOfficers = assignmentResult.TotalAssigned - strategicNodeIds.Count;
                result.NodesCreatedOnRealRoads = strategicNodeIds.Count;
                result.Message = strategicNodeIds.Count > 0
                    ? $"אירוע נוצר בהצלחה. נוצרו {strategicNodeIds.Count} צמתים אסטרטגיים על דרכים אמיתיות ושובצו {result.StrategicOfficers} שוטרים באזורים אסטרטגיים ו-{result.RegularOfficers} שוטרים נוספים"
                    : "אירוע נוצר בהצלחה ושובצו שוטרים";

                result.DebugInfo = CreateDebugInfo(request, strategicNodeIds, distributionResult);

                _logger.LogInformation($"Event {eventId} created successfully with {result.OfficerCount} officers");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating event: {request.Name}");
                result.Errors.Add($"שגיאה ביצירת האירוע: {ex.Message}");
                return result;
            }
        }

        public async Task<EventCreationResultDTO> CreateEventWithPreCalculatedPositions(CreateEventWithPositionsRequestDTO request)
        {
            var result = new EventCreationResultDTO();

            try
            {
                // בדיקות תקינות
                if (request.PreCalculatedPositions == null || !request.PreCalculatedPositions.Any())
                {
                    result.Errors.Add("לא נמצא פיזור מוכן של שוטרים");
                    return result;
                }

                if (request.SelectedArea == null || request.SelectedArea.Count < 4)
                {
                    result.Errors.Add("נדרשות לפחות 4 נקודות לתחום האירוע");
                    return result;
                }

                _logger.LogInformation($"Creating event with pre-calculated positions: {request.Name}");

                // יצירת אירוע בסיסי
                var (eventDto, zoneDto) = CreateEventAndZoneDTOsFromPreCalculated(request);
                int eventId = _eventService.CreateEventWithZone(eventDto, zoneDto);
                result.EventId = eventId;

                // שמירת גרף אם קיים
                if (_graphManager.HasCurrentGraph())
                {
                    await SaveGraphForEvent(eventId);
                }

                // שמירת אזורים אסטרטגיים
                if (request.StrategicZones != null && request.StrategicZones.Any())
                {
                    foreach (var zone in request.StrategicZones)
                        zone.EventId = eventId;
                    _strategicZoneBL.AddStrategicZones(request.StrategicZones);
                }

                // שיוך שוטרים למיקומים מוכנים
                var assignmentResult = await AssignOfficersToPreCalculatedPositions(eventDto, request.PreCalculatedPositions, eventId);

                result.Success = true;
                result.OfficerCount = assignmentResult.TotalAssigned;
                result.StrategicOfficers = assignmentResult.StrategicAssigned;
                result.RegularOfficers = assignmentResult.RegularAssigned;
                result.Message = result.StrategicOfficers > 0
                    ? $"אירוע נוצר בהצלחה. שובצו {result.StrategicOfficers} שוטרים באזורים אסטרטגיים ו-{result.RegularOfficers} שוטרים נוספים"
                    : "אירוע נוצר בהצלחה ושובצו שוטרים";

                _logger.LogInformation($"Event {eventId} created with pre-calculated positions. Total officers: {result.OfficerCount}");

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating event with pre-calculated positions: {request.Name}");
                result.Errors.Add($"שגיאה ביצירת האירוע: {ex.Message}");
                return result;
            }
        }

        public async Task<bool> DeleteEventComplete(int eventId)
        {
            try
            {
                _logger.LogInformation($"Deleting event {eventId} completely");

                // מחיקת שיוכי שוטרים
                var assignments = _officerAssignmentService.GetAssignmentsByEventId(eventId);
                // TODO: הוסף מתודה למחיקת assignments ב-IOfficerAssignmentService

                // מחיקת אזורים אסטרטגיים
                // TODO: הוסף מתודה למחיקת strategic zones ב-IStrategicZoneBL

                // מחיקת האירוע והאזור
                _eventService.DeleteEvent(eventId);

                // מחיקת הגרף
                _graphManager.RemoveGraphForEvent(eventId);

                _logger.LogInformation($"Event {eventId} deleted successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting event {eventId}");
                return false;
            }
        }

        public bool IsOfficerAvailableForEvent(int officerId, DateOnly date, TimeOnly startTime, TimeOnly endTime)
        {
            var availableOfficers = _eventService.GetAvailableOfficersForEvent(date, startTime, endTime);
            return availableOfficers.Any(o => o.PoliceOfficerId == officerId);
        }

        public List<EventWithDetailsDTO> GetAllEventsWithDetails()
        {
            var events = _eventService.GetEvents();
            var result = new List<EventWithDetailsDTO>();

            foreach (var evt in events)
            {
                result.Add(GetEventWithDetails(evt.EventId));
            }

            return result;
        }

        public EventWithDetailsDTO GetEventWithDetails(int eventId)
        {
            var eventDto = _eventService.GetEventById(eventId);
            var zone = _eventService.GetEventZoneByEventId(eventId);
            var strategicZones = _strategicZoneBL.GetStrategicZonesForEvent(eventId);
            var assignments = _officerAssignmentService.GetAssignmentsByEventId(eventId);

            // TODO: שלוף את השוטרים המשויכים עם הפרטים המלאים

            return new EventWithDetailsDTO
            {
                Event = eventDto,
                Zone = zone,
                StrategicZones = strategicZones,
                OfficerAssignments = assignments,
                AssignedOfficers = new List<PoliceOfficerDTO>() // TODO: מלא את זה
            };
        }

        #region Private Helper Methods

        private (bool IsValid, List<string> Errors) ValidateEventRequest(CreateEventRequestDTO request)
        {
            var errors = new List<string>();

            if (!_graphManager.HasCurrentGraph())
                errors.Add("אין גרף טעון במערכת.");

            if (request.SelectedArea == null || request.SelectedArea.Count < 4)
                errors.Add("נדרשות לפחות 4 נקודות לתחום האירוע.");

            if (request.StrategicZones != null && request.StrategicZones.Count > request.RequiredOfficers)
                errors.Add($"לא ניתן להציב {request.StrategicZones.Count} אזורים אסטרטגיים עם {request.RequiredOfficers} שוטרים בלבד.");

            return (errors.Count == 0, errors);
        }

        private (EventDTO eventDto, EventZoneDTO zoneDto) CreateEventAndZoneDTOs(CreateEventRequestDTO request)
        {
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

            return (eventDto, zoneDto);
        }

        private (EventDTO eventDto, EventZoneDTO zoneDto) CreateEventAndZoneDTOsFromPreCalculated(CreateEventWithPositionsRequestDTO request)
        {
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

            return (eventDto, zoneDto);
        }

        private async Task SaveGraphForEvent(int eventId)
        {
            var currentGraph = _graphManager.GetCurrentGraph();
            var currentNodes = _graphManager.GetCurrentNodes();
            var currentBounds = _graphManager.GetNodesInOriginalBounds();

            _graphManager.SaveGraphForEvent(eventId, currentGraph, currentNodes, currentBounds);
        }

        private async Task<List<long>> HandleStrategicZones(CreateEventRequestDTO request, int eventId)
        {
            var strategicNodeIds = new List<long>();

            if (request.StrategicZones != null && request.StrategicZones.Any())
            {
                foreach (var zone in request.StrategicZones)
                    zone.EventId = eventId;
                _strategicZoneBL.AddStrategicZones(request.StrategicZones);

                // יצירת צמתים אסטרטגיים על דרכים
                strategicNodeIds = await CreateStrategicNodesOnRoads(request.StrategicZones);
            }

            return strategicNodeIds;
        }

        private async Task<List<long>> CreateStrategicNodesOnRoads(List<StrategicZoneDTO> strategicZones)
        {
            var strategicNodeIds = new List<long>();
            var currentGraph = _graphManager.GetCurrentGraph();
            var currentNodes = _graphManager.GetCurrentNodes();
            var currentBounds = _graphManager.GetNodesInOriginalBounds();

            var nodesInBounds = currentBounds
                .Where(kvp => kvp.Value == true)
                .Select(kvp => kvp.Key)
                .ToHashSet();

            _logger.LogInformation($"Creating {strategicZones.Count} strategic nodes on roads");

            foreach (var zone in strategicZones)
            {
                _logger.LogDebug($"Processing strategic zone: ({zone.Latitude}, {zone.Longitude})");

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

                    _logger.LogDebug($"✅ Created strategic node {newStrategicNodeId} on real road");
                }
                else
                {
                    _logger.LogWarning($"❌ Failed to create strategic node at ({zone.Latitude}, {zone.Longitude}) - no suitable road segment found");
                    throw new InvalidOperationException($"לא ניתן ליצור צומת אסטרטגי במיקום ({zone.Latitude}, {zone.Longitude}) - לא נמצא קטע דרך קרוב");
                }
            }

            var distinctNodeIds = strategicNodeIds.Distinct().ToList();
            _logger.LogInformation($"Total strategic nodes created on roads: {distinctNodeIds.Count}");

            return distinctNodeIds;
        }

        private async Task<KCenterDistributionResultDTO> PerformKCenterDistribution(CreateEventRequestDTO request, List<long> strategicNodeIds)
        {
            var currentGraph = _graphManager.GetCurrentGraph();
            var currentBounds = _graphManager.GetNodesInOriginalBounds();

            var nodesInBounds = currentBounds
                .Where(kvp => kvp.Value == true)
                .Select(kvp => kvp.Key)
                .ToHashSet();

            // הוספת הצמתים האסטרטגיים לרשימת הצמתים המותרים
            var allowedNodesForDistribution = new HashSet<long>(nodesInBounds);
            foreach (var strategicId in strategicNodeIds)
            {
                allowedNodesForDistribution.Add(strategicId);
            }

            _logger.LogInformation($"Total nodes available for distribution: {allowedNodesForDistribution.Count}");

            try
            {
                // קריאה למתודה הקיימת שלך DistributePolice
                var originalResult = _kCenterService.DistributePolice(
                    currentGraph,
                    request.RequiredOfficers,
                    allowedNodesForDistribution,
                    strategicNodeIds
                );

                // המרה לDTO החדש
                var result = new KCenterDistributionResultDTO
                {
                    CenterNodes = originalResult.CenterNodes,
                    MaxDistance = originalResult.MaxDistance,
                    Success = true,
                    ErrorMessage = ""
                };

                // בדיקה שכל הצמתים האסטרטגיים נכללו
                if (!ValidateStrategicNodesIncluded(result.CenterNodes, strategicNodeIds))
                {
                    var missingStrategic = strategicNodeIds.Where(id => !result.CenterNodes.Contains(id)).ToList();
                    _logger.LogError($"Strategic nodes not included: {string.Join(", ", missingStrategic)}");
                    result.Success = false;
                    result.ErrorMessage = $"האלגוריתם לא הצליח לכלול את כל הצמתים האסטרטגיים. חסרים: {string.Join(", ", missingStrategic)}";
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in K-Center distribution");
                return new KCenterDistributionResultDTO
                {
                    Success = false,
                    ErrorMessage = $"שגיאה בפיזור K-Center: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// בדיקה פנימית שכל הצמתים האסטרטגיים נכללו
        /// </summary>
        private bool ValidateStrategicNodesIncluded(List<long> centerNodes, List<long> strategicNodeIds)
        {
            if (strategicNodeIds == null || !strategicNodeIds.Any())
                return true;

            return strategicNodeIds.All(strategicId => centerNodes.Contains(strategicId));
        }

        private async Task<(int TotalAssigned, int StrategicAssigned, int RegularAssigned)> AssignOfficersToPositions(
            EventDTO eventDto, List<long> selectedNodeIds, int eventId, List<long> strategicNodeIds)
        {
            var currentNodes = _graphManager.GetCurrentNodes();
            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
                eventDto.EventDate,
                eventDto.StartTime,
                eventDto.EndTime
            );

            var assignmentDtos = new List<OfficerAssignmentDTO>();
            int strategicAssigned = 0;

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

                    if (strategicNodeIds.Contains(nodeId))
                    {
                        strategicAssigned++;
                    }
                }
            }

            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

            _logger.LogInformation($"Assigned {assignmentDtos.Count} officers ({strategicAssigned} strategic, {assignmentDtos.Count - strategicAssigned} regular)");

            return (assignmentDtos.Count, strategicAssigned, assignmentDtos.Count - strategicAssigned);
        }

        private async Task<(int TotalAssigned, int StrategicAssigned, int RegularAssigned)> AssignOfficersToPreCalculatedPositions(
            EventDTO eventDto, List<PreCalculatedPositionDTO> positions, int eventId)
        {
            var availableOfficers = _eventService.GetAvailableOfficersForEvent(
                eventDto.EventDate,
                eventDto.StartTime,
                eventDto.EndTime
            );

            var assignmentDtos = new List<OfficerAssignmentDTO>();
            int strategicCount = 0;

            _logger.LogInformation($"Using pre-calculated distribution with {positions.Count} positions");

            foreach (var position in positions)
            {
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
                        _logger.LogDebug($"🎯 Strategic officer placed at ({position.Latitude}, {position.Longitude})");
                    }
                    else
                    {
                        _logger.LogDebug($"👮 Regular officer placed at ({position.Latitude}, {position.Longitude})");
                    }
                }
            }

            _officerAssignmentService.AddOfficerAssignments(assignmentDtos);

            var regularCount = assignmentDtos.Count - strategicCount;
            _logger.LogInformation($"Event {eventId} created with {assignmentDtos.Count} officers ({strategicCount} strategic, {regularCount} regular)");

            return (assignmentDtos.Count, strategicCount, regularCount);
        }

        private double CalculateDistanceFromOfficer(PoliceOfficerDTO officer, double lat, double lon)
        {
            // כאן תוכלי להוסיף חישוב מרחק אמיתי אם יש לך מיקום של השוטר
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

        private EventCreationDebugInfoDTO CreateDebugInfo(CreateEventRequestDTO request, List<long> strategicNodeIds, KCenterDistributionResultDTO distributionResult)
        {
            var currentGraph = _graphManager.GetCurrentGraph();
            var currentBounds = _graphManager.GetNodesInOriginalBounds();

            var nodesInBounds = currentBounds
                .Where(kvp => kvp.Value == true)
                .Select(kvp => kvp.Key)
                .ToHashSet();

            return new EventCreationDebugInfoDTO
            {
                OriginalStrategicZones = request.StrategicZones?.Count ?? 0,
                FoundStrategicNodes = strategicNodeIds.Count,
                TotalNodesInBounds = nodesInBounds.Count,
                TotalWaySegments = currentGraph.WaySegments?.Count ?? 0,
                SelectedNodes = distributionResult.CenterNodes.Count,
                StrategicNodeIds = strategicNodeIds,
                MissingStrategicNodes = strategicNodeIds.Where(id => !distributionResult.CenterNodes.Contains(id)).ToList()
            };
        }

        #endregion
    }
}