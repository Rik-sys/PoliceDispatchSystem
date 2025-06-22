//ממשק לניהול קריאות
using DBEntities.Models;
public interface ICallDAL
{
    int AddCall(Call call);
    List<Call> GetCallsByEventId(int eventId);
    List<Call> GetAllCalls();

}