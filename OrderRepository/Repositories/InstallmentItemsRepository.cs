using Microsoft.EntityFrameworkCore;
using OrderRepository.Data;
using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public class InstallmentItemsRepository : GenericRepository<InstallmentItems>, IInstallmentItemsRepository
    {
        private readonly OrderDbContext _context;
        public InstallmentItemsRepository(OrderDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<InstallmentItems>> GetByPlanIdAsync(int planId)
        {
            return await _context.InstallmentItems
                .Where(i => i.InstallmentPlanId == planId)
                .OrderBy(i => i.InstallmentNo)
                .ToListAsync();
        }
    }
}
