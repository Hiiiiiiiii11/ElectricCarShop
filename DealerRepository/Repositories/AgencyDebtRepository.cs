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
    public class AgencyDebtRepository : GenericRepository<AgencyDebts>, IAgencyDebtRepository
    {
        private readonly AgencyDbContext _context;
        public AgencyDebtRepository(AgencyDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddDebtAsync(int AgencyId, decimal newDebt)
        {

            // tao mới nếu chưa có công nợ
            var debt = await _context.AgencyDebts.FirstOrDefaultAsync(d => d.AgencyId == AgencyId);
            if(debt == null)
            {
                debt = new AgencyDebts
                {
                    AgencyId = AgencyId,
                    DebtAmount = newDebt,
                    PaidAmount = 0,
                    RemainingAmount = 0,
                    CreateAt = DateTime.Now,
                    UpdateAt = DateTime.UtcNow,

                };
                await _context.AgencyDebts.AddAsync(debt);
            }
            // thêm công nợ nếu đã có
            else
            {
                debt.DebtAmount += newDebt;
                debt.RemainingAmount = debt.RemainingAmount - debt.PaidAmount;
                debt.UpdateAt = DateTime.UtcNow;
                _context.AgencyDebts.Update(debt);
            }
            await _context.SaveChangesAsync();
        }

        public Task ClearDebtAsync(int AgencyId)
        {
            var debt = _context.AgencyDebts.FirstOrDefault(d => d.AgencyId == AgencyId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt record for AgencyId {AgencyId} not found.");
            _context.AgencyDebts.Remove(debt);
            return _context.SaveChangesAsync();
        }

        public async Task<AgencyDebts?> GetByAgencyIdAsync(int AgencyId)
        {
            return await _context.AgencyDebts.Include(d => d.Agency).
                FirstOrDefaultAsync(d => d.AgencyId == AgencyId);
        }

        public async Task<IEnumerable<AgencyDebts>> GetAgencysWithRemainingDebtAsync()
        {
                return await _context.AgencyDebts.Include(d => d.Agency).
                Where(d => d.RemainingAmount > 0).ToListAsync();
        }

        public async Task<IEnumerable<AgencyDebts>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.AgencyDebts.AsQueryable();
            if (fromDate.HasValue)
                query = query.Where(d => d.CreateAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(d => d.UpdateAt <= toDate.Value);
            return await query.Include(d => d.Agency).ToListAsync();

        }

        public Task UpdatePaymentAsync(int AgencyId, decimal paidAmount)
        {
            var debt = _context.AgencyDebts.FirstOrDefault(d => d.AgencyId == AgencyId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt record for AgencyId {AgencyId} not found.");
            debt.PaidAmount += paidAmount;
            debt.RemainingAmount = debt.DebtAmount - debt.PaidAmount;
            debt.UpdateAt = DateTime.UtcNow;
            _context.AgencyDebts.Update(debt);
            return _context.SaveChangesAsync();
        }
    }
}
