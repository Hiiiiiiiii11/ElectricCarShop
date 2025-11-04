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
    public class InstallmentPlansRepository : GenericRepository<InstallmentPlans>, IInstallmentPlansRepository
    {
        private readonly OrderDbContext _context;
        public InstallmentPlansRepository(OrderDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<InstallmentPlans?> GetByContractIdAsync(int contractId)
        {
            return await _context.InstallmentPlans
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.ContractId == contractId);
        }

        public async Task<IEnumerable<InstallmentPlans>> GetByAgencyContractIdAsync(int agencyContractId)
        {
            return await _context.InstallmentPlans
                .Include(p => p.Items)
                .Where(p => p.AgencyContractId == agencyContractId)
                .ToListAsync();
        }

        public async Task<IEnumerable<InstallmentPlans>> GetAllWithItemsAsync()
        {
            return await _context.InstallmentPlans
                .Include(p => p.Items)
                .ToListAsync();
        }

        public async Task<InstallmentPlans?> GetWithItemsByIdAsync(int id)
        {
            return await _context.InstallmentPlans
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<InstallmentPlans?> GetPlanWithDetailsAsync(int id)
        {
            // Giả sử DbContext của bạn tên là _context
            return await _context.InstallmentPlans
                .Include(p => p.Payments) // Tải Payments của Plan
                .Include(p => p.Items)    // Tải Items của Plan
                    .ThenInclude(i => i.Payments) // Tải Payments của TỪNG Item
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        // Thêm phương thức này vào Repository
        public async Task<IEnumerable<InstallmentPlans>> GetAllPlansWithDetailsAsync()
        {
            return await _context.InstallmentPlans
                .Include(p => p.Payments) // Tải Payments của Plan
                .Include(p => p.Items)    // Tải Items của Plan
                    .ThenInclude(i => i.Payments) // Tải Payments của TỪNG Item
                .ToListAsync();
        }
    }
}
