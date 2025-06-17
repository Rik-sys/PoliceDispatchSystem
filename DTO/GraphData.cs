using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class GraphData
    {
        public Dictionary<long, (double lat, double lon)> Nodes { get; set; }
        public Graph Graph { get; set; }
        public Dictionary<long, bool> NodesInOriginalBounds { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
