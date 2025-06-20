using DBEntities.Models;
using DTO;
using IDAL;
using Microsoft.EntityFrameworkCore;

namespace DAL
{
    public class PoliceOfficerDAL : IPoliceOfficerDAL
    {
        private readonly PoliceDispatchSystemContext _context;

        public PoliceOfficerDAL(PoliceDispatchSystemContext context)
        {
            _context = context;
        }

        public List<PoliceOfficer> GetAvailableOfficersWithUsers(DateOnly date, TimeOnly start, TimeOnly end)
        {
            // שליפת כל השוטרים ששובצו באירועים חופפים
            var busyOfficerIds = _context.OfficerAssignments
                .Where(assign => _context.Events.Any(ev =>
                    ev.EventId == assign.EventId &&
                    ev.EventDate == date &&
                    (ev.StartTime <= end && ev.EndTime >= start)
                ))
                .Select(assign => assign.PoliceOfficerId)
                .Distinct()
                .ToList();

            // שליפת רק שוטרים שאינם תפוסים + Include של User ו-VehicleType
            return _context.PoliceOfficers
                .Include(p => p.PoliceOfficerNavigation) // זה ה-User
                .Include(p => p.VehicleType) // אופציונלי אם רוצים גם סוג רכב
                .Where(p => !busyOfficerIds.Contains(p.PoliceOfficerId))
                .ToList();
        }

        public List<PoliceOfficer> GetAllOfficersWithUsers()
        {
            return _context.PoliceOfficers
                .Include(p => p.PoliceOfficerNavigation)
                .Include(p => p.VehicleType)
                .ToList();
        }

        public PoliceOfficer GetOfficerWithUserById(int officerId)
        {
            return _context.PoliceOfficers
                .Include(p => p.PoliceOfficerNavigation)
                .Include(p => p.VehicleType)
                .FirstOrDefault(p => p.PoliceOfficerId == officerId);
        }

        public OfficerStatusDTO GetOfficerStatus(int officerId)
        {
            bool isInCall = _context.CallAssignments.Any(c => c.PoliceOfficerId == officerId);
            if (isInCall)
                return new OfficerStatusDTO { OfficerId = officerId, Status = "AssignedToCall" };

            bool isInEvent = _context.OfficerAssignments.Any(e => e.PoliceOfficerId == officerId);
            if (isInEvent)
                return new OfficerStatusDTO { OfficerId = officerId, Status = "AssignedToEvent" };

            return new OfficerStatusDTO { OfficerId = officerId, Status = "Available" };
        }

    }
}
