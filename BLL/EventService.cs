//using AutoMapper;
//using DBEntities.Models;
//using DTO;
//using IBL;
//using IDAL;

//namespace BLL
//{
//    public class EventService : IEventService
//    {
//        private readonly IEventDAL _eventDal;
//        private readonly IMapper _mapper;

//        public EventService(IEventDAL eventDal)
//        {
//            _eventDal = eventDal;

//            var config = new MapperConfiguration(cfg =>
//            {
//                cfg.CreateMap<EventDTO, Event>().ReverseMap();
//                cfg.CreateMap<EventZoneDTO, EventZone>().ReverseMap();
//            });

//            _mapper = config.CreateMapper();
//        }

//        public int CreateEventWithZone(EventDTO eventDto, EventZoneDTO zoneDto)
//        {
//            // המרה מ־DTO ל־Entity
//            var eventEntity = _mapper.Map<Event>(eventDto);
//            int eventId = _eventDal.AddEvent(eventEntity);

//            var zoneEntity = _mapper.Map<EventZone>(zoneDto);
//            zoneEntity.EventId = eventId;

//            _eventDal.AddEventZone(zoneEntity);

//            return eventId;
//        }

//        public EventDTO GetEventById(int eventId)
//        {
//            var ev = _eventDal.GetEventById(eventId);
//            return _mapper.Map<EventDTO>(ev);
//        }

//        public List<PoliceOfficer> GetAvailableOfficersForEvent(DateOnly date, TimeOnly start, TimeOnly end)
//        {
//            using var context = new PoliceDispatchSystemContext();

//            // שליפת כל השוטרים ששובצו באירועים חופפים
//            var busyOfficerIds = context.OfficerAssignments
//                .Where(assign => context.Events.Any(ev =>
//                    ev.EventId == assign.EventId &&
//                    ev.EventDate == date &&
//                    (
//                        (ev.StartTime <= end && ev.EndTime >= start)
//                    )
//                ))
//                .Select(assign => assign.PoliceOfficerId)
//                .Distinct()
//                .ToList();

//            // שליפת רק שוטרים שאינם תפוסים
//            var availableOfficers = context.PoliceOfficers
//                .Where(p => !busyOfficerIds.Contains(p.PoliceOfficerId))
//                .ToList();

//            return availableOfficers;
//        }

//    }
//}
using AutoMapper;
using DBEntities.Models;
using DTO;
using IBL;
using IDAL;

namespace BLL
{
    public class EventService : IEventService
    {
        private readonly IEventDAL _eventDal;
        private readonly IPoliceOfficerDAL _policeOfficerDal;
        private readonly IMapper _mapper;

        public EventService(IEventDAL eventDal, IPoliceOfficerDAL policeOfficerDal)
        {
            _eventDal = eventDal;
            _policeOfficerDal = policeOfficerDal;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<EventDTO, Event>().ReverseMap();
                cfg.CreateMap<EventZoneDTO, EventZone>().ReverseMap();

                // מיפוי של User (DTO ← → Entity)
                cfg.CreateMap<UserDTO, DBEntities.Models.User>().ReverseMap();

                // מיפוי של PoliceOfficer עם User מקונן
                cfg.CreateMap<PoliceOfficer, PoliceOfficerDTO>()
                    .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.PoliceOfficerNavigation));

                cfg.CreateMap<PoliceOfficerDTO, PoliceOfficer>()
                    .ForMember(dest => dest.PoliceOfficerNavigation, opt => opt.MapFrom(src => src.User));
            });
            _mapper = config.CreateMapper();
        }

        public int CreateEventWithZone(EventDTO eventDto, EventZoneDTO zoneDto)
        {
            var eventEntity = _mapper.Map<Event>(eventDto);
            int eventId = _eventDal.AddEvent(eventEntity);

            var zoneEntity = _mapper.Map<EventZone>(zoneDto);
            zoneEntity.EventId = eventId;
            _eventDal.AddEventZone(zoneEntity);

            return eventId;
        }

        public EventDTO GetEventById(int eventId)
        {
            var ev = _eventDal.GetEventById(eventId);
            return _mapper.Map<EventDTO>(ev);
        }

        public List<PoliceOfficerDTO> GetAvailableOfficersForEvent(DateOnly date, TimeOnly start, TimeOnly end)
        {
            // קריאה דרך DAL עם Include של Users
            var availableOfficers = _policeOfficerDal.GetAvailableOfficersWithUsers(date, start, end);

            // המרה ל-DTO הקיים
            return _mapper.Map<List<PoliceOfficerDTO>>(availableOfficers);
        }
    }
}