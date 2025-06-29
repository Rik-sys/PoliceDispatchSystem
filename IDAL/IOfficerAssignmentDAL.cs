
//ממשק לניהול מידע על שיוך שוטרים לאירועים
using DBEntities.Models;

namespace IDAL
{
    public interface IOfficerAssignmentDAL
    {
        void AddAssignments(List<OfficerAssignment> assignments);
        void AddAssignment(OfficerAssignment assignment);
        List<OfficerAssignment> GetAssignmentsByEventId(int eventId);
        List<OfficerAssignment> GetAssignmentsByOfficerId(int officerId);
        List<OfficerAssignment> GetAllAssignments();
        void DeleteAssignmentsByEventId(int eventId);
        void DeleteAssignmentsByOfficerId(int officerId);
        bool AssignmentExists(int officerId, int eventId);
        // צריך להוסיף למחלקה OfficerAssignmentDAL
        public void UpdateAssignment(OfficerAssignment assignment);
        
    }
}
