using DTO;

namespace IDAL
{
    public interface IStrategicZoneDAL
    {
        void AddStrategicZones(List<StrategicZoneDTO> zones);
        List<StrategicZoneDTO> GetStrategicZonesForEvent(int eventId);
        List<StrategicZoneDTO> GetAllStrategicZones();

    }
}
