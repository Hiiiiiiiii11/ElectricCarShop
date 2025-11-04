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
    public class InstallmentPaymentsRepository : GenericRepository<InstallmentPayments>, IInstallmentPaymentsRepository
    {
        private readonly OrderDbContext _context;
        public InstallmentPaymentsRepository(OrderDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<List<InstallmentPayments>> GetByPlanIdAsync(int planId)
        {
            return await _context.InstallmentPayments
                .Where(p => p.InstallmentPlanId == planId)
                .OrderBy(p => p.PaidDate)
                .ToListAsync();
        }

        // ✅ Lấy danh sách payment theo item cụ thể
        public async Task<List<InstallmentPayments>> GetByItemIdAsync(int itemId)
        {
            return await _context.InstallmentPayments
                .Where(p => p.InstallmentItemId == itemId)
                .ToListAsync();
        }
    }
}
