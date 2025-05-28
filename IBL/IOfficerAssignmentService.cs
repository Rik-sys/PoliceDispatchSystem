using DTO;

namespace IBL
{
    public interface IOfficerAssignmentService
    {
        void AddOfficerAssignments(List<OfficerAssignmentDTO> assignments);
        List<OfficerAssignmentDTO> GetAssignmentsByEventId(int eventId);
        List<OfficerAssignmentDTO> GetAssignmentsByOfficerId(int officerId);
    }
}