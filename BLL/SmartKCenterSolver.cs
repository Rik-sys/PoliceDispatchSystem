//using DTO;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BLL
//{
//    public class SmartKCenterSolver
//    {
//        private readonly Graph _graph;
//        private readonly Dictionary<(long, long), double> _shortestPaths;
//        private readonly List<double> _allDistances;

//        public SmartKCenterSolver(Graph graph)
//        {
//            _graph = graph;
//            _shortestPaths = ComputeAllPairsShortestPaths(graph);
//            _allDistances = _shortestPaths.Values.Distinct().OrderBy(x => x).ToList();
//        }

//        /// <summary>
//        /// פתרון חכם עם אזורים אסטרטגיים - שילוב האילוצים בחיפוש הבינארי
//        /// </summary>
//        public (List<long> centers, double radius) SolveWithStrategicZones(int k, List<long> strategicNodes = null)
//        {
//            if (k <= 0)
//                throw new ArgumentException("חייב להיות גדול מ0 k");

//            strategicNodes = strategicNodes ?? new List<long>();

//            if (strategicNodes.Count > k)
//                throw new ArgumentException($"לא ניתן לכסות {strategicNodes.Count} אזורים אסטרטגיים עם {k} שוטרים בלבד");

//            if (k >= _graph.Nodes.Count)
//                return (_graph.Nodes.Keys.ToList(), 0);

//            // חיפוש בינארי על הרדיוס עם שילוב האילוצים
//            int low = 0;
//            int high = _allDistances.Count - 1;
//            List<long> bestCenters = null;
//            double bestRadius = double.MaxValue;

//            while (low <= high)
//            {
//                int mid = (low + high) / 2;
//                double radius = _allDistances[mid];

//                // בדיקה אם ניתן לפתור עם הרדיוס הזה ועם האילוצים
//                var centers = CanSolveWithRadius(radius, k, strategicNodes);

//                if (centers != null)
//                {
//                    bestCenters = centers;
//                    bestRadius = radius;
//                    high = mid - 1;
//                }
//                else
//                {
//                    low = mid + 1;
//                }
//            }

//            return (bestCenters ?? strategicNodes.Take(k).ToList(), bestRadius);
//        }

//        /// <summary>
//        /// בדיקה חכמה אם ניתן לפתור עם רדיוס נתון ואילוצים אסטרטגיים
//        /// </summary>
//        private List<long> CanSolveWithRadius(double radius, int k, List<long> strategicNodes)
//        {
//            // יצירת גרף דומיננטיות לרדיוס הנתון
//            var dominationGraph = BuildDominationGraph(radius);

//            // פתרון בעיית Set Cover המשוקללת עם אילוצים
//            return SolveConstrainedSetCover(dominationGraph, k, strategicNodes);
//        }

//        /// <summary>
//        /// בניית גרף דומיננטיות - כל צומת "שולט" על הצמתים במרחק הרדיוס
//        /// </summary>
//        private Dictionary<long, HashSet<long>> BuildDominationGraph(double radius)
//        {
//            var dominationGraph = new Dictionary<long, HashSet<long>>();
//            var allNodes = _graph.Nodes.Keys.ToList();

//            foreach (var node in allNodes)
//            {
//                dominationGraph[node] = new HashSet<long>();

//                foreach (var other in allNodes)
//                {
//                    if (GetDistance(node, other) <= radius)
//                    {
//                        dominationGraph[node].Add(other);
//                    }
//                }
//            }

//            return dominationGraph;
//        }

//        /// <summary>
//        /// פתרון Set Cover עם אילוצים אסטרטגיים - גישה מתוחכמת
//        /// </summary>
//        private List<long> SolveConstrainedSetCover(Dictionary<long, HashSet<long>> dominationGraph, int k, List<long> strategicNodes)
//        {
//            var allNodes = _graph.Nodes.Keys.ToHashSet();
//            var uncoveredNodes = new HashSet<long>(allNodes);
//            var selectedCenters = new List<long>();

//            // שלב 1: הוספת האזורים האסטרטגיים וכיסוי ראשוני
//            foreach (var strategic in strategicNodes)
//            {
//                selectedCenters.Add(strategic);
//                if (dominationGraph.ContainsKey(strategic))
//                {
//                    uncoveredNodes.ExceptWith(dominationGraph[strategic]);
//                }
//            }

//            // שלב 2: השלמה אופטימלית עם "גרידי חכם" 
//            while (uncoveredNodes.Count > 0 && selectedCenters.Count < k)
//            {
//                var bestCandidate = FindBestCandidateSmarter(dominationGraph, uncoveredNodes, selectedCenters, k);

//                if (bestCandidate == -1)
//                    return null; // לא ניתן לפתור

//                selectedCenters.Add(bestCandidate);
//                if (dominationGraph.ContainsKey(bestCandidate))
//                {
//                    uncoveredNodes.ExceptWith(dominationGraph[bestCandidate]);
//                }
//            }

//            return uncoveredNodes.Count == 0 ? selectedCenters : null;
//        }

//        /// <summary>
//        /// מציאת מועמד חכם עם שקלול מתקדם
//        /// </summary>
//        private long FindBestCandidateSmarter(Dictionary<long, HashSet<long>> dominationGraph,
//            HashSet<long> uncoveredNodes, List<long> currentCenters, int k)
//        {
//            long bestCandidate = -1;
//            double bestScore = -1;

//            foreach (var candidate in dominationGraph.Keys)
//            {
//                if (currentCenters.Contains(candidate))
//                    continue;

//                // חישוב ציון מתוחכם המשלב כמה פרמטרים
//                double score = CalculateCandidateScore(candidate, dominationGraph, uncoveredNodes, currentCenters, k);

//                if (score > bestScore)
//                {
//                    bestScore = score;
//                    bestCandidate = candidate;
//                }
//            }

//            return bestScore > 0 ? bestCandidate : -1;
//        }

//        /// <summary>
//        /// חישוב ציון מתוחכם למועמד - לא רק כמות אלא גם איכות הכיסוי
//        /// </summary>
//        private double CalculateCandidateScore(long candidate, Dictionary<long, HashSet<long>> dominationGraph,
//            HashSet<long> uncoveredNodes, List<long> currentCenters, int k)
//        {
//            if (!dominationGraph.ContainsKey(candidate))
//                return 0;

//            var candidateCoverage = dominationGraph[candidate];
//            var newlyCovered = candidateCoverage.Where(node => uncoveredNodes.Contains(node)).ToList();

//            if (newlyCovered.Count == 0)
//                return 0;

//            // משקל בסיסי לפי כמות הכיסוי
//            double coverageWeight = newlyCovered.Count;

//            // בונוס לכיסוי "בודד" - צמתים שרק מועמד זה יכול לכסות
//            int remainingCenters = k - currentCenters.Count - 1;
//            double uniquenessBonus = 0;

//            if (remainingCenters > 0)
//            {
//                foreach (var node in newlyCovered)
//                {
//                    int alternativeCovers = dominationGraph.Keys
//                        .Where(other => other != candidate && !currentCenters.Contains(other))
//                        .Count(other => dominationGraph[other].Contains(node));

//                    if (alternativeCovers <= remainingCenters)
//                    {
//                        uniquenessBonus += 1.0 / (alternativeCovers + 1);
//                    }
//                }
//            }

//            // ציון סופי
//            return coverageWeight + uniquenessBonus * 0.5;
//        }

//        // שמירה על הפונקציות המקוריות
//        private Dictionary<(long, long), double> ComputeAllPairsShortestPaths(Graph graph)
//        {
//            var distances = new Dictionary<(long, long), double>();

//            foreach (var source in graph.Nodes.Values)
//            {
//                distances[(source.Id, source.Id)] = 0;
//                var priorityQueue = new SortedDictionary<double, HashSet<long>>();
//                var visited = new HashSet<long>();
//                var currentDistances = new Dictionary<long, double>();

//                foreach (var node in graph.Nodes.Values)
//                {
//                    if (node.Id == source.Id)
//                    {
//                        currentDistances[node.Id] = 0;
//                        if (!priorityQueue.ContainsKey(0))
//                            priorityQueue[0] = new HashSet<long>();
//                        priorityQueue[0].Add(node.Id);
//                    }
//                    else
//                    {
//                        currentDistances[node.Id] = double.PositiveInfinity;
//                    }
//                }

//                while (priorityQueue.Count > 0)
//                {
//                    var minDistance = priorityQueue.Keys.First();
//                    var nodes = priorityQueue[minDistance];
//                    var currentId = nodes.First();

//                    nodes.Remove(currentId);
//                    if (nodes.Count == 0)
//                        priorityQueue.Remove(minDistance);

//                    if (visited.Contains(currentId))
//                        continue;

//                    visited.Add(currentId);
//                    var current = graph.Nodes[currentId];

//                    foreach (var edge in current.Edges)
//                    {
//                        var neighborId = edge.To.Id;
//                        if (visited.Contains(neighborId))
//                            continue;

//                        var newDistance = currentDistances[currentId] + edge.Weight;

//                        if (newDistance < currentDistances[neighborId])
//                        {
//                            if (currentDistances[neighborId] != double.PositiveInfinity)
//                            {
//                                var oldDistance = currentDistances[neighborId];
//                                if (priorityQueue.ContainsKey(oldDistance) && priorityQueue[oldDistance].Contains(neighborId))
//                                {
//                                    priorityQueue[oldDistance].Remove(neighborId);
//                                    if (priorityQueue[oldDistance].Count == 0)
//                                        priorityQueue.Remove(oldDistance);
//                                }
//                            }

//                            currentDistances[neighborId] = newDistance;

//                            if (!priorityQueue.ContainsKey(newDistance))
//                                priorityQueue[newDistance] = new HashSet<long>();
//                            priorityQueue[newDistance].Add(neighborId);

//                            distances[(source.Id, neighborId)] = newDistance;
//                        }
//                    }
//                }
//            }

//            return distances;
//        }

//        private double GetDistance(long node1, long node2)
//        {
//            if (_shortestPaths.TryGetValue((node1, node2), out double distance))
//                return distance;
//            else if (_shortestPaths.TryGetValue((node2, node1), out distance))
//                return distance;
//            else
//                return double.MaxValue;
//        }
//    }
//}
using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class SmartKCenterSolver
    {
        private readonly Graph _graph;
        private readonly Dictionary<(long, long), double> _shortestPaths;
        private readonly List<double> _allDistances;

        public SmartKCenterSolver(Graph graph)
        {
            _graph = graph;
            _shortestPaths = ComputeAllPairsShortestPaths(graph);
            _allDistances = _shortestPaths.Values.Distinct().OrderBy(x => x).ToList();
        }

        /// <summary>
        /// פתרון חכם עם אזורים אסטרטגיים - מבטיח שהם יכללו בפתרון
        /// </summary>
        public (List<long> centers, double radius) SolveWithStrategicZones(int k, List<long> strategicNodes = null)
        {
            if (k <= 0)
                throw new ArgumentException("חייב להיות גדול מ0 k");

            strategicNodes = strategicNodes ?? new List<long>();

            // בדיקה קריטית: האם יש יותר אזורים אסטרטגיים מאשר שוטרים
            if (strategicNodes.Count > k)
                throw new ArgumentException($"לא ניתן לכסות {strategicNodes.Count} אזורים אסטרטגיים עם {k} שוטרים בלבד");

            // אם אין אזורים אסטרטגיים - פתרון רגיל
            if (!strategicNodes.Any())
            {
                var regularSolver = new KCenterSolver(_graph);
                return regularSolver.Solve(k);
            }

            // אם מספר השוטרים שווה למספר האזורים האסטרטגיים
            if (strategicNodes.Count == k)
            {
                var maxDistance = CalculateMaxDistanceForCenters(strategicNodes);
                return (strategicNodes.ToList(), maxDistance);
            }

            // פתרון מתקדם: חיפוש בינארי עם אילוץ אזורים אסטרטגיים
            if (k >= _graph.Nodes.Count)
                return (_graph.Nodes.Keys.ToList(), 0);

            int low = 0;
            int high = _allDistances.Count - 1;
            List<long> bestCenters = null;
            double bestRadius = double.MaxValue;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                double radius = _allDistances[mid];

                // בדיקה אם ניתן לפתור עם הרדיוס הזה ועם חובת הכללת האזורים האסטרטגיים
                var centers = CanSolveWithRadiusAndConstraints(radius, k, strategicNodes);

                if (centers != null && centers.Count <= k && ContainsAllStrategicNodes(centers, strategicNodes))
                {
                    bestCenters = centers;
                    bestRadius = radius;
                    high = mid - 1;
                }
                else
                {
                    low = mid + 1;
                }
            }

            // אם לא מצאנו פתרון טוב, ננסה פתרון "חירום"
            if (bestCenters == null || !ContainsAllStrategicNodes(bestCenters, strategicNodes))
            {
                bestCenters = FallbackSolution(k, strategicNodes);
                bestRadius = CalculateMaxDistanceForCenters(bestCenters);
            }

            return (bestCenters, bestRadius);
        }

        /// <summary>
        /// בדיקה אם ניתן לפתור עם רדיוס נתון ואילוץ הכללת האזורים האסטרטגיים
        /// </summary>
        private List<long> CanSolveWithRadiusAndConstraints(double radius, int k, List<long> strategicNodes)
        {
            // יצירת גרף דומיננטיות לרדיוס הנתון
            var dominationGraph = BuildDominationGraph(radius);

            // פתרון בעיית Set Cover עם אילוץ הכללת האזורים האסטרטגיים
            return SolveConstrainedSetCoverMandatory(dominationGraph, k, strategicNodes);
        }

        /// <summary>
        /// פתרון Set Cover עם חובת הכללת האזורים האסטרטגיים
        /// </summary>
        private List<long> SolveConstrainedSetCoverMandatory(Dictionary<long, HashSet<long>> dominationGraph, int k, List<long> strategicNodes)
        {
            var allNodes = _graph.Nodes.Keys.ToHashSet();
            var uncoveredNodes = new HashSet<long>(allNodes);
            var selectedCenters = new List<long>();

            // שלב 1: הוספה חובה של כל האזורים האסטרטגיים
            foreach (var strategic in strategicNodes)
            {
                selectedCenters.Add(strategic);
                if (dominationGraph.ContainsKey(strategic))
                {
                    uncoveredNodes.ExceptWith(dominationGraph[strategic]);
                }
            }

            // בדיקה שלא חרגנו ממספר השוטרים המותר
            if (selectedCenters.Count > k)
            {
                return null; // לא ניתן לפתור
            }

            // שלב 2: השלמה אופטימלית עם השוטרים הנותרים
            int remainingSlots = k - selectedCenters.Count;

            while (uncoveredNodes.Count > 0 && remainingSlots > 0)
            {
                var bestCandidate = FindBestCandidateForRemainingNodes(dominationGraph, uncoveredNodes, selectedCenters);

                if (bestCandidate == -1)
                    break; // לא נמצא מועמד טוב

                selectedCenters.Add(bestCandidate);
                if (dominationGraph.ContainsKey(bestCandidate))
                {
                    uncoveredNodes.ExceptWith(dominationGraph[bestCandidate]);
                }
                remainingSlots--;
            }

            // החזרת פתרון רק אם כל הצמתים כוסו
            return uncoveredNodes.Count == 0 ? selectedCenters : null;
        }

        /// <summary>
        /// מציאת מועמד טוב ביותר לכיסוי הצמתים הנותרים
        /// </summary>
        private long FindBestCandidateForRemainingNodes(Dictionary<long, HashSet<long>> dominationGraph,
            HashSet<long> uncoveredNodes, List<long> currentCenters)
        {
            long bestCandidate = -1;
            int maxNewCoverage = 0;

            foreach (var candidate in dominationGraph.Keys)
            {
                if (currentCenters.Contains(candidate))
                    continue;

                var candidateCoverage = dominationGraph[candidate];
                var newlyCovered = candidateCoverage.Where(node => uncoveredNodes.Contains(node)).Count();

                if (newlyCovered > maxNewCoverage)
                {
                    maxNewCoverage = newlyCovered;
                    bestCandidate = candidate;
                }
            }

            return maxNewCoverage > 0 ? bestCandidate : -1;
        }

        /// <summary>
        /// פתרון חירום - מבטיח הכללת האזורים האסטרטגיים עם גרידי פשוט
        /// </summary>
        private List<long> FallbackSolution(int k, List<long> strategicNodes)
        {
            var allNodes = _graph.Nodes.Keys.ToHashSet();
            var uncoveredNodes = new HashSet<long>(allNodes);
            var selectedCenters = new List<long>();

            // הוספה חובה של האזורים האסטרטגיים
            foreach (var strategic in strategicNodes)
            {
                selectedCenters.Add(strategic);
                // הסרת צמתים קרובים (בהנחה שהם יכוסו)
                var nearbyNodes = allNodes.Where(node => GetDistance(strategic, node) <= GetReasonableRadius()).ToList();
                foreach (var nearby in nearbyNodes)
                {
                    uncoveredNodes.Remove(nearby);
                }
            }

            // השלמה גרידית עם השוטרים הנותרים
            while (uncoveredNodes.Count > 0 && selectedCenters.Count < k)
            {
                var bestCandidate = FindGreedyCandidate(uncoveredNodes, selectedCenters);
                if (bestCandidate == -1)
                    break;

                selectedCenters.Add(bestCandidate);
                var nearbyNodes = allNodes.Where(node => GetDistance(bestCandidate, node) <= GetReasonableRadius()).ToList();
                foreach (var nearby in nearbyNodes)
                {
                    uncoveredNodes.Remove(nearby);
                }
            }

            return selectedCenters;
        }

        /// <summary>
        /// מציאת מועמד גרידי פשוט
        /// </summary>
        private long FindGreedyCandidate(HashSet<long> uncoveredNodes, List<long> currentCenters)
        {
            if (!uncoveredNodes.Any())
                return -1;

            // פשוט בחירת הצומת הראשון שלא נכלל
            return uncoveredNodes.First();
        }

        /// <summary>
        /// חישוב רדיוס סביר לפתרון חירום
        /// </summary>
        private double GetReasonableRadius()
        {
            if (_allDistances.Count > 0)
            {
                var medianIndex = _allDistances.Count / 2;
                return _allDistances[medianIndex];
            }
            return 1000; // ערך ברירת מחדל
        }

        /// <summary>
        /// בדיקה שכל האזורים האסטרטגיים כלולים ברשימת המרכזים
        /// </summary>
        private bool ContainsAllStrategicNodes(List<long> centers, List<long> strategicNodes)
        {
            return strategicNodes.All(strategic => centers.Contains(strategic));
        }

        /// <summary>
        /// חישוב המרחק המקסימלי עבור רשימת מרכזים נתונה
        /// </summary>
        private double CalculateMaxDistanceForCenters(List<long> centers)
        {
            if (!centers.Any())
                return 0;

            double maxDistance = 0;
            var allNodes = _graph.Nodes.Keys;

            foreach (var node in allNodes)
            {
                var minDistanceToCenter = centers.Min(center => GetDistance(node, center));
                maxDistance = Math.Max(maxDistance, minDistanceToCenter);
            }

            return maxDistance;
        }

        /// <summary>
        /// בניית גרף דומיננטיות - כל צומת "שולט" על הצמתים במרחק הרדיוס
        /// </summary>
        private Dictionary<long, HashSet<long>> BuildDominationGraph(double radius)
        {
            var dominationGraph = new Dictionary<long, HashSet<long>>();
            var allNodes = _graph.Nodes.Keys.ToList();

            foreach (var node in allNodes)
            {
                dominationGraph[node] = new HashSet<long>();

                foreach (var other in allNodes)
                {
                    if (GetDistance(node, other) <= radius)
                    {
                        dominationGraph[node].Add(other);
                    }
                }
            }

            return dominationGraph;
        }

        // שמירה על הפונקציות המקוריות
        private Dictionary<(long, long), double> ComputeAllPairsShortestPaths(Graph graph)
        {
            var distances = new Dictionary<(long, long), double>();

            foreach (var source in graph.Nodes.Values)
            {
                distances[(source.Id, source.Id)] = 0;
                var priorityQueue = new SortedDictionary<double, HashSet<long>>();
                var visited = new HashSet<long>();
                var currentDistances = new Dictionary<long, double>();

                foreach (var node in graph.Nodes.Values)
                {
                    if (node.Id == source.Id)
                    {
                        currentDistances[node.Id] = 0;
                        if (!priorityQueue.ContainsKey(0))
                            priorityQueue[0] = new HashSet<long>();
                        priorityQueue[0].Add(node.Id);
                    }
                    else
                    {
                        currentDistances[node.Id] = double.PositiveInfinity;
                    }
                }

                while (priorityQueue.Count > 0)
                {
                    var minDistance = priorityQueue.Keys.First();
                    var nodes = priorityQueue[minDistance];
                    var currentId = nodes.First();

                    nodes.Remove(currentId);
                    if (nodes.Count == 0)
                        priorityQueue.Remove(minDistance);

                    if (visited.Contains(currentId))
                        continue;

                    visited.Add(currentId);
                    var current = graph.Nodes[currentId];

                    foreach (var edge in current.Edges)
                    {
                        var neighborId = edge.To.Id;
                        if (visited.Contains(neighborId))
                            continue;

                        var newDistance = currentDistances[currentId] + edge.Weight;

                        if (newDistance < currentDistances[neighborId])
                        {
                            if (currentDistances[neighborId] != double.PositiveInfinity)
                            {
                                var oldDistance = currentDistances[neighborId];
                                if (priorityQueue.ContainsKey(oldDistance) && priorityQueue[oldDistance].Contains(neighborId))
                                {
                                    priorityQueue[oldDistance].Remove(neighborId);
                                    if (priorityQueue[oldDistance].Count == 0)
                                        priorityQueue.Remove(oldDistance);
                                }
                            }

                            currentDistances[neighborId] = newDistance;

                            if (!priorityQueue.ContainsKey(newDistance))
                                priorityQueue[newDistance] = new HashSet<long>();
                            priorityQueue[newDistance].Add(neighborId);

                            distances[(source.Id, neighborId)] = newDistance;
                        }
                    }
                }
            }

            return distances;
        }

        private double GetDistance(long node1, long node2)
        {
            if (_shortestPaths.TryGetValue((node1, node2), out double distance))
                return distance;
            else if (_shortestPaths.TryGetValue((node2, node1), out distance))
                return distance;
            else
                return double.MaxValue;
        }
    }
}