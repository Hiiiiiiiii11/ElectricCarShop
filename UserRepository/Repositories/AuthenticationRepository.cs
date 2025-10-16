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
    public class AuthenticationRepository : GenericRepository<Users>, IAuthenticationRepository
    {
        private readonly UserDbContext _context;
        public AuthenticationRepository(UserDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<Users> GetUserByEmailAsync(string email)
        {
            return await _context.Users
                         .Include(u => u.Role) // <--- DÒNG QUAN TRỌNG NHẤT
                         .FirstOrDefaultAsync(u => u.Email == email);
        }
    }
}
