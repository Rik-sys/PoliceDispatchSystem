using AutoMapper;
using DTO;
using IBL;
using IDAL;
using DBEntities.Models;
using Microsoft.Extensions.Logging;
namespace BLL
{
    public class EventService : IEventService
    {
        private readonly IEventDAL _eventDal;
        private readonly IPoliceOfficerDAL _policeOfficerDal;
        private readonly IMapper _mapper;
        private readonly ILogger<EventService> _logger;

        public EventService(
            IEventDAL eventDal,
            IPoliceOfficerDAL policeOfficerDal,
            ILogger<EventService> logger)
        {
            _eventDal = eventDal;
            _policeOfficerDal = policeOfficerDal;
            _logger = logger;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<EventDTO, Event>().ReverseMap();
                cfg.CreateMap<EventZoneDTO, EventZone>().ReverseMap();
                cfg.CreateMap<UserDTO, User>().ReverseMap();
                cfg.CreateMap<PoliceOfficer, PoliceOfficerDTO>()
                    .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.PoliceOfficerNavigation));
                cfg.CreateMap<PoliceOfficerDTO, PoliceOfficer>()
                    .ForMember(dest => dest.PoliceOfficerNavigation, opt => opt.MapFrom(src => src.User));
            });
            _mapper = config.CreateMapper();
        }

        public int CreateEventWithZone(EventDTO eventDto, EventZoneDTO zoneDto)
        {
            try
            {
                _logger.LogInformation($"Creating event: {eventDto.EventName}");

                var eventEntity = _mapper.Map<Event>(eventDto);
                int eventId = _eventDal.AddEvent(eventEntity);

                var zoneEntity = _mapper.Map<EventZone>(zoneDto);
                zoneEntity.EventId = eventId;
                _eventDal.AddEventZone(zoneEntity);

                _logger.LogInformation($"Event {eventId} created successfully");
                return eventId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating event: {eventDto.EventName}");
                throw;
            }
        }

        public EventDTO GetEventById(int eventId)
        {
            try
            {
                var eventEntity = _eventDal.GetEventById(eventId);
                if (eventEntity == null)
                {
                    _logger.LogWarning($"Event {eventId} not found");
                    return null;
                }

                return _mapper.Map<EventDTO>(eventEntity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting event {eventId}");
                throw;
            }
        }

        public List<PoliceOfficerDTO> GetAvailableOfficersForEvent(DateOnly date, TimeOnly start, TimeOnly end)
        {
            try
            {
                _logger.LogDebug($"Getting available officers for {date} {start}-{end}");

                // קריאה דרך DAL עם Include של Users
                var availableOfficers = _policeOfficerDal.GetAvailableOfficersWithUsers(date, start, end);
                return _mapper.Map<List<PoliceOfficerDTO>>(availableOfficers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting available officers for {date} {start}-{end}");
                throw;
            }
        }

        public void DeleteEvent(int eventId)
        {
            try
            {
                _logger.LogInformation($"Deleting event {eventId}");

                // בדיקה שהאירוע קיים
                var existingEvent = _eventDal.GetEventById(eventId);
                if (existingEvent == null)
                {
                    throw new ArgumentException($"Event {eventId} not found");
                }

                // מחיקת האירוע 
                _eventDal.DeleteEvent(eventId);

                _logger.LogInformation($"Event {eventId} deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting event {eventId}");
                throw;
            }
        }

        public void DeleteEventComplete(int eventId)
        {
            try
            {
                _logger.LogInformation($"Performing complete deletion of event {eventId}");

                // בינתיים רק מחיקה בסיסית
                _eventDal.DeleteEvent(eventId);

                _logger.LogInformation($"Complete deletion of event {eventId} finished successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in complete deletion of event {eventId}");
                throw;
            }
        }

        public List<EventDTO> GetEvents()
        {
            try
            {
                var events = _eventDal.GetEvents();
                return events.Select(ev => _mapper.Map<EventDTO>(ev)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all events");
                throw;
            }
        }

        public List<EventZoneDTO> GetAllEventZones()
        {
            try
            {
                var zones = _eventDal.GetAllEventZones();
                return zones.Select(z => _mapper.Map<EventZoneDTO>(z)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all event zones");
                throw;
            }
        }

        public EventZoneDTO? GetEventZoneByEventId(int eventId)
        {
            try
            {
                var zone = _eventDal.GetEventZoneByEventId(eventId);
                return zone != null ? _mapper.Map<EventZoneDTO>(zone) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting zone for event {eventId}");
                throw;
            }
        }

        public List<EventDTO> GetEventsByDateRange(DateOnly EventDate)
        {
            try
            {
                var events = _eventDal.GetEventsByDateRange(EventDate);
                return events.Select(ev => _mapper.Map<EventDTO>(ev)).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting events for date {EventDate}");
                throw;
            }
        }

        public bool IsEventActive(int eventId, DateTime currentTime)
        {
            try
            {
                var eventDto = GetEventById(eventId);
                if (eventDto == null) return false;

                var eventDateTime = eventDto.EventDate.ToDateTime(eventDto.StartTime);
                var eventEndDateTime = eventDto.EventDate.ToDateTime(eventDto.EndTime);

                return currentTime >= eventDateTime && currentTime <= eventEndDateTime;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if event {eventId} is active");
                return false;
            }
        }


    }
}
