using Microsoft.EntityFrameworkCore;
using OrderRepository.Data;
using OrderRepository.Model;
using Share.ShareRepo;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public class ContractRepository : GenericRepository<Contracts>, IContractRepository
    {
        private readonly OrderDbContext _context;

        public ContractRepository(OrderDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Contracts>> GetByQuotationIdAsync(int quotationId)
        {
            return await _context.Contracts
                .Where(c => c.QuotationId == quotationId)
                .ToListAsync();
        }

        public async Task<Contracts?> GetByContractNumberAsync(string contractNumber)
        {
            return await _context.Contracts
                .FirstOrDefaultAsync(c => c.ContractNumber == contractNumber);
        }
        public async Task<IEnumerable<Contracts>> GetContractsByAgencyIdAsync(int agencyId)
        {
            return await _context.Contracts
                .Include(c => c.Quotation)
                .Where(c => c.Quotation.AgencyId == agencyId)
                .ToListAsync();
        }
    }
}
