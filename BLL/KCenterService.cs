using DTO;
using IBL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLL
{
    public class KCenterService : IKCenterService
    {
        public (List<long> centers, double maxResponseTime) SolveKCenter(Graph graph, int k)
        {
            var solver = new KCenterSolver(graph);
            var (centers, radius) = solver.Solve(k);

            // המרה של מרחק (מטרים) לזמן תגובה (שניות)
            const double averageSpeed = 10.0; // מטר לשנייה
            double maxResponseTime = radius / averageSpeed;

            return (centers, maxResponseTime);
        }

    }
}
