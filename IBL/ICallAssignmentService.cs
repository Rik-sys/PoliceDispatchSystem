using DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBL
{
    public interface ICallAssignmentService
    {
        void AssignOfficersToCall(List<CallAssignmentDTO> assignments);
        List<CallAssignmentDTO> GetAssignmentsByCall(int callId);
    }
}
