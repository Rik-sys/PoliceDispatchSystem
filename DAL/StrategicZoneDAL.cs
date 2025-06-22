//using DTO;
//using IDAL;
//using DBEntities.Models;

//namespace DAL
//{
//    public class StrategicZoneDAL : IStrategicZoneDAL
//    {
//        private readonly PoliceDispatchSystemContext _context;

//        public StrategicZoneDAL(PoliceDispatchSystemContext context)
//        {
//            _context = context;
//        }

//        public void AddStrategicZones(List<StrategicZoneDTO> zones)
//        {
//            var entities = zones.Select(z => new StrategicZone
//            {
//                EventId = z.EventId,
//                Latitude = z.Latitude,
//                Longitude = z.Longitude,
//                StrategyLevel = z.StrategyLevel > 0 ? z.StrategyLevel : 1
//            }).ToList();

//            _context.StrategicZones.AddRange(entities);
//            _context.SaveChanges();
//        }

//        public List<StrategicZoneDTO> GetStrategicZonesForEvent(int eventId)
//        {
//            return _context.StrategicZones
//                .Where(z => z.EventId == eventId)
//                .Select(z => new StrategicZoneDTO
//                {
//                    StrategicZoneId = z.StrategicZoneId,
//                    EventId = z.EventId,
//                    Latitude = z.Latitude,
//                    Longitude = z.Longitude,
//                    StrategyLevel = z.StrategyLevel
//                }).ToList();
//        }
//        public List<StrategicZoneDTO> GetAllStrategicZones()
//        {
//            return _context.StrategicZones
//                .Select(z => new StrategicZoneDTO
//                {
//                    StrategicZoneId = z.StrategicZoneId,
//                    EventId = z.EventId,
//                    Latitude = z.Latitude,
//                    Longitude = z.Longitude,
//                    StrategyLevel = z.StrategyLevel
//                }).ToList();
//        }

//        public void AddStrategicZone(StrategicZoneDTO zone)
//        {
//            var entity = new StrategicZone
//            {
//                EventId = zone.EventId,
//                Latitude = zone.Latitude,
//                Longitude = zone.Longitude,
//                StrategyLevel = zone.StrategyLevel > 0 ? zone.StrategyLevel : 1
//            };

//            _context.StrategicZones.Add(entity);
//            _context.SaveChanges();
//        }


//        public void DeleteStrategicZonesByEventId(int eventId)
//        {
//            var zones = _context.StrategicZones
//                .Where(z => z.EventId == eventId)
//                .ToList();

//            if (zones.Any())
//            {
//                _context.StrategicZones.RemoveRange(zones);
//                _context.SaveChanges();
//            }
//        }


//        public void DeleteStrategicZone(int zoneId)
//        {
//            var zone = _context.StrategicZones
//                .FirstOrDefault(z => z.StrategicZoneId == zoneId);

//            if (zone != null)
//            {
//                _context.StrategicZones.Remove(zone);
//                _context.SaveChanges();
//            }
//        }


//        public StrategicZoneDTO? GetStrategicZoneById(int zoneId)
//        {
//            var zone = _context.StrategicZones
//                .FirstOrDefault(z => z.StrategicZoneId == zoneId);

//            if (zone == null)
//                return null;

//            return new StrategicZoneDTO
//            {
//                StrategicZoneId = zone.StrategicZoneId,
//                EventId = zone.EventId,
//                Latitude = zone.Latitude,
//                Longitude = zone.Longitude,
//                StrategyLevel = zone.StrategyLevel
//            };
//        }

//    }
//}
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
