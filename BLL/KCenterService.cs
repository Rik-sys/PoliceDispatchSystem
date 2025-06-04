//using DTO;
//using IBL;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace BLL
//{
//    public class KCenterService : IKCenterService
//    {
//        public (List<long> CenterNodes, double MaxDistance) DistributePolice(Graph graph, int k, HashSet<long> allowedNodes)
//        {
//            // Filter the graph to include only allowed nodes
//            var filteredGraph = graph.FilterNodes(allowedNodes);

//            // Solve the k-center problem on the filtered graph
//            var solver = new KCenterSolver(filteredGraph);
//            var (centerNodes, radius) = solver.Solve(k);

//            // Return the center nodes and the maximum distance (radius)
//            return (centerNodes, radius);
//        }

//        public (List<long> centers, double maxResponseTime) SolveKCenter(Graph graph, int k)
//        {
//            var solver = new KCenterSolver(graph);
//            var (centers, radius) = solver.Solve(k);

//            // המרה של מרחק (מטרים) לזמן תגובה (שניות)
//            const double averageSpeed = 10.0; // מטר לשנייה
//            double maxResponseTime = radius / averageSpeed;

//            return (centers, maxResponseTime);
//        }

//    }
//}
using BLL;
using DTO;
using IBL;

public class KCenterService : IKCenterService
{
    public (List<long> CenterNodes, double MaxDistance) DistributePolice(
        Graph graph,
        int k,
        HashSet<long> allowedNodes,
        List<long> strategicNodes = null)
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
        }
        else
        {
            // אחרת – פיזור רגיל
            var solver = new KCenterSolver(filteredGraph);
            (centerNodes, radius) = solver.Solve(k);
        }

        return (centerNodes, radius);
    }

    public (List<long> centers, double maxResponseTime) SolveKCenter(
        Graph graph,
        int k,
        List<long> strategicNodes = null)
    {
        List<long> centers;
        double radius;

        if (strategicNodes != null && strategicNodes.Any())
        {
            var solver = new SmartKCenterSolver(graph);
            (centers, radius) = solver.SolveWithStrategicZones(k, strategicNodes);
        }
        else
        {
            var solver = new KCenterSolver(graph);
            (centers, radius) = solver.Solve(k);
        }

        // המרה של מרחק לזמן תגובה
        const double averageSpeed = 10.0;
        double maxResponseTime = radius / averageSpeed;

        return (centers, maxResponseTime);
    }
}
