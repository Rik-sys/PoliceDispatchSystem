
using DBEntities.Models;
using IDAL;

namespace DAL
{
    public class CallAssignmentDAL : ICallAssignmentDAL
    {
        private readonly PoliceDispatchSystemContext _context;

        public CallAssignmentDAL(PoliceDispatchSystemContext context)
        {
            _context = context;
        }

        public void AddAssignments(List<CallAssignment> assignments)
        {
            _context.CallAssignments.AddRange(assignments);
            _context.SaveChanges();
        }

        public List<CallAssignment> GetAssignmentsByCallId(int callId)
        {
            return _context.CallAssignments
                .Where(ca => ca.CallId == callId)
                .ToList();
        }
    }
}
