using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    internal class StrategicZone
    {
        public int StrategicZoneId { get; set; }

        public int? EventId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int StrategyLevel { get; set; }
    }
}
