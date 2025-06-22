//ממשק לניהול מידע על אזורים אסטרטגיים
using DBEntities.Models;

namespace IDAL
{
    public interface IStrategicZoneDAL
    {
        void AddStrategicZones(List<StrategicZone> zones);
        void AddStrategicZone(StrategicZone zone);
        List<StrategicZone> GetStrategicZonesForEvent(int eventId);
        List<StrategicZone> GetAllStrategicZones();
        void DeleteStrategicZonesByEventId(int eventId);
        void DeleteStrategicZone(int zoneId);
        StrategicZone GetStrategicZoneById(int zoneId);
    }
}