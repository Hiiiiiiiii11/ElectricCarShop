using AgencyRepository.Data;
using AgencyRepository.Model;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Repositories
{
    public class AgencyTargetRepository : GenericRepository<AgencyTargets>, IAgencyTargetRepository
    {
        private readonly AgencyDbContext _context;
        public AgencyTargetRepository(AgencyDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<AgencyTargets?> GetByAgencyAndPeriodAsync(int AgencyId, int year, int month)
        {
            return await _context.AgencyTargets.Include(dt => dt.Agency)
                .FirstOrDefaultAsync(dt => dt.AgencyId == AgencyId && dt.TargetYear == year && dt.TargetMonth == month);
        }

        public async Task<AgencyTargets?> GetAgencyTargetsByAgencyId(int AgencyId)
        {
           return await _context.AgencyTargets
                .Include(dt => dt.Agency)
                .Where(dt => dt.AgencyId == AgencyId)
                .OrderByDescending(dt => dt.TargetYear)
                .ThenByDescending(dt => dt.TargetMonth)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<AgencyTargets>> GetTargetsByAgencyAsync(int AgencyId, int? year = null, int? month = null)
        {
            var query = _context.AgencyTargets
                .Include(dt => dt.Agency)
                .Where(dt => dt.AgencyId == AgencyId);
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

        public async Task<IEnumerable<AgencyTargets>> GetTargetsReportAsync(int? year = null, int? month = null)
        {
            var query = _context.AgencyTargets
                .Include(dt => dt.Agency)
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
            var target = await _context.AgencyTargets.FindAsync(targetId);
            if(target == null)
            {
                throw new KeyNotFoundException($"Agency target with ID {targetId} not found.");
            }
            target.AchievedSales += achievedSales;
            _context.AgencyTargets.Update(target);
            await _context.SaveChangesAsync();
        }
    }
}
