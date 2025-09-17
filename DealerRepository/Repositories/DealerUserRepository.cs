using DealerRepository.Data;
using DealerRepository.Model;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Repositories
{
    public class DealerUserRepository : GenericRepository<DealerUser>, IDealerUserRepository
    {
        private readonly DealerDbContext _context;
        public DealerUserRepository(DealerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<DealerUser?> GetByUserAndDealerAsync(int userId, int dealerId)
        {
            return await _context.DealerUsers
            .FirstOrDefaultAsync(x => x.UserId == userId && x.DealerId == dealerId);
        }

        public async Task<DealerUser> GetDealerUserByUserId(int userId)
        {
            return await _context.DealerUsers
            .FirstOrDefaultAsync(x => x.UserId == userId);
        }

        public async Task<IEnumerable<DealerUser>> GetUsersByDealerIdAsync(int dealerId)
        {
            return await _context.DealerUsers
                .Where(u => u.DealerId == dealerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<DealerUser>> SearchDealerUsersAsync(string searchTerm)
        {
            return await _context.DealerUsers
                .Where(u =>
                    u.Position.Contains(searchTerm)) // tuỳ bạn search theo field nào
                .ToListAsync();
        }
    }
}
