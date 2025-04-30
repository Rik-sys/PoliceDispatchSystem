using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface IGraphService
    {
        Graph BuildGraphFromOsm(string filePath);
    }
}
