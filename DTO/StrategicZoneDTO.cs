
//מחלקה שמייצגת אובייקט מקביל לטבלה אזורים אסטרטגיים במסד
namespace DTO
{
    public class StrategicZoneDTO
    {
        public int StrategicZoneId { get; set; }

        public int? EventId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public int StrategyLevel { get; set; } = 1;
    }
}

