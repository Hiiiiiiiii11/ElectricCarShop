using AgencyRepository.Data;
using AgencyRepository.Model;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Repositories
{
    public class AgencyContractRepository : GenericRepository<AgencyContracts>, IAgencyContractRepository
    {
        private readonly AgencyDbContext _context;
        public AgencyContractRepository(AgencyDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<AgencyContracts>> GetAllContractAgencyIdAsync(int AgencyId)
        {
            return await _context.AgencyContracts
                .Where(c => c.AgencyId == AgencyId)
                .Include(c => c.Agency)
                .ToListAsync();
        }

        public async Task<IEnumerable<AgencyContracts>> GetActiveByAgencyIdAsync(int AgencyId)
        {
            return await _context.AgencyContracts
                .Where(c => c.AgencyId == AgencyId && c.Status == "Active")
                .Include(c => c.Agency)
                .ToListAsync();
        }

        public async Task<IEnumerable<AgencyContracts>> GetByAgencyIdAsync(int AgencyId)
        {
            return await _context.AgencyContracts
                .Where(c => c.AgencyId == AgencyId)
                .Include(c => c.Agency).ToListAsync();
        }

        public async Task<IEnumerable<AgencyContracts>> GetExpiredByAgencyIdAsync(int AgencyId)
        {
            return await _context.AgencyContracts
               .Where(c => c.AgencyId == AgencyId && c.Status == "Expired")
               .Include(c => c.Agency)
               .ToListAsync();
        }

        public async Task RenewContractAsync(int contractId, DateTime newDate, string newTerms)
        {
            var AgencyContract = await _context.AgencyContracts.FindAsync(contractId);
            if (AgencyContract == null)
                throw new KeyNotFoundException($"Contract with Id {contractId} not found.");
            AgencyContract.ContractDate = newDate;
            AgencyContract.Terms = newTerms;
            AgencyContract.Status = "Active";

            _context.AgencyContracts.Update(AgencyContract);
            await _context.SaveChangesAsync();

        }

        public async Task TerminateContractAsync(int contractId, string reason)
        {
            var contract = await _context.AgencyContracts.FindAsync(contractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with Id {contractId} not found.");

            contract.Status = "Terminated";
            contract.Terms += $"\nTerminated Reason: {reason}";

            _context.AgencyContracts.Update(contract);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int contractId, string status)
        {
            var AgencyContract = await _context.AgencyContracts.FindAsync(contractId);
            if (AgencyContract == null)
                throw new KeyNotFoundException($"Contract with Id {contractId} not found.");

            AgencyContract.Status = status;
            _context.AgencyContracts.Update(AgencyContract);
            await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<AgencyContracts>> SearchAsync(
              string? contractNumber = null,
              string? status = null,
              DateTime? startDate = null,
              DateTime? endDate = null)
        {
            var query = _context.AgencyContracts.AsQueryable();

            if (!string.IsNullOrEmpty(contractNumber))
                query = query.Where(c => c.ContractNumber.Contains(contractNumber));

            if (!string.IsNullOrEmpty(status))
                query = query.Where(c => c.Status == status);

            if (startDate.HasValue)
                query = query.Where(c => c.ContractDate >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.ContractDate <= endDate.Value);

            return await query
                .Include(c => c.Agency)
                .ToListAsync();
        }

        public Task<AgencyContracts?> GetByContractNumberAsync(string contractNumber)
        {
            return _context.AgencyContracts.Include(d => d.Agency)
                .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber);
        }
    }
}
