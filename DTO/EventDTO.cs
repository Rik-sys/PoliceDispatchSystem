
namespace DTO
{

    public class EventDTO
    {
        public int EventId { get; set; }
        public string EventName { get; set; } = null!;
        public DateOnly EventDate { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int RequiredOfficers { get; set; }

        public string? Description { get; set; }
        public string? Priority { get; set; }
    }
}