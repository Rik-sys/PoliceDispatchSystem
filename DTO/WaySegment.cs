using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class WaySegment
    {
        public long WayId { get; set; }
        public long FromNodeId { get; set; }
        public long ToNodeId { get; set; }
        public (double lat, double lon) FromCoord { get; set; }
        public (double lat, double lon) ToCoord { get; set; }
        public string HighwayType { get; set; } = "";
    }
}
