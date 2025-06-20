
//מחלקה שמייצגת את השיוך בפועל למסד נתונים של שיוך שוטר לקריאה
namespace DTO
{
    public class CallAssignmentDTO
    {
        public int PoliceOfficerId { get; set; }
        public int CallId { get; set; }
        public DateTime AssignmentTime { get; set; } = DateTime.Now;
    }
}
