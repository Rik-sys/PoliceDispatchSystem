//ממשק לניהול מידע על אירוע
using DBEntities.Models;

namespace IDAL
{
    public interface IEventDAL
    {
        int AddEvent(Event eventEntity);
        void AddEventZone(EventZone zone);
        Event? GetEventById(int eventId);
        List<Event> GetEvents();
        List<EventZone> GetAllEventZones();
        EventZone? GetEventZoneByEventId(int eventId);
        void DeleteEvent(int eventId);
        void DeleteEventZoneByEventId(int eventId);
        List<Event> GetEventsByDateRange(DateOnly EventDate);
        void UpdateEvent(Event eventEntity);
        bool EventExists(int eventId);

    }
}
