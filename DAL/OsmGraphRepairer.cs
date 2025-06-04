
namespace DAL
{
    public static class OsmGraphRepairer
    {
        public static List<(long from, long to)> FindConnectingPath(
            HashSet<long> componentA,
            HashSet<long> componentB,
            Dictionary<long, (double lat, double lon)> fullNodes,
            List<(long from, long to)> fullEdges,
            double maxSearchDistance)
        {
            // סינון הצמתים שקיימים במפה המורחבת
            var componentAFiltered = componentA.Where(node => fullNodes.ContainsKey(node)).ToHashSet();
            var componentBFiltered = componentB.Where(node => fullNodes.ContainsKey(node)).ToHashSet();

            if (componentAFiltered.Count == 0 || componentBFiltered.Count == 0)
                return new List<(long from, long to)>();

            // בניית הגרף המורחב
            var fullGraph = BuildGraph(fullEdges);

            // חיפוש זוג הצמתים הקרובים ביותר
            var minPair = FindClosestPair(componentAFiltered, componentBFiltered, fullNodes);

            if (minPair.dist > maxSearchDistance || minPair.a == -1 || minPair.b == -1)
                return new List<(long from, long to)>();

            // חיפוש מסלול בגרף המורחב
            return Dijkstra(fullGraph, fullNodes, minPair.a, minPair.b);
        }
        private static (long a, long b, double dist) FindClosestPair(
            HashSet<long> componentA,
            HashSet<long> componentB,
            Dictionary<long, (double lat, double lon)> fullNodes)
        {
            var minPair = (a: -1L, b: -1L, dist: double.MaxValue);

            var sampleSizeA = Math.Min(componentA.Count, 100);
            var sampleSizeB = Math.Min(componentB.Count, 100);

            var sampledA = componentA.Count <= sampleSizeA
                ? componentA
                : new HashSet<long>(componentA.OrderBy(_ => Guid.NewGuid()).Take(sampleSizeA));

            var sampledB = componentB.Count <= sampleSizeB
                ? componentB
                : new HashSet<long>(componentB.OrderBy(_ => Guid.NewGuid()).Take(sampleSizeB));

            foreach (var a in sampledA)
            {
                var coordA = fullNodes[a];
                foreach (var b in sampledB)
                {
                    var coordB = fullNodes[b];
                    double dist = Haversine(coordA.lat, coordA.lon, coordB.lat, coordB.lon);
                    if (dist < minPair.dist)
                    {
                        minPair = (a, b, dist);
                    }
                }
            }
            return minPair;
        }

        private static List<(long from, long to)> Dijkstra(
            Dictionary<long, List<long>> graph,
            Dictionary<long, (double lat, double lon)> nodes,
            long start,
            long end)
        {
            var dist = new Dictionary<long, double>();
            var prev = new Dictionary<long, long>();
            var queue = new PriorityQueue<long, double>();

            foreach (var node in graph.Keys)
                dist[node] = double.MaxValue;

            dist[start] = 0;
            queue.Enqueue(start, 0);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                // אם הגענו ליעד, נפסיק את החיפוש
                if (current == end) break;

                // אם אין שכנים, נמשיך
                if (!graph.ContainsKey(current)) continue;

                foreach (var neighbor in graph[current])
                {
                    // חישוב משקל הקשת באמצעות מרחק
                    double weight = Haversine(
                        nodes[current].lat, nodes[current].lon,
                        nodes[neighbor].lat, nodes[neighbor].lon);

                    double alt = dist[current] + weight;
                    if (!dist.ContainsKey(neighbor) || alt < dist[neighbor])
                    {
                        dist[neighbor] = alt;
                        prev[neighbor] = current;
                        queue.Enqueue(neighbor, alt);
                    }
                }
            }

            // בדיקה אם מצאנו מסלול
            if (!prev.ContainsKey(end)) return new List<(long, long)>();

            // שחזור המסלול
            var path = new List<(long, long)>();
            for (long at = end; at != start; at = prev[at])
            {
                path.Insert(0, (prev[at], at));
            }
            return path;
        }

        private static Dictionary<long, List<long>> BuildGraph(List<(long from, long to)> edges)
        {
            var graph = new Dictionary<long, List<long>>();
            foreach (var (from, to) in edges)
            {
                if (!graph.ContainsKey(from)) graph[from] = new List<long>();
                if (!graph.ContainsKey(to)) graph[to] = new List<long>();
                graph[from].Add(to);
                graph[to].Add(from);
            }
            return graph;
        }

        private static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371e3;
            double phi1 = lat1 * Math.PI / 180;
            double phi2 = lat2 * Math.PI / 180;
            double dPhi = (lat2 - lat1) * Math.PI / 180;
            double dLambda = (lon2 - lon1) * Math.PI / 180;
            double a = Math.Sin(dPhi / 2) * Math.Sin(dPhi / 2) +
                       Math.Cos(phi1) * Math.Cos(phi2) *
                       Math.Sin(dLambda / 2) * Math.Sin(dLambda / 2);
            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return R * c;
        }
    }
}