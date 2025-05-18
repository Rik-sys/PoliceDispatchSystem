using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class Edge
    {
        public Node To { get; set; } //הצומת שאליה הקשת מציבעה
        public double Weight { get; set; } = 1;//משקל הקשת
    }
}
