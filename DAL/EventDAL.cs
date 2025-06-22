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

        public List<Event> GetEvents()
        {
            return _context.Events.ToList();
        }
        public List<EventZone> GetAllEventZones()
        {
            return _context.EventZones.ToList();
        }
        public EventZone? GetEventZoneByEventId(int eventId)
        {
            return _context.EventZones.FirstOrDefault(z => z.EventId == eventId);
        }

        public void DeleteEvent(int eventId)
        {
            var ev = _context.Events.FirstOrDefault(e => e.EventId == eventId);
            if (ev != null)
            {
                _context.Events.Remove(ev);
                _context.SaveChanges();
            }
        }

        public void DeleteEventZoneByEventId(int eventId)
        {
            var zone = _context.EventZones.FirstOrDefault(z => z.EventId == eventId);
            if (zone != null)
            {
                _context.EventZones.Remove(zone);
                _context.SaveChanges();
            }
        }

        public List<Event> GetEventsByDateRange(DateOnly eventDate)
        {
            return _context.Events
                .Where(e => e.EventDate == eventDate)
                .ToList();
        }



        public void UpdateEvent(Event eventEntity)
        {
            var existing = _context.Events.FirstOrDefault(e => e.EventId == eventEntity.EventId);
            if (existing != null)
            {
                existing.EventName = eventEntity.EventName;
                existing.Description = eventEntity.Description;
                existing.Priority = eventEntity.Priority;
                existing.EventDate= eventEntity.EventDate;
                existing.StartTime = eventEntity.StartTime;
                existing.EndTime = eventEntity.EndTime;
                existing.RequiredOfficers = eventEntity.RequiredOfficers;

                _context.SaveChanges();
            }
        }

        public bool EventExists(int eventId)
        {
            return _context.Events.Any(e => e.EventId == eventId);
        }

    }
}
