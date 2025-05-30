//using DTO;
//using DBEntities.Models;

//namespace IBL
//{
//    public interface IEventService
//    {
//        int CreateEventWithZone(EventDTO eventDto, EventZoneDTO zoneDto);
//        EventDTO GetEventById(int eventId);
//        List<PoliceOfficer> GetAvailableOfficersForEvent(DateOnly date, TimeOnly start, TimeOnly end);

//    }
//}
using DBEntities.Models;
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
    }
}