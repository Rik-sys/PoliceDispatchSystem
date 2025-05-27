using DTO;
using DBEntities.Models;

namespace IBL
{
    public interface IEventService
    {
        int CreateEventWithZone(EventDTO eventDto, EventZoneDTO zoneDto);
        EventDTO GetEventById(int eventId);
        List<PoliceOfficer> GetAvailableOfficersForEvent(DateOnly date, TimeOnly start, TimeOnly end);

    }
}
