
//תפקיד המחלקה
//מייצגת את הבקשה שמגיעה מצד לקוח כאשר מוקדנית רוצה לשייך שוטר לקריאה
namespace DTO
{
    public class AssignOfficerRequestDTO
    {
        public int CallId { get; set; }
        public int OfficerId { get; set; }
    }
}
