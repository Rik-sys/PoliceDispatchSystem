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
           // לוגיקה נכונה לבדיקת חפיפה
           (ev.StartTime < end && ev.EndTime > start)
       ))
       .Select(assign => assign.PoliceOfficerId)
       .Distinct()
       .ToList();

            // ראשית - שלוף שוטרים בלי Include
            var availableOfficers = _context.PoliceOfficers
                .Include(p => p.VehicleType) // רק VehicleType
                .Where(p => !busyOfficerIds.Contains(p.PoliceOfficerId))
                .ToList();

            // לוג לבדיקה
            Console.WriteLine($"Found {availableOfficers.Count} officers before user check");

            //  Include אחר כך - אם יש בעיה, לפחות שיהיה שוטרים
            try
            {
                var officersWithUsers = _context.PoliceOfficers
                    .Include(p => p.PoliceOfficerNavigation) // User
                    .Include(p => p.VehicleType)
                    .Where(p => !busyOfficerIds.Contains(p.PoliceOfficerId))
                    .Where(p => p.PoliceOfficerNavigation != null) // רק עם משתמש תקין!
                    .ToList();

                Console.WriteLine($"Found {officersWithUsers.Count} officers WITH users");
                return officersWithUsers;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Include failed: {ex.Message}");
                Console.WriteLine($"Returning {availableOfficers.Count} officers without users");
                return availableOfficers;
            }
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
            .FirstOrDefault(p => p.PoliceOfficerId == officerId)!; // סימון שאני בטוחה שזה לא null

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

        public PoliceOfficer? GetOfficerById(int officerId)
        {
            return _context.PoliceOfficers
                .FirstOrDefault(o => o.PoliceOfficerId == officerId);
        }


    }
}
