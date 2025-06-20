//מחלקה שמייצגת אובייקט בטבלת שיוך שוטרים לאירוע
namespace DTO
{
    public class OfficerAssignmentDTO
    {
        public int PoliceOfficerId { get; set; }
        public int EventId { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}

