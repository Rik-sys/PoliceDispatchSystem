using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class User
    {
        public int UserId { get; set; }

        public string Username { get; set; } = null!;

        public string PasswordHash { get; set; } = null!;

        public string Idnumber { get; set; } = null!;

        public string Phone { get; set; } = null!;

        public string Email { get; set; } = null!;

    }
}
