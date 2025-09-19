using DealerRepository.Data;
using DealerRepository.Model;
using DealerRepository.Model.DTO;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Repositories
{
    public class DealerContractRepository : GenericRepository<DealerContracts>, IDealerContractRepository
    {
        private readonly DealerDbContext _context;
        public DealerContractRepository(DealerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DealerContracts>> GetActiveByDealerIdAsync(int dealerId)
        {
            return await _context.DealerContracts
                .Where(c => c.DealerId == dealerId && c.Status == "Active")
                .Include(c => c.Dealer)
                .ToListAsync();
        }

        public async Task<IEnumerable<DealerContracts>> GetByDealerIdAsync(int dealerId)
        {
            return await _context.DealerContracts
                .Where(c => c.DealerId == dealerId)
                .Include(c => c.Dealer).ToListAsync();
        }

        public async Task<IEnumerable<DealerContracts>> GetExpiredByDealerIdAsync(int dealerId)
        {
            return await _context.DealerContracts
               .Where(c => c.DealerId == dealerId && c.Status == "Expired")
               .Include(c => c.Dealer)
               .ToListAsync();
        }

        public async Task RenewContractAsync(int contractId, DateTime newDate, string newTerms)
        {
            var dealerContract = await _context.DealerContracts.FindAsync(contractId);
            if (dealerContract == null)
                throw new KeyNotFoundException($"Contract with Id {contractId} not found.");
            dealerContract.ContractDate = newDate;
            dealerContract.Terms = newTerms;
            dealerContract.Status = "Active";

            _context.DealerContracts.Update(dealerContract);
            await _context.SaveChangesAsync();

        }

        public async Task TerminateContractAsync(int contractId, string reason)
        {
            var contract = await _context.DealerContracts.FindAsync(contractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with Id {contractId} not found.");

            contract.Status = "Terminated";
            contract.Terms += $"\nTerminated Reason: {reason}";

            _context.DealerContracts.Update(contract);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int contractId, string status)
        {
            var dealerContract = await _context.DealerContracts.FindAsync(contractId);
            if (dealerContract == null)
                throw new KeyNotFoundException($"Contract with Id {contractId} not found.");

            dealerContract.Status = status;
            _context.DealerContracts.Update(dealerContract);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<DealerContracts>> SearchAsync(
              string? contractNumber = null,
              string? status = null,
              DateTime? startDate = null,
              DateTime? endDate = null)
        {
            var query = _context.DealerContracts.AsQueryable();

            if (!string.IsNullOrEmpty(contractNumber))
                query = query.Where(c => c.ContractNumber.Contains(contractNumber));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            if (startDate.HasValue)
                query = query.Where(c => c.ContractDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.ContractDate <= endDate.Value);

            return await query
                .Include(c => c.Dealer)
                .ToListAsync();
        }

        public Task<DealerContracts?> GetByContractNumberAsync(string contractNumber)
        {
            return _context.DealerContracts.Include(d => d.Dealer)
                .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber);
        }
    }
}
