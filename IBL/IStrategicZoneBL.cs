using DTO;

namespace IBL
{
    public interface IStrategicZoneBL
    {
        void AddStrategicZones(List<StrategicZoneDTO> zones);
        List<StrategicZoneDTO> GetStrategicZonesForEvent(int eventId);
        List<StrategicZoneDTO> GetAllStrategicZones();

    }
}
