using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface IKCenterService
    {      
        (List<long> centers, double maxResponseTime) SolveKCenter(Graph graph, int k);
       // List<long> FindMinimumOfficersForResponseTime(Graph graph, double maxResponseTime);
    }
}
