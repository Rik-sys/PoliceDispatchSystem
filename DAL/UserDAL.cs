using DAL.Models;
using DBEntities.Models;
using IDAL;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL
{
    public class UserDAL : IUserDAL
    {
        public void AddUser(User user)
        {
            throw new NotImplementedException();
        }

        public void UpdateUser(User user)
        {
            throw new NotImplementedException();
        }

        public void DeleteUser(User user)
        {
            throw new NotImplementedException();
        }

        public User GetUserById(int userId)
        {
            throw new NotImplementedException();
        }

        public List<User> GetAllUsers()
        {
            try
            {
                using PoliceDispatchSystemContext context = new PoliceDispatchSystemContext();
                return context.Users.Select(u => (User)u).ToList();
            }
            catch {
                return null;
            }
        }
    }
    
}
