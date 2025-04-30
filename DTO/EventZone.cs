using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    internal class EventZone
    {
        public int ZoneId { get; set; }

        public int? EventId { get; set; }

        public double Latitude1 { get; set; }

        public double Longitude1 { get; set; }

        public double Latitude2 { get; set; }

        public double Longitude2 { get; set; }

        public double Latitude3 { get; set; }

        public double Longitude3 { get; set; }

        public double Latitude4 { get; set; }

        public double Longitude4 { get; set; }

        public virtual Event? Event { get; set; }
    }
}
