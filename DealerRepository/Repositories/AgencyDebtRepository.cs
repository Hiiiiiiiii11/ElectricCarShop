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

        public async Task<IEnumerable<AgencyDebts>> GetAllDebtAgencyIdAsync(int AgencyId)
        {
            return await _context.AgencyDebts
                .Include(d => d.Agency)
                .Include(d => d.AgencyContract)
                .Where(c => c.AgencyId == AgencyId).ToListAsync();
        }
        public async Task<IEnumerable<AgencyDebts>> GetAgencysWithRemainingDebtByAgencyIdAsync(int AgencyId)
        {

            return await _context.AgencyDebts
                 .Where(d => d.RemainingAmount > 0 && d.AgencyId == AgencyId)
                 .Include(d => d.Agency)
                 .Include(d => d.AgencyContract)
                 .OrderBy(d => d.DueDate)
                 .ToListAsync();

        }

        // Tạo mới hoặc cập nhật công nợ cho một hợp đồng
        public async Task AddOrUpdateDebtAsync(int agencyId, int contractId, decimal newDebt ,string note)
        {
            var debt = await _context.AgencyDebts
                .FirstOrDefaultAsync(d => d.AgencyId == agencyId && d.AgencyContractId == contractId);

            if (debt == null)
            {
                debt = new AgencyDebts
                {
                    AgencyId = agencyId,
                    AgencyContractId = contractId,
                    DebtAmount = newDebt,
                    PaidAmount = 0,
                    RemainingAmount = newDebt,
                    Notes = note,
                    CreateAt = DateTime.UtcNow,
                    UpdateAt = DateTime.UtcNow,
                    Status = "Unpaid"
                };
                await _context.AgencyDebts.AddAsync(debt);
            }
            else
            {
                debt.DebtAmount += newDebt;
                debt.RemainingAmount = debt.DebtAmount - debt.PaidAmount;
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
            return await _context.AgencyDebts
                .Include(d => d.Agency)
                .Include(d => d.AgencyContract)
                .FirstOrDefaultAsync(d => d.AgencyId == AgencyId);
        }

        public async Task<IEnumerable<AgencyDebts>> GetAgencysWithRemainingDebtAsync()
        {
                return await _context.AgencyDebts
                .Include(d => d.Agency)
                .Include(d => d.AgencyContract)
                .Where(d => d.RemainingAmount > 0).ToListAsync();
        }

        public async Task<IEnumerable<AgencyDebts>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var query = _context.AgencyDebts.AsQueryable();
            if (fromDate.HasValue)
                query = query.Where(d => d.CreateAt >= fromDate.Value);
            if (toDate.HasValue)
                query = query.Where(d => d.UpdateAt <= toDate.Value);
            return await query
                .Include(d => d.Agency)
                .Include(d => d.AgencyContract)
                .ToListAsync();

        }

        public async Task UpdatePaymentAsync(int agencyId, int contractId, decimal paidAmount)
        {
            var debt = await _context.AgencyDebts
                .FirstOrDefaultAsync(d => d.AgencyId == agencyId && d.AgencyContractId == contractId);

            if (debt == null)
                throw new KeyNotFoundException($"Debt not found for AgencyId {agencyId}, ContractId {contractId}");

            debt.PaidAmount += paidAmount;
            debt.RemainingAmount = debt.DebtAmount - debt.PaidAmount;
            debt.UpdateAt = DateTime.UtcNow;
            debt.Status = debt.RemainingAmount <= 0 ? "Paid" : "Partial";

            _context.AgencyDebts.Update(debt);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<AgencyDebts>> GetDebtsByContractAsync(int contractId)
        {
            return await _context.AgencyDebts
                .Include(d => d.Agency)
                .Include(d => d.AgencyContract)
                .Where(d => d.AgencyContractId == contractId)
                .ToListAsync();
        }
    }
}
