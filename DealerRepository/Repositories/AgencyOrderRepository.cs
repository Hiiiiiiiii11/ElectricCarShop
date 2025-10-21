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
    public class AgencyOrderRepository :GenericRepository<AgencyOrder>, IAgencyOrderRepository
    {
        private readonly AgencyDbContext _context;
        public AgencyOrderRepository(AgencyDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<AgencyOrder>> GetByAgencyIdAsync(int agencyId)
        {
            return await _context.AgencyOrders
                .Where(o => o.AgencyId == agencyId)
                .Include(o => o.AgencyContracts)
                .ToListAsync();
        }

        public async Task<IEnumerable<AgencyOrder>> GetByContractIdAsync(int contractId)
        {
            return await _context.AgencyOrders
                .Where(o => o.AgencyContractId == contractId)
                .ToListAsync();
        }
    }
}
