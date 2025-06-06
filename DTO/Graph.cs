
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

        // 🆕 שמירת מידע על Ways לצורך פיצול
        public List<WaySegment> WaySegments { get; set; } = new();

        // מונה לצמתים חדשים
        private static long _nextVirtualNodeId = 100_000_000_000L;

        public void AddEdge(long from, long to, double weight = 1)
        {
            if (!Nodes.ContainsKey(from) || !Nodes.ContainsKey(to))
                return;
            Nodes[from].Edges.Add(new Edge { To = Nodes[to], Weight = weight });
            Nodes[to].Edges.Add(new Edge { To = Nodes[from], Weight = weight });
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

        /// <summary>
        /// מוסיף מידע על קטע דרך לגרף
        /// </summary>
        public void AddWaySegment(long wayId, long fromNode, long toNode,
            (double lat, double lon) fromCoord, (double lat, double lon) toCoord,
            string highwayType)
        {
            WaySegments.Add(new WaySegment
            {
                WayId = wayId,
                FromNodeId = fromNode,
                ToNodeId = toNode,
                FromCoord = fromCoord,
                ToCoord = toCoord,
                HighwayType = highwayType
            });
        }

        /// <summary>
        /// יוצר צומת אסטרטגי על ה-Way הקרוב ביותר
        /// </summary>
        public long CreateStrategicNodeOnWay(double latitude, double longitude, HashSet<long> allowedNodes)
        {
            Console.WriteLine($"🔍 מחפש Way קרוב למיקום ({latitude}, {longitude})");

            // מציאת הקטע הקרוב ביותר
            var closestSegment = FindClosestWaySegment(latitude, longitude, allowedNodes);

            if (closestSegment == null)
            {
                Console.WriteLine("❌ לא נמצא קטע דרך מתאים");
                return -1;
            }

            Console.WriteLine($"📍 נמצא קטע דרך {closestSegment.WayId} מצומת {closestSegment.FromNodeId} לצומת {closestSegment.ToNodeId}");

            // חישוב נקודת ההטלה על הקטע
            var projectionPoint = ProjectPointOntoSegment(
                latitude, longitude,
                closestSegment.FromCoord.lat, closestSegment.FromCoord.lon,
                closestSegment.ToCoord.lat, closestSegment.ToCoord.lon
            );

            Console.WriteLine($"📐 נקודת הטלה: ({projectionPoint.lat:F6}, {projectionPoint.lon:F6})");

            // יצירת צומת חדש
            long newNodeId = _nextVirtualNodeId++;
            AddNode(newNodeId, projectionPoint.lat, projectionPoint.lon);

            // **פיצול הקטע המקורי**
            return SplitWaySegment(closestSegment, newNodeId, projectionPoint);
        }

        /// <summary>
        /// מוצא את קטע הדרך הקרוב ביותר לנקודה נתונה
        /// </summary>
        private WaySegment FindClosestWaySegment(double lat, double lon, HashSet<long> allowedNodes)
        {
            WaySegment closestSegment = null;
            double minDistance = double.MaxValue;

            foreach (var segment in WaySegments)
            {
                // בדיקה שהקטע חוצה את האזור המותר
                if (!allowedNodes.Contains(segment.FromNodeId) || !allowedNodes.Contains(segment.ToNodeId))
                    continue;

                // חישוב מרחק מהנקודה לקטע
                double distance = DistanceFromPointToLineSegment(
                    lat, lon,
                    segment.FromCoord.lat, segment.FromCoord.lon,
                    segment.ToCoord.lat, segment.ToCoord.lon
                );

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestSegment = segment;
                }
            }

            if (closestSegment != null)
            {
                double distanceInMeters = minDistance * 111_000; // המרה גסה למטרים
                Console.WriteLine($"🎯 נמצא קטע קרוב ביותר: מרחק {distanceInMeters:F0} מטר");

                // בדיקת סבירות המרחק
                if (distanceInMeters > 500) // יותר מ-500 מטר
                {
                    Console.WriteLine($"⚠️  אזהרה: המרחק גדול ({distanceInMeters:F0}m), הצומת עלול להיות רחוק מהדרך");
                }
            }

            return closestSegment;
        }

        /// <summary>
        /// מפצל קטע דרך ומוסיף צומת חדש באמצע
        /// </summary>
        private long SplitWaySegment(WaySegment originalSegment, long newNodeId, (double lat, double lon) splitPoint)
        {
            Console.WriteLine($"✂️  מפצל קטע מ-{originalSegment.FromNodeId} ל-{originalSegment.ToNodeId}");

            // 1. מחיקת הקשת המקורית
            RemoveEdgeBetweenNodes(originalSegment.FromNodeId, originalSegment.ToNodeId);

            // 2. חישוב משקלים לקשתות החדשות
            double distanceToFrom = CalculateDistanceInMeters(
                splitPoint.lat, splitPoint.lon,
                originalSegment.FromCoord.lat, originalSegment.FromCoord.lon
            );

            double distanceToEnd = CalculateDistanceInMeters(
                splitPoint.lat, splitPoint.lon,
                originalSegment.ToCoord.lat, originalSegment.ToCoord.lon
            );

            // 3. הוספת קשתות חדשות
            AddEdge(originalSegment.FromNodeId, newNodeId, distanceToFrom);
            AddEdge(newNodeId, originalSegment.ToNodeId, distanceToEnd);

            Console.WriteLine($"🔗 נוצרו קשתות:");
            Console.WriteLine($"   {originalSegment.FromNodeId} → {newNodeId} ({distanceToFrom:F0}m)");
            Console.WriteLine($"   {newNodeId} → {originalSegment.ToNodeId} ({distanceToEnd:F0}m)");

            // 4. עדכון רשימת הקטעים
            WaySegments.Remove(originalSegment);

            // הוספת שני קטעים חדשים
            WaySegments.Add(new WaySegment
            {
                WayId = originalSegment.WayId,
                FromNodeId = originalSegment.FromNodeId,
                ToNodeId = newNodeId,
                FromCoord = originalSegment.FromCoord,
                ToCoord = splitPoint,
                HighwayType = originalSegment.HighwayType
            });

            WaySegments.Add(new WaySegment
            {
                WayId = originalSegment.WayId,
                FromNodeId = newNodeId,
                ToNodeId = originalSegment.ToNodeId,
                FromCoord = splitPoint,
                ToCoord = originalSegment.ToCoord,
                HighwayType = originalSegment.HighwayType
            });

            Console.WriteLine($"✅ פיצול הושלם בהצלחה - צומת {newNodeId} נוסף ל-Way {originalSegment.WayId}");

            return newNodeId;
        }

        /// <summary>
        /// מחיקת קשת בין שני צמתים
        /// </summary>
        private void RemoveEdgeBetweenNodes(long nodeId1, long nodeId2)
        {
            if (Nodes.TryGetValue(nodeId1, out var node1))
            {
                node1.Edges.RemoveAll(e => e.To.Id == nodeId2);
            }

            if (Nodes.TryGetValue(nodeId2, out var node2))
            {
                node2.Edges.RemoveAll(e => e.To.Id == nodeId1);
            }
        }

        /// <summary>
        /// מטיל נקודה על קטע ישר ומחזיר את הנקודה הקרובה ביותר על הקטע
        /// </summary>
        private (double lat, double lon) ProjectPointOntoSegment(
            double px, double py, // הנקודה להטלה
            double x1, double y1, // תחילת הקטע
            double x2, double y2  // סוף הקטע
        )
        {
            // חישוב וקטור הקטע
            double dx = x2 - x1;
            double dy = y2 - y1;

            // אם הקטע הוא נקודה
            if (Math.Abs(dx) < 1e-10 && Math.Abs(dy) < 1e-10)
                return (x1, y1);

            // חישוב פרמטר ההטלה t
            double t = ((px - x1) * dx + (py - y1) * dy) / (dx * dx + dy * dy);

            // הגבלת t לתחום [0,1] כדי שהנקודה תהיה על הקטע
            t = Math.Max(0, Math.Min(1, t));

            // חישוב נקודת ההטלה
            double projX = x1 + t * dx;
            double projY = y1 + t * dy;

            return (projX, projY);
        }

        /// <summary>
        /// חישוב מרחק מנקודה לקטע ישר
        /// </summary>
        private double DistanceFromPointToLineSegment(
            double px, double py, // הנקודה
            double x1, double y1, // תחילת הקטע
            double x2, double y2  // סוף הקטע
        )
        {
            var projection = ProjectPointOntoSegment(px, py, x1, y1, x2, y2);

            // חישוב מרחק אווירי מהנקודה המקורית לנקודת ההטלה
            return Math.Sqrt(
                Math.Pow(px - projection.lat, 2) +
                Math.Pow(py - projection.lon, 2)
            );
        }

        /// <summary>
        /// חישוב מרחק גיאוגרפי במטרים (בקירוב)
        /// </summary>
        private double CalculateDistanceInMeters(double lat1, double lon1, double lat2, double lon2)
        {
            // שימוש בנוסחת Haversine לדיוק טוב יותר
            const double R = 6371000; // רדיוס כדור הארץ במטרים

            double lat1Rad = lat1 * Math.PI / 180;
            double lat2Rad = lat2 * Math.PI / 180;
            double deltaLat = (lat2 - lat1) * Math.PI / 180;
            double deltaLon = (lon2 - lon1) * Math.PI / 180;

            double a = Math.Sin(deltaLat / 2) * Math.Sin(deltaLat / 2) +
                       Math.Cos(lat1Rad) * Math.Cos(lat2Rad) *
                       Math.Sin(deltaLon / 2) * Math.Sin(deltaLon / 2);

            double c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return R * c;
        }

        /// <summary>
        /// בדיקה האם צומת הוא צומת אסטרטגי
        /// </summary>
        public bool IsStrategicNode(long nodeId)
        {
            return nodeId >= 100_000_000_000L;
        }

        // שאר הפונקציות הקיימות נשארות כמו שהן...
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

        public Graph FilterNodes(HashSet<long> allowedNodes)
        {
            var filteredGraph = new Graph();
            foreach (var nodeId in allowedNodes)
            {
                if (Nodes.ContainsKey(nodeId))
                {
                    filteredGraph.Nodes[nodeId] = Nodes[nodeId];
                }
            }
            foreach (var node in filteredGraph.Nodes.Values)
            {
                node.Edges = node.Edges.Where(edge => filteredGraph.Nodes.ContainsKey(edge.To.Id)).ToList();
            }
            return filteredGraph;
        }
    }
}
    //public class Graph
    //{
    //    public Dictionary<long, Node> Nodes { get; set; } = new();

    //    public void AddEdge(long from, long to, double weight = 1)
    //    {
    //        if (!Nodes.ContainsKey(from) || !Nodes.ContainsKey(to))
    //            return;

    //        Nodes[from].Edges.Add(new Edge { To = Nodes[to], Weight = weight });
    //        Nodes[to].Edges.Add(new Edge { To = Nodes[from], Weight = weight }); // דו-כיווני
    //    }

    //    public bool IsConnected()
    //    {
    //        if (!Nodes.Any()) return false;

    //        var visited = new HashSet<long>();
    //        var queue = new Queue<Node>();
    //        var first = Nodes.Values.First();
    //        queue.Enqueue(first);
    //        visited.Add(first.Id);

    //        while (queue.Any())
    //        {
    //            var current = queue.Dequeue();
    //            foreach (var edge in current.Edges)
    //            {
    //                if (!visited.Contains(edge.To.Id))
    //                {
    //                    visited.Add(edge.To.Id);
    //                    queue.Enqueue(edge.To);
    //                }
    //            }
    //        }

    //        return visited.Count == Nodes.Count;
    //    }

    //    public List<HashSet<long>> GetConnectedComponents()
    //    {
    //        var visited = new HashSet<long>();
    //        var components = new List<HashSet<long>>();

    //        foreach (var node in Nodes.Values)
    //        {
    //            if (!visited.Contains(node.Id))
    //            {
    //                var component = new HashSet<long>();
    //                var stack = new Stack<Node>();
    //                stack.Push(node);

    //                while (stack.Any())
    //                {
    //                    var current = stack.Pop();
    //                    if (!visited.Add(current.Id)) continue;
    //                    component.Add(current.Id);

    //                    foreach (var edge in current.Edges)
    //                    {
    //                        if (!visited.Contains(edge.To.Id))
    //                            stack.Push(edge.To);
    //                    }
    //                }

    //                components.Add(component);
    //            }
    //        }

    //        return components;
    //    }

    //    public List<(long from, long to)> GetAllEdges()
    //    {
    //        var edges = new HashSet<(long, long)>();

    //        foreach (var node in Nodes.Values)
    //        {
    //            foreach (var edge in node.Edges)
    //            {
    //                var a = node.Id;
    //                var b = edge.To.Id;
    //                if (a < b) edges.Add((a, b));
    //                else edges.Add((b, a));
    //            }
    //        }

    //        return edges.ToList();
    //    }

    //    public void AddNode(long nodeId, double lat, double lon)
    //    {
    //        if (!Nodes.ContainsKey(nodeId))
    //        {
    //            Nodes[nodeId] = new Node
    //            {
    //                Id = nodeId,
    //                Latitude = lat,
    //                Longitude = lon,
    //                Edges = new List<Edge>()
    //            };
    //        }
    //    }
    //    public Graph FilterNodes(HashSet<long> allowedNodes)
    //    {
    //        var filteredGraph = new Graph();

    //        foreach (var nodeId in allowedNodes)
    //        {
    //            if (Nodes.ContainsKey(nodeId))
    //            {
    //                filteredGraph.Nodes[nodeId] = Nodes[nodeId];
    //            }
    //        }

    //        foreach (var node in filteredGraph.Nodes.Values)
    //        {
    //            node.Edges = node.Edges.Where(edge => filteredGraph.Nodes.ContainsKey(edge.To.Id)).ToList();

    //        }

    //        return filteredGraph;
    //    }

    //}

