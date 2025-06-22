
using DBEntities.Models;
namespace DAL
{
    public class CallDAL : ICallDAL
    {
        private readonly PoliceDispatchSystemContext _context;

        public CallDAL(PoliceDispatchSystemContext context)
        {
            _context = context;
        }

        public int AddCall(Call call)
        {
            _context.Calls.Add(call);
            _context.SaveChanges();
            return call.CallId;
        }

        public List<Call> GetCallsByEventId(int eventId)
        {
            return _context.Calls
                .Where(c => c.EventId == eventId)
                .ToList();
        }
        public List<Call> GetAllCalls()
        {
            return _context.Calls.ToList();
        }

    }

}

