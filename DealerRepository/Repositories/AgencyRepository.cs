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
    public class AgencyRepository :GenericRepository<Agency>, IAgencyRepository
    {
        private readonly AgencyDbContext _context;
        public AgencyRepository(AgencyDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Agency> GetAgencyByNameAsync(string AgencyName)
        {
            return await _context.Agencys
                .FirstOrDefaultAsync(d => d.AgencyName == AgencyName);
        }

        public async Task<IEnumerable<Agency>> SearchAgencysAsync(string searchTerm)
        {
            return await _context.Agencys
                .Where(d => d.AgencyName.Contains(searchTerm) || d.Address.Contains(searchTerm))
                .ToListAsync()
                .ContinueWith(task => task.Result.AsEnumerable());
        }
    }
}
