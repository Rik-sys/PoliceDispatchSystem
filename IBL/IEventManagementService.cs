using static DTO.EventRequestsDTO;

namespace IBL
{
    public interface IEventManagementService
    {
        /// <summary>
        /// יוצר אירוע חדש עם פיזור אוטומטי של שוטרים
        /// </summary>
        Task<EventCreationResultDTO> CreateEventWithAutoDistribution(CreateEventRequestDTO request);

        /// <summary>
        /// יוצר אירוע עם מיקומים מחושבים מראש
        /// </summary>
        Task<EventCreationResultDTO> CreateEventWithPreCalculatedPositions(CreateEventWithPositionsRequestDTO request);

        /// <summary>
        /// מוחק אירוע ומנקה את כל הקשרים הקשורים
        /// </summary>
        Task<bool> DeleteEventComplete(int eventId);

        /// <summary>
        /// מחשב אם שוטר זמין לאירוע נתון
        /// </summary>
        bool IsOfficerAvailableForEvent(int officerId, DateOnly date, TimeOnly startTime, TimeOnly endTime);

        /// <summary>
        /// מחזיר את כל האירועים עם פרטים מלאים
        /// </summary>
        List<EventWithDetailsDTO> GetAllEventsWithDetails();

        /// <summary>
        /// מחזיר אירוע עם כל הפרטים הקשורים
        /// </summary>
        EventWithDetailsDTO GetEventWithDetails(int eventId);
    }
}