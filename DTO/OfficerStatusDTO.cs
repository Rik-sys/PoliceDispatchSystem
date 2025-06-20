//מחלקה לייצוג המצב של השוטר-כי שכחתי לעשות עמודת סטטוס בטבלה במסד שלו

namespace DTO
{
    public class OfficerStatusDTO
    {
        public int OfficerId { get; set; }
        public string Status { get; set; } = string.Empty; // "Available", "AssignedToEvent", "AssignedToCall"
    }

}
