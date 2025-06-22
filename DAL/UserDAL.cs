using DBEntities.Models;
using IDAL;

namespace DAL
{
    public class UserDAL : IUserDAL
    {
        private readonly PoliceDispatchSystemContext _context;

        public UserDAL(PoliceDispatchSystemContext context)
        {
            _context = context;
        }

        public void AddUser(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void DeleteUser(User user)
        {
            _context.Users.Remove(user);
            _context.SaveChanges();
        }

        public User? GetUserById(int userId)
        {
            return _context.Users
                .FirstOrDefault(u => u.UserId == userId);
        }

        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }
    }
}
