using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class PoliceOfficerDTO
    {
        public int PoliceOfficerId { get; set; }

        public int? VehicleTypeId { get; set; }
        public UserDTO User { get; set; } = new UserDTO();

    }
}


