using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class CallAssignmentDTO
    {
        public int PoliceOfficerId { get; set; }
        public int CallId { get; set; }
        public DateTime AssignmentTime { get; set; } = DateTime.Now;
    }
}
