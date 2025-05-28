using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class DispatcherDTO
    {
        public int DispatcherId { get; set; }

        public virtual UserDTO DispatcherNavigation { get; set; } = null!;
    }
}
