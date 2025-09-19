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
    public class DealerDebtRepository : GenericRepository<DealerDebts>, IDealerDebtRepository
    {
        private readonly DealerDbContext _context;
        public DealerDebtRepository(DealerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddDebtAsync(int dealerId, decimal newDebt)
        {

            // tao mới nếu chưa có công nợ
            var debt = await _context.DealerDebts.FirstOrDefaultAsync(d => d.DealerId == dealerId);
            if(debt == null)
            {
                debt = new DealerDebts
                {
                    DealerId = dealerId,
                    TotalDebt = newDebt,
                    PaidAmount = 0,
                    LastUpdate = DateTime.UtcNow,

                };
                await _context.DealerDebts.AddAsync(debt);
            }
            // thêm công nợ nếu đã có
            else
            {
                debt.TotalDebt += newDebt;
                debt.RemainingDebt = debt.TotalDebt - debt.PaidAmount;
                debt.LastUpdate = DateTime.UtcNow;
                _context.DealerDebts.Update(debt);
            }
            await _context.SaveChangesAsync();
        }

        public Task ClearDebtAsync(int dealerId)
        {
            var debt = _context.DealerDebts.FirstOrDefault(d => d.DealerId == dealerId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt record for DealerId {dealerId} not found.");
            _context.DealerDebts.Remove(debt);
            return _context.SaveChangesAsync();
        }

        public async Task<DealerDebts?> GetByDealerIdAsync(int dealerId)
        {
            return await _context.DealerDebts.Include(d => d.Dealer).
                FirstOrDefaultAsync(d => d.DealerId == dealerId);
        }

        public async Task<IEnumerable<DealerDebts>> GetDealersWithRemainingDebtAsync()
        {
                return await _context.DealerDebts.Include(d => d.Dealer).
                Where(d => d.RemainingDebt > 0).ToListAsync();
        }

        public async Task<IEnumerable<DealerDebts>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.DealerDebts.AsQueryable();
            if (fromDate.HasValue)
                query = query.Where(d => d.LastUpdate >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(d => d.LastUpdate <= toDate.Value);
            return await query.Include(d => d.Dealer).ToListAsync();

        }

        public Task UpdatePaymentAsync(int dealerId, decimal paidAmount)
        {
            var debt = _context.DealerDebts.FirstOrDefault(d => d.DealerId == dealerId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt record for DealerId {dealerId} not found.");
            debt.PaidAmount += paidAmount;
            debt.RemainingDebt = debt.TotalDebt - debt.PaidAmount;
            debt.LastUpdate = DateTime.UtcNow;
            _context.DealerDebts.Update(debt);
            return _context.SaveChangesAsync();
        }
    }
}
