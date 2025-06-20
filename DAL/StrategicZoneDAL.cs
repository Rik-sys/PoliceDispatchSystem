using DTO;
using IDAL;
using DBEntities.Models;

namespace DAL
{
    public class StrategicZoneDAL : IStrategicZoneDAL
    {
        private readonly PoliceDispatchSystemContext _context;

        public StrategicZoneDAL(PoliceDispatchSystemContext context)
        {
            _context = context;
        }

        public void AddStrategicZones(List<StrategicZoneDTO> zones)
        {
            var entities = zones.Select(z => new StrategicZone
            {
                EventId = z.EventId,
                Latitude = z.Latitude,
                Longitude = z.Longitude,
                StrategyLevel = z.StrategyLevel > 0 ? z.StrategyLevel : 1
            }).ToList();

            _context.StrategicZones.AddRange(entities);
            _context.SaveChanges();
        }

        public List<StrategicZoneDTO> GetStrategicZonesForEvent(int eventId)
        {
            return _context.StrategicZones
                .Where(z => z.EventId == eventId)
                .Select(z => new StrategicZoneDTO
                {
                    StrategicZoneId = z.StrategicZoneId,
                    EventId = z.EventId,
                    Latitude = z.Latitude,
                    Longitude = z.Longitude,
                    StrategyLevel = z.StrategyLevel
                }).ToList();
        }
        public List<StrategicZoneDTO> GetAllStrategicZones()
        {
            return _context.StrategicZones
                .Select(z => new StrategicZoneDTO
                {
                    StrategicZoneId = z.StrategicZoneId,
                    EventId = z.EventId,
                    Latitude = z.Latitude,
                    Longitude = z.Longitude,
                    StrategyLevel = z.StrategyLevel
                }).ToList();
        }

    }
}
