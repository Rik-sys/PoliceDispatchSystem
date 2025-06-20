//מחלקה לייצוג צומת בגרף
namespace DTO
{
    public class Node
    {
        public long Id { get; set; }

        //קו רוחב של מיקום הצומת
        public double Latitude { get; set; }

        //קו אורך של מיקום הצומת
        public double Longitude { get; set; }

        //ייצגתי את הגרף באמצעות רשימת שכנויות-וזה רשימת הקשתות שיוצאות מצומת מסוים
        public List<Edge> Edges { get; set; } = new();
    }
}
