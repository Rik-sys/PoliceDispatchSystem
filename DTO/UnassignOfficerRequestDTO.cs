//בקשה שמגיעה מצד הלקוח כדי לבטל שיוך של שוטר לקריאה מסוימת.
namespace DTO
{
    public class UnassignOfficerRequestDTO
    {
        public int CallId { get; set; }
        public int OfficerId { get; set; }
    }
}
