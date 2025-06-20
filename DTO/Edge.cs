//מחלקה לייצוג של קשת בגרף
namespace DTO
{
    public class Edge
    {
        //אל איזה צומת הקשת מגיעה
        public Node To { get; set; }
        public double Weight { get; set; } = 1;
    }
}
