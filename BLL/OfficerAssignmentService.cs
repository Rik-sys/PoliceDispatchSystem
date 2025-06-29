
using AutoMapper;
using DBEntities.Models;
using DTO;
using IBL;
using IDAL;
using Microsoft.Extensions.Logging;

namespace BLL
{
    public class OfficerAssignmentService : IOfficerAssignmentService
    {
        private readonly IOfficerAssignmentDAL _officerAssignmentDal;
        private readonly IPoliceOfficerDAL _policeOfficerDal;
        private readonly IEventDAL _eventDal;
        private readonly IMapper _mapper;
        private readonly ILogger<OfficerAssignmentService> _logger;

        public OfficerAssignmentService(
            IOfficerAssignmentDAL officerAssignmentDal,
            IPoliceOfficerDAL policeOfficerDal,
            IEventDAL eventDal,
            ILogger<OfficerAssignmentService> logger)
        {
            _officerAssignmentDal = officerAssignmentDal;
            _policeOfficerDal = policeOfficerDal;
            _eventDal = eventDal;
            _logger = logger;

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<OfficerAssignmentDTO, OfficerAssignment>().ReverseMap();
                cfg.CreateMap<PoliceOfficer, PoliceOfficerDTO>()
                    .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.PoliceOfficerNavigation));
                cfg.CreateMap<EventDTO, Event>().ReverseMap();
                cfg.CreateMap<UserDTO, User>().ReverseMap();
            });
            _mapper = config.CreateMapper();
        }

        public void AddOfficerAssignments(List<OfficerAssignmentDTO> assignments)
        {
            try
            {
                if (assignments == null || !assignments.Any())
                {
                    _logger.LogWarning("Attempted to add empty assignments list");
                    return;
                }

                _logger.LogInformation($"Adding {assignments.Count} officer assignments");

                // בדיקת תקינות השיוכים
                var validatedAssignments = ValidateAssignments(assignments);

                if (!validatedAssignments.Any())
                {
                    _logger.LogWarning("No valid assignments to add after validation");
                    return;
                }

                // המרה מ-DTO ל-Entity
                var entities = _mapper.Map<List<OfficerAssignment>>(validatedAssignments);


                // שמירה דרך שכבת ה-DAL
                _officerAssignmentDal.AddAssignments(entities);

                _logger.LogInformation($"Successfully added {entities.Count} officer assignments");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding officer assignments");
                throw;
            }
        }

        public List<OfficerAssignmentDTO> GetAssignmentsByEventId(int eventId)
        {
            try
            {
                var entities = _officerAssignmentDal.GetAssignmentsByEventId(eventId);
                return _mapper.Map<List<OfficerAssignmentDTO>>(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting assignments for event {eventId}");
                throw;
            }
        }

        public List<OfficerAssignmentDTO> GetAssignmentsByOfficerId(int officerId)
        {
            try
            {
                var entities = _officerAssignmentDal.GetAssignmentsByOfficerId(officerId);
                return _mapper.Map<List<OfficerAssignmentDTO>>(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting assignments for officer {officerId}");
                throw;
            }
        }

        public List<OfficerAssignmentDTO> GetAllAssignments()
        {
            try
            {
                var entities = _officerAssignmentDal.GetAllAssignments();
                return _mapper.Map<List<OfficerAssignmentDTO>>(entities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all assignments");
                throw;
            }
        }

        public void DeleteAssignmentsByEventId(int eventId)
        {
            try
            {
                _logger.LogInformation($"Deleting assignments for event {eventId}");

                var assignments = _officerAssignmentDal.GetAssignmentsByEventId(eventId);
                if (assignments.Any())
                {
                    _officerAssignmentDal.DeleteAssignmentsByEventId(eventId);
                    _logger.LogInformation($"Deleted {assignments.Count} assignments for event {eventId}");
                }
                else
                {
                    _logger.LogInformation($"No assignments found for event {eventId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting assignments for event {eventId}");
                throw;
            }
        }

        public void DeleteAssignmentsByOfficerId(int officerId)
        {
            try
            {
                _logger.LogInformation($"Deleting assignments for officer {officerId}");

                var assignments = _officerAssignmentDal.GetAssignmentsByOfficerId(officerId);
                if (assignments.Any())
                {
                    _officerAssignmentDal.DeleteAssignmentsByOfficerId(officerId);
                    _logger.LogInformation($"Deleted {assignments.Count} assignments for officer {officerId}");
                }
                else
                {
                    _logger.LogInformation($"No assignments found for officer {officerId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting assignments for officer {officerId}");
                throw;
            }
        }

        public bool IsOfficerAssignedToEvent(int officerId, int eventId)
        {
            try
            {
                var assignment = _officerAssignmentDal.GetAssignmentsByEventId(eventId)
                    .FirstOrDefault(a => a.PoliceOfficerId == officerId);

                return assignment != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking if officer {officerId} is assigned to event {eventId}");
                return false;
            }
        }

        public List<PoliceOfficerDTO> GetOfficersForEvent(int eventId)
        {
            try
            {
                _logger.LogDebug($"Getting officers for event {eventId}");

                // שליפת השיוכים
                var assignments = _officerAssignmentDal.GetAssignmentsByEventId(eventId);
                var officerIds = assignments.Select(a => a.PoliceOfficerId).Distinct().ToList();

                if (!officerIds.Any())
                {
                    _logger.LogInformation($"No officers assigned to event {eventId}");
                    return new List<PoliceOfficerDTO>();
                }

                // שליפת השוטרים עם הפרטים המלאים
                var officers = new List<PoliceOfficerDTO>();
                foreach (var officerId in officerIds)
                {
                    var officer = _policeOfficerDal.GetOfficerWithUserById(officerId);
                    if (officer != null)
                    {
                        officers.Add(_mapper.Map<PoliceOfficerDTO>(officer));
                    }
                }

                _logger.LogInformation($"Found {officers.Count} officers for event {eventId}");
                return officers;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting officers for event {eventId}");
                throw;
            }
        }


        /// <summary>
        /// מוודא שהשיוכים תקינים לפני השמירה
        /// </summary>
        private List<OfficerAssignmentDTO> ValidateAssignments(List<OfficerAssignmentDTO> assignments)
        {
            var validAssignments = new List<OfficerAssignmentDTO>();

            foreach (var assignment in assignments)
            {
                try
                {
                    // בדיקה שהשוטר קיים
                    var officer = _policeOfficerDal.GetOfficerWithUserById(assignment.PoliceOfficerId);
                    if (officer == null)
                    {
                        _logger.LogWarning($"Officer {assignment.PoliceOfficerId} not found, skipping assignment");
                        continue;
                    }

                    // בדיקה שהאירוע קיים
                    var eventEntity = _eventDal.GetEventById(assignment.EventId);
                    if (eventEntity == null)
                    {
                        _logger.LogWarning($"Event {assignment.EventId} not found, skipping assignment");
                        continue;
                    }

                    // בדיקה שהשוטר לא כבר משויך לאירוע הזה
                    if (IsOfficerAssignedToEvent(assignment.PoliceOfficerId, assignment.EventId))
                    {
                        _logger.LogWarning($"Officer {assignment.PoliceOfficerId} already assigned to event {assignment.EventId}, skipping");
                        continue;
                    }

                    // בדיקה שהמיקום תקין
                    if (!IsValidCoordinate(assignment.Latitude, assignment.Longitude))
                    {
                        _logger.LogWarning($"Invalid coordinates ({assignment.Latitude}, {assignment.Longitude}), skipping assignment");
                        continue;
                    }

                    validAssignments.Add(assignment);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error validating assignment for officer {assignment.PoliceOfficerId}");
                }
            }

            _logger.LogInformation($"Validated {validAssignments.Count} out of {assignments.Count} assignments");
            return validAssignments;
        }

        /// <summary>
        /// בודק אם קואורדינטות תקינות
        /// </summary>
        private bool IsValidCoordinate(double latitude, double longitude)
        {
            return latitude >= -90 && latitude <= 90 &&
                   longitude >= -180 && longitude <= 180 &&
                   !double.IsNaN(latitude) && !double.IsNaN(longitude) &&
                   !double.IsInfinity(latitude) && !double.IsInfinity(longitude);
        }

        
        public void UpdateOfficerAssignments(List<OfficerAssignmentDTO> assignments)
        {
            try
            {
                if (assignments == null || !assignments.Any())
                {
                    _logger.LogWarning("Attempted to update empty assignments list");
                    return;
                }

                _logger.LogInformation($"Updating {assignments.Count} officer assignments");

                foreach (var assignment in assignments)
                {
                    // מציאת הרשומה הקיימת
                    var existingAssignment = _officerAssignmentDal.GetAssignmentsByEventId(assignment.EventId)
                        .FirstOrDefault(a => a.PoliceOfficerId == assignment.PoliceOfficerId);

                    if (existingAssignment != null)
                    {
                        // עדכון המיקום
                        existingAssignment.Latitude = assignment.Latitude;
                        existingAssignment.Longitude = assignment.Longitude;

                        // שמירה במסד
                        _officerAssignmentDal.UpdateAssignment(existingAssignment);
                    }
                    else
                    {
                        _logger.LogWarning($"Assignment not found for officer {assignment.PoliceOfficerId} in event {assignment.EventId}");
                    }
                }

                _logger.LogInformation($"Successfully updated {assignments.Count} officer assignments");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating officer assignments");
                throw;
            }
        }
    }
}