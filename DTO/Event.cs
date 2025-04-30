using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    internal class Event
    {
        public int EventId { get; set; }

        public string EventName { get; set; } = null!;

        public DateOnly EventDate { get; set; }

        public TimeOnly StartTime { get; set; }

        public TimeOnly EndTime { get; set; }

        public int RequiredOfficers { get; set; }
    }
}
