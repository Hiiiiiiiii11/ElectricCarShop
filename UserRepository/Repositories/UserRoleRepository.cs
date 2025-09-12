using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserRepository.Data;
using UserRepository.Model;

namespace UserRepository.Repositories
{
    public class UserRoleRepository : GenericRepository<UserRoles>, IUserRoleRepository
    {
        public UserRoleRepository(UserDbContext context) : base(context) { }
    }
}
