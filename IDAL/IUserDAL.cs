using DBEntities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IDAL
{
    public interface IUserDAL
    {
        public void AddUser(User user);
        public void UpdateUser(User user);
        public void DeleteUser(User user);
        public User GetUserById(int userId);
        public List<User> GetAllUsers();
    }
}
