using System;
using System.Collections.Generic;

namespace Utilities
{
    public static class GraphUtils
    {
        public static double Haversine(double lat1, double lon1, double lat2, double lon2)
        {
            double R = 6371e3; // רדיוס כדור הארץ במטרים
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

        public static Dictionary<long, List<long>> BuildSimpleGraph(List<(long from, long to)> edges)
        {
            var graph = new Dictionary<long, List<long>>();

            foreach (var (from, to) in edges)
            {
                if (!graph.ContainsKey(from))
                    graph[from] = new List<long>();
                if (!graph.ContainsKey(to))
                    graph[to] = new List<long>();

                graph[from].Add(to);
                graph[to].Add(from); // דו כיווני
            }

            return graph;
        }
    }
}
