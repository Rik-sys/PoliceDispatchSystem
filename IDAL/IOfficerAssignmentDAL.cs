using DBEntities.Models;

namespace IDAL
{
    public interface IOfficerAssignmentDAL
    {
        void AddAssignments(List<OfficerAssignment> list);
        List<OfficerAssignment> GetAssignmentsByEventId(int eventId);
        List<OfficerAssignment> GetAssignmentsByOfficerId(int officerId);
    }
}
