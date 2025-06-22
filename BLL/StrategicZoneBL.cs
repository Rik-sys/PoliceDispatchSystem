//using DTO;
//using IBL;
//using IDAL;

//namespace BLL
//{
//    public class StrategicZoneBL : IStrategicZoneBL
//    {
//        private readonly IStrategicZoneDAL _dal;

//        public StrategicZoneBL(IStrategicZoneDAL dal)
//        {
//            _dal = dal;
//        }

//        public void AddStrategicZones(List<StrategicZoneDTO> zones)
//        {
//            _dal.AddStrategicZones(zones);
//        }

//        public List<StrategicZoneDTO> GetStrategicZonesForEvent(int eventId)
//        {
//            return _dal.GetStrategicZonesForEvent(eventId);
//        }
//        public List<StrategicZoneDTO> GetAllStrategicZones()
//        {
//            return _dal.GetAllStrategicZones();
//        }

//    }
//}
//using IBL;
//using IDAL;
//using DTO;
//using Microsoft.Extensions.Logging;

//namespace BLL
//{
//    public class StrategicZoneBL : IStrategicZoneBL
//    {
//        private readonly IStrategicZoneDAL _dal;
//        private readonly ILogger<StrategicZoneBL> _logger;

//        public StrategicZoneBL(IStrategicZoneDAL dal, ILogger<StrategicZoneBL> logger)
//        {
//            _dal = dal;
//            _logger = logger;
//        }

//        public void AddStrategicZones(List<StrategicZoneDTO> zones)
//        {
//            try
//            {
//                if (zones == null || !zones.Any())
//                {
//                    _logger.LogWarning("Attempted to add empty strategic zones list");
//                    return;
//                }

//                _logger.LogInformation($"Adding {zones.Count} strategic zones");

//                // בדיקת תקינות האזורים
//                var validZones = ValidateStrategicZones(zones);

//                if (!validZones.Any())
//                {
//                    _logger.LogWarning("No valid strategic zones to add after validation");
//                    return;
//                }

//                // הוספת זמן יצירה ואתחול ערכים
//                foreach (var zone in validZones)
//                {
//                    // במסד שלך יש StrategyLevel במקום Priority
//                    if (zone.StrategyLevel <= 0) zone.StrategyLevel = 1;
//                }

//                _dal.AddStrategicZones(validZones);
//                _logger.LogInformation($"Successfully added {validZones.Count} strategic zones");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error adding strategic zones");
//                throw;
//            }
//        }

//        public List<StrategicZoneDTO> GetStrategicZonesForEvent(int eventId)
//        {
//            try
//            {
//                _logger.LogDebug($"Getting strategic zones for event {eventId}");
//                var zones = _dal.GetStrategicZonesForEvent(eventId);
//                _logger.LogInformation($"Found {zones.Count} strategic zones for event {eventId}");
//                return zones;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error getting strategic zones for event {eventId}");
//                throw;
//            }
//        }

//        public List<StrategicZoneDTO> GetAllStrategicZones()
//        {
//            try
//            {
//                var zones = _dal.GetAllStrategicZones();
//                _logger.LogInformation($"Retrieved {zones.Count} strategic zones");
//                return zones;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Error getting all strategic zones");
//                throw;
//            }
//        }

//        public void DeleteStrategicZonesByEventId(int eventId)
//        {
//            try
//            {
//                _logger.LogInformation($"Deleting strategic zones for event {eventId}");

//                var zones = _dal.GetStrategicZonesForEvent(eventId);
//                if (zones.Any())
//                {
//                    _dal.DeleteStrategicZonesByEventId(eventId);
//                    _logger.LogInformation($"Deleted {zones.Count} strategic zones for event {eventId}");
//                }
//                else
//                {
//                    _logger.LogInformation($"No strategic zones found for event {eventId}");
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error deleting strategic zones for event {eventId}");
//                throw;
//            }
//        }

//        public void DeleteStrategicZone(int zoneId)
//        {
//            try
//            {
//                _logger.LogInformation($"Deleting strategic zone {zoneId}");

//                var zone = _dal.GetStrategicZoneById(zoneId);
//                if (zone == null)
//                {
//                    _logger.LogWarning($"Strategic zone {zoneId} not found");
//                    throw new ArgumentException($"Strategic zone {zoneId} not found");
//                }

//                _dal.DeleteStrategicZone(zoneId);
//                _logger.LogInformation($"Strategic zone {zoneId} deleted successfully");
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error deleting strategic zone {zoneId}");
//                throw;
//            }
//        }

//        public StrategicZoneDTO? GetStrategicZoneById(int zoneId)
//        {
//            try
//            {
//                return _dal.GetStrategicZoneById(zoneId);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error getting strategic zone {zoneId}");
//                throw;
//            }
//        }

//        public int CountStrategicZonesForEvent(int eventId)
//        {
//            try
//            {
//                var zones = _dal.GetStrategicZonesForEvent(eventId);
//                return zones.Count;
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, $"Error counting strategic zones for event {eventId}");
//                return 0;
//            }
//        }

//        #region Private Helper Methods

//        /// <summary>
//        /// מוודא שהאזורים האסטרטגיים תקינים
//        /// </summary>
//        private List<StrategicZoneDTO> ValidateStrategicZones(List<StrategicZoneDTO> zones)
//        {
//            var validZones = new List<StrategicZoneDTO>();

//            foreach (var zone in zones)
//            {
//                try
//                {
//                    // בדיקה שהמיקום תקין
//                    if (!IsValidCoordinate(zone.Latitude, zone.Longitude))
//                    {
//                        _logger.LogWarning($"Invalid coordinates ({zone.Latitude}, {zone.Longitude}), skipping strategic zone");
//                        continue;
//                    }

//                    // בדיקה שיש EventId
//                    if (!zone.EventId.HasValue || zone.EventId.Value <= 0)
//                    {
//                        _logger.LogWarning($"Invalid EventId {zone.EventId}, skipping strategic zone");
//                        continue;
//                    }

//                    // בדיקת עדיפות
//                    if (zone.StrategyLevel <= 0)
//                    {
//                        _logger.LogDebug($"Setting default strategy level 1 for strategic zone");
//                        zone.StrategyLevel = 1;
//                    }

//                    validZones.Add(zone);
//                }
//                catch (Exception ex)
//                {
//                    _logger.LogError(ex, $"Error validating strategic zone at ({zone.Latitude}, {zone.Longitude})");
//                }
//            }

//            _logger.LogInformation($"Validated {validZones.Count} out of {zones.Count} strategic zones");
//            return validZones;
//        }

//        /// <summary>
//        /// בודק אם קואורדינטות תקינות
//        /// </summary>
//        private bool IsValidCoordinate(double latitude, double longitude)
//        {
//            return latitude >= -90 && latitude <= 90 &&
//                   longitude >= -180 && longitude <= 180 &&
//                   !double.IsNaN(latitude) && !double.IsNaN(longitude) &&
//                   !double.IsInfinity(latitude) && !double.IsInfinity(longitude);
//        }

//        #endregion
//    }
//}
using AutoMapper;
using DBEntities.Models;
using DTO;
using IBL;
using IDAL;
using Microsoft.Extensions.Logging;

namespace BLL
{
    public class StrategicZoneBL : IStrategicZoneBL
    {
        private readonly IStrategicZoneDAL _dal;
        private readonly ILogger<StrategicZoneBL> _logger;
        private readonly IMapper _mapper;

        public StrategicZoneBL(
            IStrategicZoneDAL dal,
            ILogger<StrategicZoneBL> logger)
        {
            _dal = dal;
            _logger = logger;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<StrategicZone, StrategicZoneDTO>().ReverseMap();
            });
            _mapper = config.CreateMapper();
        }

        public void AddStrategicZones(List<StrategicZoneDTO> zones)
        {
            try
            {
                if (zones == null || !zones.Any())
                {
                    _logger.LogWarning("Attempted to add empty strategic zones list");
                    return;
                }

                _logger.LogInformation($"Adding {zones.Count} strategic zones");

                var validZones = ValidateStrategicZones(zones);

                if (!validZones.Any())
                {
                    _logger.LogWarning("No valid strategic zones to add after validation");
                    return;
                }

                
                var entities = _mapper.Map<List<StrategicZone>>(validZones);
                _dal.AddStrategicZones(entities);

                _logger.LogInformation($"Successfully added {entities.Count} strategic zones");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding strategic zones");
                throw;
            }
        }

        public List<StrategicZoneDTO> GetStrategicZonesForEvent(int eventId)
        {
            try
            {
                _logger.LogDebug($"Getting strategic zones for event {eventId}");
                var zones = _dal.GetStrategicZonesForEvent(eventId);
                var result = _mapper.Map<List<StrategicZoneDTO>>(zones);
                _logger.LogInformation($"Found {result.Count} strategic zones for event {eventId}");
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting strategic zones for event {eventId}");
                throw;
            }
        }

        public List<StrategicZoneDTO> GetAllStrategicZones()
        {
            try
            {
                var zones = _dal.GetAllStrategicZones();
                return _mapper.Map<List<StrategicZoneDTO>>(zones);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all strategic zones");
                throw;
            }
        }

        public void DeleteStrategicZonesByEventId(int eventId)
        {
            try
            {
                _logger.LogInformation($"Deleting strategic zones for event {eventId}");
                _dal.DeleteStrategicZonesByEventId(eventId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting strategic zones for event {eventId}");
                throw;
            }
        }

        public void DeleteStrategicZone(int zoneId)
        {
            try
            {
                _logger.LogInformation($"Deleting strategic zone {zoneId}");
                _dal.DeleteStrategicZone(zoneId);
                _logger.LogInformation($"Strategic zone {zoneId} deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting strategic zone {zoneId}");
                throw;
            }
        }

        public StrategicZoneDTO? GetStrategicZoneById(int zoneId)
        {
            try
            {
                var zone = _dal.GetStrategicZoneById(zoneId);
                return zone != null ? _mapper.Map<StrategicZoneDTO>(zone) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting strategic zone {zoneId}");
                throw;
            }
        }

        public int CountStrategicZonesForEvent(int eventId)
        {
            try
            {
                var zones = _dal.GetStrategicZonesForEvent(eventId);
                return zones.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error counting strategic zones for event {eventId}");
                return 0;
            }
        }


        private List<StrategicZoneDTO> ValidateStrategicZones(List<StrategicZoneDTO> zones)
        {
            var validZones = new List<StrategicZoneDTO>();

            foreach (var zone in zones)
            {
                try
                {
                    if (!IsValidCoordinate(zone.Latitude, zone.Longitude))
                    {
                        _logger.LogWarning($"Invalid coordinates ({zone.Latitude}, {zone.Longitude}), skipping");
                        continue;
                    }

                    if (!zone.EventId.HasValue || zone.EventId.Value <= 0)
                    {
                        _logger.LogWarning($"Invalid EventId {zone.EventId}, skipping");
                        continue;
                    }

                    if (zone.StrategyLevel <= 0)
                        zone.StrategyLevel = 1;

                    validZones.Add(zone);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error validating strategic zone ({zone.Latitude}, {zone.Longitude})");
                }
            }

            _logger.LogInformation($"Validated {validZones.Count} out of {zones.Count} zones");
            return validZones;
        }

        private bool IsValidCoordinate(double latitude, double longitude)
        {
            return latitude >= -90 && latitude <= 90 &&
                   longitude >= -180 && longitude <= 180 &&
                   !double.IsNaN(latitude) && !double.IsNaN(longitude) &&
                   !double.IsInfinity(latitude) && !double.IsInfinity(longitude);
        }

        
    }
}
