using DBEntities.Models;

namespace IDAL
{
    public interface IEventDAL
    {
        int AddEvent(Event eventEntity);
        void AddEventZone(EventZone zone);
        Event? GetEventById(int eventId);

        List<Event> GetEvents();
    }
}
