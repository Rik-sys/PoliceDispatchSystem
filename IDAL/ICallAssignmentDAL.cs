using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface ICallAssignmentDAL
    {
        void AddAssignments(List<CallAssignment> assignments);
        List<CallAssignment> GetAssignmentsByCallId(int callId);
    }
}
