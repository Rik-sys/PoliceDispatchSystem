//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace DTO
//{
//    public class Graph
//    {
//        public Dictionary<long, Node> Nodes { get; set; } = new();

//        public void AddEdge(long from, long to, double weight = 1)
//        {
//            if (!Nodes.ContainsKey(from) || !Nodes.ContainsKey(to))
//                return;

//            Nodes[from].Edges.Add(new Edge { To = Nodes[to], Weight = weight });
//            Nodes[to].Edges.Add(new Edge { To = Nodes[from], Weight = weight }); // דו-כיווני
//        }


//        public bool IsConnected()
//        {
//            if (!Nodes.Any()) return false;

//            var visited = new HashSet<long>();
//            var queue = new Queue<Node>();
//            var first = Nodes.Values.First();
//            queue.Enqueue(first);
//            visited.Add(first.Id);

//            while (queue.Any())
//            {
//                var current = queue.Dequeue();
//                foreach (var edge in current.Edges)
//                {
//                    if (!visited.Contains(edge.To.Id))
//                    {
//                        visited.Add(edge.To.Id);
//                        queue.Enqueue(edge.To);
//                    }
//                }
//            }

//            return visited.Count == Nodes.Count;
//        }
//    }
//}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//ניסוי פיזור בתחום
namespace DTO
{
    public class Graph
    {
        public Dictionary<long, Node> Nodes { get; set; } = new();

        public void AddEdge(long from, long to, double weight = 1)
        {
            if (!Nodes.ContainsKey(from) || !Nodes.ContainsKey(to))
                return;

            Nodes[from].Edges.Add(new Edge { To = Nodes[to], Weight = weight });
            Nodes[to].Edges.Add(new Edge { To = Nodes[from], Weight = weight }); // דו-כיווני
        }

        public bool IsConnected()
        {
            if (!Nodes.Any()) return false;

            var visited = new HashSet<long>();
            var queue = new Queue<Node>();
            var first = Nodes.Values.First();
            queue.Enqueue(first);
            visited.Add(first.Id);

            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var edge in current.Edges)
                {
                    if (!visited.Contains(edge.To.Id))
                    {
                        visited.Add(edge.To.Id);
                        queue.Enqueue(edge.To);
                    }
                }
            }

            return visited.Count == Nodes.Count;
        }

        public List<HashSet<long>> GetConnectedComponents()
        {
            var visited = new HashSet<long>();
            var components = new List<HashSet<long>>();

            foreach (var node in Nodes.Values)
            {
                if (!visited.Contains(node.Id))
                {
                    var component = new HashSet<long>();
                    var stack = new Stack<Node>();
                    stack.Push(node);

                    while (stack.Any())
                    {
                        var current = stack.Pop();
                        if (!visited.Add(current.Id)) continue;
                        component.Add(current.Id);

                        foreach (var edge in current.Edges)
                        {
                            if (!visited.Contains(edge.To.Id))
                                stack.Push(edge.To);
                        }
                    }

                    components.Add(component);
                }
            }

            return components;
        }

        public List<(long from, long to)> GetAllEdges()
        {
            var edges = new HashSet<(long, long)>();

            foreach (var node in Nodes.Values)
            {
                foreach (var edge in node.Edges)
                {
                    var a = node.Id;
                    var b = edge.To.Id;
                    if (a < b) edges.Add((a, b));
                    else edges.Add((b, a));
                }
            }

            return edges.ToList();
        }

        public void AddNode(long nodeId, double lat, double lon)
        {
            if (!Nodes.ContainsKey(nodeId))
            {
                Nodes[nodeId] = new Node
                {
                    Id = nodeId,
                    Latitude = lat,
                    Longitude = lon,
                    Edges = new List<Edge>()
                };
            }
        }

    }
}