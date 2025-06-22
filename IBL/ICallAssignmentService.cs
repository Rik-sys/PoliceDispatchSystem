//ממשק-לוגיקת שיוך שוטרים לקריאה
using DTO;

namespace IBL
{
    public interface ICallAssignmentService
    {
        void AssignOfficersToCall(List<CallAssignmentDTO> assignments);
        List<CallAssignmentDTO> GetAssignmentsByCall(int callId);
    }
}
