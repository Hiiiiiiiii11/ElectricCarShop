using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System.Linq;
using System.Threading.Tasks;
using UserRepository.Data;
using UserRepository.Model;

namespace UserRepository.Repositories
{
    public class UserRepository : GenericRepository<Users>, IUserRepository
    {
        private readonly UserDbContext _context;

        public UserRepository(UserDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Users>> GetAllUsersWithRolesAsync()
        {
            return await _context.Users
                .Include(u => u.Role)
                .ToListAsync();
        }
        public async Task<Users?> GetUserWithRolesAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<Users?> GetByUserNameAsync(string userName)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.UserName == userName);
        }
        public async Task<bool> AssignUserToAgencyAsync(int userId, int agencyId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            user.AgencyId = agencyId;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
