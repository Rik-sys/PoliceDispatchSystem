using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
