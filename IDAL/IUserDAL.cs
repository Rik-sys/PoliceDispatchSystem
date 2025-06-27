
//ממשק לניהול מידע על יוזר
using DBEntities.Models;
namespace IDAL
{
    public interface IUserDAL
    {
        public void AddUser(User user);
        public void DeleteUser(User user);
        public User GetUserById(int userId);
        public List<User> GetAllUsers();
    }
}
