
using DTO;

namespace IBL
{
    public interface IEventService
    {
        int CreateEventWithZone(EventDTO eventDto, EventZoneDTO zoneDto);
        EventDTO GetEventById(int eventId);
        List<PoliceOfficerDTO> GetAvailableOfficersForEvent(DateOnly date, TimeOnly start, TimeOnly end); // חזרה ל-DTO הקיים
        void DeleteEvent(int eventId);
        public List<EventDTO> GetEvents();
        List<EventZoneDTO> GetAllEventZones();
        EventZoneDTO? GetEventZoneByEventId(int eventId);
        void DeleteEventComplete(int eventId); // מחיקה מלאה כולל קשרים
        List<EventDTO> GetEventsByDateRange(DateOnly EventDate);
        bool IsEventActive(int eventId, DateTime currentTime);

    }
}