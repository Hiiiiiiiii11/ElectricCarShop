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
    public class DealerRepository :GenericRepository<Dealers>, IDealerRepository
    {
        private readonly DealerDbContext _context;
        public DealerRepository(DealerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Dealers> GetDealerByNameAsync(string dealerName)
        {
            return await _context.Dealers
                .FirstOrDefaultAsync(d => d.DealerName == dealerName);
        }

        public async Task<IEnumerable<Dealers>> SearchDealersAsync(string searchTerm)
        {
            return await _context.Dealers
                .Where(d => d.DealerName.Contains(searchTerm) || d.Address.Contains(searchTerm))
                .ToListAsync()
                .ContinueWith(task => task.Result.AsEnumerable());
        }
    }
}
