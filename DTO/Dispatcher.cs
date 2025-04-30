using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    internal class Dispatcher
    {
        public int DispatcherId { get; set; }

        public virtual User DispatcherNavigation { get; set; } = null!;
    }
}
