using DBEntities.Models;
using IDAL;

namespace DAL
{
    public class EventDAL : IEventDAL
    {
        private readonly PoliceDispatchSystemContext _context;

        public EventDAL(PoliceDispatchSystemContext context)
        {
            _context = context;
        }

        public int AddEvent(Event eventEntity)
        {
            _context.Events.Add(eventEntity);
            _context.SaveChanges();
            return eventEntity.EventId;
        }

        public void AddEventZone(EventZone zone)
        {
            _context.EventZones.Add(zone);
            _context.SaveChanges();
        }

        public Event? GetEventById(int eventId)
        {
            return _context.Events.FirstOrDefault(e => e.EventId == eventId);
        }
    }
}
