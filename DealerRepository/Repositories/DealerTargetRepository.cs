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
    public class DealerTargetRepository : GenericRepository<DealerTargets>, IDealerTargetRepository
    {
        private readonly DealerDbContext _context;
        public DealerTargetRepository(DealerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<DealerTargets?> GetByDealerAndPeriodAsync(int dealerId, int year, int month)
        {
            return await _context.DealerTargets.Include(dt => dt.Dealer)
                .FirstOrDefaultAsync(dt => dt.DealerId == dealerId && dt.TargetYear == year && dt.TargetMonth == month);
        }

        public async Task<DealerTargets?> GetDealerTargetsByDealerId(int dealerId)
        {
           return await _context.DealerTargets
                .Include(dt => dt.Dealer)
                .Where(dt => dt.DealerId == dealerId)
                .OrderByDescending(dt => dt.TargetYear)
                .ThenByDescending(dt => dt.TargetMonth)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<DealerTargets>> GetTargetsByDealerAsync(int dealerId, int? year = null, int? month = null)
        {
            var query = _context.DealerTargets
                .Include(dt => dt.Dealer)
                .Where(dt => dt.DealerId == dealerId);
            if (year.HasValue)
            {
                query = query.Where(dt => dt.TargetYear == year.Value);
            }
            if (month.HasValue)
            {
                query = query.Where(dt => dt.TargetMonth == month.Value);
            }
            return await query.ToListAsync();
        }

        public async Task<IEnumerable<DealerTargets>> GetTargetsReportAsync(int? year = null, int? month = null)
        {
            var query = _context.DealerTargets
                .Include(dt => dt.Dealer)
                .AsQueryable();
            if (year.HasValue)
            {
                query = query.Where(dt => dt.TargetYear == year.Value);
            }
            if (month.HasValue)
            {
                query = query.Where(dt => dt.TargetMonth == month.Value);
            }
            return await query.ToListAsync();
        }

        public async Task UpdateAchievedSalesAsync(int targetId, int achievedSales)
        {
            var target = await _context.DealerTargets.FindAsync(targetId);
            if(target == null)
            {
                throw new KeyNotFoundException($"Dealer target with ID {targetId} not found.");
            }
            target.AchievedSales += achievedSales;
            _context.DealerTargets.Update(target);
            await _context.SaveChangesAsync();
        }
    }
}
