//using BLL;
//using DTO;
//using IBL;

//public class KCenterService : IKCenterService
//{
//    public (List<long> CenterNodes, double MaxDistance) DistributePolice(
//        Graph graph,
//        int k,
//        HashSet<long> allowedNodes,
//        List<long> strategicNodes = null)
//    {
//        // סינון הגרף לפי הצמתים המותרים (התחום)
//        var filteredGraph = graph.FilterNodes(allowedNodes);

//        List<long> centerNodes;
//        double radius;

//        if (strategicNodes != null && strategicNodes.Any())
//        {
//            // אם יש אזורים אסטרטגיים – השתמש בפתרון החכם
//            var solver = new SmartKCenterSolver(filteredGraph);
//            (centerNodes, radius) = solver.SolveWithStrategicZones(k, strategicNodes);
//        }
//        else
//        {
//            // אחרת – פיזור רגיל
//            var solver = new KCenterSolver(filteredGraph);
//            (centerNodes, radius) = solver.Solve(k);
//        }

//        return (centerNodes, radius);
//    }
//}

// BLL/KCenterService.cs
using BLL;
using DTO;
using IBL;
using Microsoft.Extensions.Logging;

namespace BLL
{
    public class KCenterService : IKCenterService
    {
        private readonly ILogger<KCenterService> _logger;

        public KCenterService(ILogger<KCenterService> logger)
        {
            _logger = logger;
        }

        public (List<long> CenterNodes, double MaxDistance) DistributePolice(
            Graph graph,
            int k,
            HashSet<long> allowedNodes,
            List<long> strategicNodes = null)
        {
            if (k <= 0)
                throw new ArgumentException("מספר השוטרים חייב להיות גדול מאפס");

            try
            {
                // סינון הגרף לפי הצמתים המותרים (התחום)
                var filteredGraph = graph.FilterNodes(allowedNodes);

                List<long> centerNodes;
                double radius;

                if (strategicNodes != null && strategicNodes.Any())
                {
                    // אם יש אזורים אסטרטגיים – השתמש בפתרון החכם
                    var solver = new SmartKCenterSolver(filteredGraph);
                    (centerNodes, radius) = solver.SolveWithStrategicZones(k, strategicNodes);

                    _logger.LogInformation($"פיזור עם אזורים אסטרטגיים הושלם: {centerNodes.Count} מרכזים, רדיוס: {radius:F2}");
                }
                else
                {
                    // אחרת – פיזור רגיל
                    var solver = new KCenterSolver(filteredGraph);
                    (centerNodes, radius) = solver.Solve(k);

                    _logger.LogInformation($"פיזור רגיל הושלם: {centerNodes.Count} מרכזים, רדיוס: {radius:F2}");
                }

                return (centerNodes, radius);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"שגיאה בפיזור שוטרים: k={k}, allowedNodes={allowedNodes?.Count}, strategicNodes={strategicNodes?.Count}");
                throw;
            }
        }

        public KCenterResultDTO DistributePoliceWithStrategic(
            Graph graph,
            Dictionary<long, (double lat, double lon)> nodes,
            Dictionary<long, bool> bounds,
            DistributeWithStrategicRequest request)
        {
            if (request.K <= 0)
                throw new ArgumentException("מספר השוטרים חייב להיות גדול מאפס");

            var originalNodes = bounds.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToHashSet();

            _logger.LogInformation($"🔍 מספר צמתים בתחום: {originalNodes.Count}");
            _logger.LogInformation($"🛣️  מספר קטעי דרך זמינים: {graph.WaySegments.Count}");

            // יצירת צמתים אסטרטגיים על Ways אמיתיים
            List<long> strategicNodeIds = new List<long>();

            if (request.StrategicZones != null && request.StrategicZones.Any())
            {
                _logger.LogInformation($"🎯 יוצר {request.StrategicZones.Count} צמתים אסטרטגיים על דרכים:");

                foreach (var zone in request.StrategicZones)
                {
                    _logger.LogDebug($"\n📍 מעבד אזור: ({zone.Latitude}, {zone.Longitude})");

                    // שימוש בפונקציה שמפצלת Ways
                    var newStrategicNodeId = graph.CreateStrategicNodeOnWay(
                        zone.Latitude,
                        zone.Longitude,
                        originalNodes
                    );

                    if (newStrategicNodeId != -1)
                    {
                        strategicNodeIds.Add(newStrategicNodeId);

                        // עדכון המילונים הגלובליים
                        var actualCoord = graph.Nodes[newStrategicNodeId];
                        nodes[newStrategicNodeId] = (actualCoord.Latitude, actualCoord.Longitude);
                        bounds[newStrategicNodeId] = true;

                        _logger.LogDebug($"✅ נוצר צומת אסטרטגי {newStrategicNodeId} על דרך אמיתית");
                    }
                    else
                    {
                        _logger.LogWarning($"❌ כשל ביצירת צומת אסטרטגי - לא נמצא קטע דרך מתאים");
                    }
                }

                strategicNodeIds = strategicNodeIds.Distinct().ToList();
                _logger.LogInformation($"\n🎯 סה\"כ צמתים אסטרטגיים נוצרו: {strategicNodeIds.Count}");
            }

            // עדכון רשימת הצמתים המותרים
            var allowedNodesForDistribution = new HashSet<long>(originalNodes);
            foreach (var strategicId in strategicNodeIds)
            {
                allowedNodesForDistribution.Add(strategicId);
            }

            _logger.LogInformation($"📊 סה\"כ צמתים זמינים לפיזור: {allowedNodesForDistribution.Count}");

            // פיזור עם צמתים אסטרטגיים
            var result = DistributePolice(graph, request.K, allowedNodesForDistribution, strategicNodeIds);

            _logger.LogInformation($"\n📍 האלגוריתם בחר {result.CenterNodes.Count} צמתים:");
            foreach (var nodeId in result.CenterNodes)
            {
                if (nodes.TryGetValue(nodeId, out var coord))
                {
                    var isStrategic = strategicNodeIds.Contains(nodeId) ? "🎯 אסטרטגי" : "👮 רגיל";
                    var nodeType = graph.IsStrategicNode(nodeId) ? " (על דרך)" : " (OSM מקורי)";
                    _logger.LogDebug($"   {isStrategic}: צומת {nodeId} במיקום ({coord.lat:F6}, {coord.lon:F6}){nodeType}");
                }
            }

            // בדיקה שכל הצמתים האסטרטגיים נכללו
            var missingStrategic = strategicNodeIds.Where(id => !result.CenterNodes.Contains(id)).ToList();
            if (missingStrategic.Any())
            {
                _logger.LogError($"❌ צמתים אסטרטגיים שלא נכללו: {string.Join(", ", missingStrategic)}");
                throw new InvalidOperationException($"האלגוריתם לא הצליח לכלול את כל הצמתים האסטרטגיים. חסרים: {string.Join(", ", missingStrategic)}");
            }

            double maxDistanceInKilometers = result.MaxDistance / 1000.0;
            var strategicCount = strategicNodeIds.Count;
            var regularCount = result.CenterNodes.Count - strategicCount;

            return new KCenterResultDTO
            {
                PolicePositions = result.CenterNodes.Select(nodeId => new OfficerAssignmentDTO
                {
                    PoliceOfficerId = 0, // יהיה מוקצה מאוחר יותר בתהליך השיוך
                    EventId = 0, // יהיה מוקצה מאוחר יותר
                    Latitude = graph.Nodes[nodeId].Latitude,
                    Longitude = graph.Nodes[nodeId].Longitude
                }).ToList(),
                MaxDistance = result.MaxDistance,
                MaxDistanceInKilometers = maxDistanceInKilometers,
                StrategicOfficers = strategicCount,
                RegularOfficers = regularCount,
                NodesCreatedOnRoads = strategicNodeIds.Count,
                StrategicNodeIds = strategicNodeIds,
                Message = strategicCount > 0
                    ? $"פוזרו {request.K} שוטרים - {strategicCount} בצמתים אסטרטגיים על דרכים אמיתיות ו-{regularCount} נוספים. מרחק מקסימלי: {maxDistanceInKilometers:F2} ק\"מ."
                    : $"פוזרו {request.K} שוטרים בהצלחה. מרחק מקסימלי: {maxDistanceInKilometers:F2} ק\"מ."
            };
        }

        public KCenterResultDTO DistributePoliceStandard(
            Graph graph,
            Dictionary<long, (double lat, double lon)> nodes,
            Dictionary<long, bool> inBounds,
            int k,
            int? eventId = null)
        {
            var allowed = inBounds.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToHashSet();
            var result = DistributePolice(graph, k, allowed);

            double maxDistanceInKilometers = result.MaxDistance / 1000.0;

            return new KCenterResultDTO
            {
                EventId = eventId,
                PolicePositions = result.CenterNodes.Select(id => new OfficerAssignmentDTO
                {
                    PoliceOfficerId = 0, // יהיה מוקצה מאוחר יותר
                    EventId = eventId ?? 0,
                    Latitude = graph.Nodes[id].Latitude,
                    Longitude = graph.Nodes[id].Longitude
                }).ToList(),
                MaxDistance = result.MaxDistance,
                MaxDistanceInKilometers = maxDistanceInKilometers,
                StrategicOfficers = 0,
                RegularOfficers = result.CenterNodes.Count,
                NodesCreatedOnRoads = 0,
                StrategicNodeIds = new List<long>(),
                Message = $"פוזרו {k} שוטרים בהצלחה. מרחק מקסימלי: {maxDistanceInKilometers:F2} ק\"מ."
            };
        }
    }
}