using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    internal class Call
    {
        public int CallId { get; set; }

        public int? EventId { get; set; }

        public string Address { get; set; } = null!;

        public int RequiredOfficers { get; set; }

        public string ContactPhone { get; set; } = null!;

        public int UrgencyLevel { get; set; }

        public DateTime CallTime { get; set; }

        public string Status { get; set; } = null!;

    }
}
