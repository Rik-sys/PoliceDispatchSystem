using DBEntities.Models;
using IDAL;

namespace DAL
{
    public class OfficerAssignmentDAL : IOfficerAssignmentDAL
    {
        private readonly PoliceDispatchSystemContext _context;

        public OfficerAssignmentDAL(PoliceDispatchSystemContext context)
        {
            _context = context;
        }

        public void AddAssignments(List<OfficerAssignment> list)
        {
            _context.OfficerAssignments.AddRange(list);
            _context.SaveChanges();
        }

        public List<OfficerAssignment> GetAssignmentsByEventId(int eventId)
        {
            return _context.OfficerAssignments
                .Where(oa => oa.EventId == eventId)
                .ToList();
        }

        public List<OfficerAssignment> GetAssignmentsByOfficerId(int officerId)
        {
            return _context.OfficerAssignments
                .Where(oa => oa.PoliceOfficerId == officerId)
                .ToList();
        }
    }
}