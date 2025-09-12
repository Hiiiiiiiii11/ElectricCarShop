using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserRepository.Model
{
    public class Roles
    {
        public int Id { get; set; }
        public string RoleName { get; set; }

        public ICollection<UserRoles> UserRoles { get; set; }
    }
}
