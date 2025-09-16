using Microsoft.EntityFrameworkCore;
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
    public class RoleRepository : GenericRepository<Roles>, IRoleRepository
    {
        private readonly UserDbContext _context;
        public RoleRepository(UserDbContext context) : base(context) {
            _context = context;
        }

        public async Task<bool> GetByRoleNameAsync(string roleName)
        {
            return await _context.Roles.AnyAsync(r => r.RoleName == roleName);
        }
    }
}
