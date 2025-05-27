using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    internal class PoliceOfficer
    {
        public int PoliceOfficerId { get; set; }

        public int? VehicleTypeId { get; set; }

        public virtual User PoliceOfficerNavigation { get; set; } = null!;

        public virtual VehicleType? VehicleType { get; set; }

        public virtual ICollection<EventDTO> Events { get; set; } = new List<EventDTO>();
    }
}
