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
            foreach (var assignment in list)
            {
                if (!_context.OfficerAssignments.Any(a =>
                    a.PoliceOfficerId == assignment.PoliceOfficerId &&
                    a.EventId == assignment.EventId))
                {
                    _context.OfficerAssignments.Add(assignment);
                }
            }
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
        public List<OfficerAssignment> GetAllAssignments()
        {
            return _context.OfficerAssignments.ToList();
        }

        public void AddAssignment(OfficerAssignment assignment)
        {
            if (!_context.OfficerAssignments.Any(a =>
                a.PoliceOfficerId == assignment.PoliceOfficerId &&
                a.EventId == assignment.EventId))
            {
                _context.OfficerAssignments.Add(assignment);
                _context.SaveChanges();
            }
        }


        public void DeleteAssignmentsByEventId(int eventId)
        {
            var toRemove = _context.OfficerAssignments
                .Where(a => a.EventId == eventId)
                .ToList();

            if (toRemove.Any())
            {
                _context.OfficerAssignments.RemoveRange(toRemove);
                _context.SaveChanges();
            }
        }


        public void DeleteAssignmentsByOfficerId(int officerId)
        {
            var toRemove = _context.OfficerAssignments
                .Where(a => a.PoliceOfficerId == officerId)
                .ToList();

            if (toRemove.Any())
            {
                _context.OfficerAssignments.RemoveRange(toRemove);
                _context.SaveChanges();
            }
        }

        public bool AssignmentExists(int officerId, int eventId)
        {
            return _context.OfficerAssignments
                .Any(a => a.PoliceOfficerId == officerId && a.EventId == eventId);
        }
        
        public void UpdateAssignment(OfficerAssignment assignment)
        {
            var existing = _context.OfficerAssignments
                .FirstOrDefault(a => a.PoliceOfficerId == assignment.PoliceOfficerId &&
                                    a.EventId == assignment.EventId);

            if (existing != null)
            {
                existing.Latitude = assignment.Latitude;
                existing.Longitude = assignment.Longitude;
                _context.SaveChanges();
            }
        }
    }
}