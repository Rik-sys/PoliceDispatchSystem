
//ניסוי פיזור בתחום
using DTO;

namespace BLL
{
    public class KCenterSolver
    {
        private readonly Graph _graph;
        private readonly Dictionary<(long, long), double> _shortestPaths;
        private readonly List<double> _allDistances;

        public KCenterSolver(Graph graph)
        {
            _graph = graph;
            _shortestPaths = ComputeAllPairsShortestPaths(graph);
            _allDistances = _shortestPaths.Values.Distinct().OrderBy(x => x).ToList();
        }

        
        public (List<long> centers, double radius) Solve(int k)
        {
            if (k <= 0)
            {
                throw new ArgumentException("חייב להיות גדול מ0 k");
            }
            if (k >= _graph.Nodes.Count)
            {
                return (_graph.Nodes.Keys.ToList(), 0);
            }
            int low = 0;
            int high = _allDistances.Count - 1;
            List<long> bestCenters = null;
            double bestRadius = double.MaxValue;

            while (low <= high)
            {
                int mid = (low + high) / 2;
                double radius = _allDistances[mid];
                var centers = FindCentersWithRadius(radius);

                //אם הרדיוס הסתדר אני מנסה להקטין אותו כמה שיותר
                if (centers.Count <= k)
                {
                    bestCenters = centers;
                    bestRadius = radius;
                    high = mid - 1;
                }
                //אם לא הסתדר נצטרך להגדיל את הרדיוס
                else
                {
                    low = mid + 1;
                }
            }
            return (bestCenters, bestRadius);
        }
        private List<long> FindCentersWithRadius(double radius)
        {
            //לשמור את כל הצמתים
            var nodeIds = _graph.Nodes.Keys.ToList();
            //המרכזים שייבחרו בסוף
            var centers = new List<long>();
            //הצמתים שעדיין לא כוסו
            var remainingNodes = new HashSet<long>(nodeIds);

            // טיפול מיוחד במקרה של k=1 (כשמצפים למרכז אחד)
            if (radius >= _allDistances.Last())
            {
                // אם הרדיוס גדול מהמרחק המקסימלי, צומת אחד יכול לכסות הכל
                centers.Add(nodeIds.First());
                return centers;
            }
            while (remainingNodes.Count > 0)
            {
                //לקחתי את הצומת הראשון כמרכז
                long x = remainingNodes.First();
                centers.Add(x);

                //הצמתים שיכוסו ע''י המרכז שבחרתי ואפשר להסיר אותם
                var toRemove = new HashSet<long>();

                foreach (var nodeId in remainingNodes)
                {
                    //אם המרחק בין הצומת לבין המרכז שנבחר כרגע קטן מהרדיוס אפשר להוסיף אותו לרשימת הצמתים שיוסרו
                    if (GetDistance(x, nodeId) <= radius)
                    {
                        toRemove.Add(nodeId);
                    }
                    else
                    {
                        //בדיקה אם אולי אפשר לכסות אותו דרך צומת שלישי
                        bool dominated = nodeIds.Any(z =>
                        GetDistance(x, z) <= radius && GetDistance(z, nodeId) <= radius);
                        if (dominated)
                        {
                            toRemove.Add(nodeId);
                        }
                    }
                }
                //הסרה של כל הצמתים שכוסו מרשימת הצמתים שעדיין לא כוסו
                remainingNodes.ExceptWith(toRemove); 
            }
            return centers;
        }

        //הכנה של מטריצת מרחקים ע''י אלגוריתם דייקסטרה
        private Dictionary<(long, long), double> ComputeAllPairsShortestPaths(Graph graph)
        {
            var distances = new Dictionary<(long, long), double>();

            //על כל צומת אני מריצה את דייקסטרה מחדש
            foreach (var source in graph.Nodes.Values)
            {
                //מרחק של צומת מעצמו הוא 0
                distances[(source.Id, source.Id)] = 0;

                // לשמירה של המרחק הקצר הידוע
                var priorityQueue = new SortedDictionary<double, HashSet<long>>();
                //לשמירה של כל הצמתים שכבר ביקרתי בהם
                var visited = new HashSet<long>();
                //לשמירה של המרחק הכי קצר שהיה עד עכשיו
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

                    // עדכון של כל השכנים שלו
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

                            // עדכון המרחק
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
            {
                return distance;
            }
            else if (_shortestPaths.TryGetValue((node2, node1), out distance))
            {
                return distance;
            }
            else//למרות שבעיקרון הגרף אמור להיות קשיר,אבל ליתר ביטחון
            {
                return double.MaxValue;
            }
        }
    }
}
