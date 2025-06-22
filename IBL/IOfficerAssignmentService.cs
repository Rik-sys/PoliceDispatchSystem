using DTO;

namespace IBL
{
    public interface IOfficerAssignmentService
    {
        void AddOfficerAssignments(List<OfficerAssignmentDTO> assignments);
        List<OfficerAssignmentDTO> GetAssignmentsByEventId(int eventId);
        List<OfficerAssignmentDTO> GetAssignmentsByOfficerId(int officerId);
        List<OfficerAssignmentDTO> GetAllAssignments();
        void DeleteAssignmentsByEventId(int eventId);
        void DeleteAssignmentsByOfficerId(int officerId);
        bool IsOfficerAssignedToEvent(int officerId, int eventId);
        List<PoliceOfficerDTO> GetOfficersForEvent(int eventId); // עם פרטים מלאים
        public void UpdateOfficerAssignments(List<OfficerAssignmentDTO> assignments);

    }
}