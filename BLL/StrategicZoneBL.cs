using DTO;
using IBL;
using IDAL;

namespace BLL
{
    public class StrategicZoneBL : IStrategicZoneBL
    {
        private readonly IStrategicZoneDAL _dal;

        public StrategicZoneBL(IStrategicZoneDAL dal)
        {
            _dal = dal;
        }

        public void AddStrategicZones(List<StrategicZoneDTO> zones)
        {
            _dal.AddStrategicZones(zones);
        }

        public List<StrategicZoneDTO> GetStrategicZonesForEvent(int eventId)
        {
            return _dal.GetStrategicZonesForEvent(eventId);
        }
        public List<StrategicZoneDTO> GetAllStrategicZones()
        {
            return _dal.GetAllStrategicZones();
        }

    }
}
