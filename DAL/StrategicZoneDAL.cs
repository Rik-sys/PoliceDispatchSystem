
using DBEntities.Models;
using IDAL;

namespace DAL
{
    public class StrategicZoneDAL : IStrategicZoneDAL
    {
        private readonly PoliceDispatchSystemContext _context;

        public StrategicZoneDAL(PoliceDispatchSystemContext context)
        {
            _context = context;
        }

        public void AddStrategicZones(List<StrategicZone> zones)
        {
            _context.StrategicZones.AddRange(zones);
            _context.SaveChanges();
        }

        public void AddStrategicZone(StrategicZone zone)
        {
            _context.StrategicZones.Add(zone);
            _context.SaveChanges();
        }

        public List<StrategicZone> GetStrategicZonesForEvent(int eventId)
        {
            return _context.StrategicZones
                .Where(z => z.EventId == eventId)
                .ToList();
        }

        public List<StrategicZone> GetAllStrategicZones()
        {
            return _context.StrategicZones.ToList();
        }

        public void DeleteStrategicZonesByEventId(int eventId)
        {
            var zones = _context.StrategicZones
                .Where(z => z.EventId == eventId)
                .ToList();

            if (zones.Any())
            {
                _context.StrategicZones.RemoveRange(zones);
                _context.SaveChanges();
            }
        }

        public void DeleteStrategicZone(int zoneId)
        {
            var zone = _context.StrategicZones
                .FirstOrDefault(z => z.StrategicZoneId == zoneId);

            if (zone != null)
            {
                _context.StrategicZones.Remove(zone);
                _context.SaveChanges();
            }
        }

        public StrategicZone? GetStrategicZoneById(int zoneId)
        {
            return _context.StrategicZones
                .FirstOrDefault(z => z.StrategicZoneId == zoneId);
        }
    }
}
