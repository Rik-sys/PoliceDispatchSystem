//מחלקה שמייצגת את האובייקט מטבלת שוטר במסד
namespace DTO
{
    public class PoliceOfficerDTO
    {
        public int PoliceOfficerId { get; set; }

        public int? VehicleTypeId { get; set; }
        public UserDTO User { get; set; } = new UserDTO();

    }
}


