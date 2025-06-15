using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class Edge
    {
        //אל איזה צומת הקשת מגיעה
        public Node To { get; set; }
        public double Weight { get; set; } = 1;
    }
}
