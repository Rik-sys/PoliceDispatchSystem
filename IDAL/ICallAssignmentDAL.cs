
//ממשק לניהול שיוך שוטרים לקריאות
using DBEntities.Models;
namespace IDAL
{
    public interface ICallAssignmentDAL
    {

        void AddAssignments(List<CallAssignment> assignments);
        List<CallAssignment> GetAssignmentsByCallId(int callId);
    }
}
