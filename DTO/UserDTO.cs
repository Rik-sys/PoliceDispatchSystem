//אובייקט שמייצג את הטבלה יוזר במסד
namespace DTO
{
    public class UserDTO
    {
        public int UserId { get; set; }

        public string Username { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Idnumber { get; set; } = null!;

        public string Phone { get; set; } = null!;

        public string Email { get; set; } = null!;

    }
}
