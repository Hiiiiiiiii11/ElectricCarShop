
using AllocationRepository.Model;
using OrderRepository.Data;
using Share.ShareRepo;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public class QuotationRepository : GenericRepository<Quotations>, IQuotationRepository
    {
        private readonly OrderDbContext _context;   
        public QuotationRepository(OrderDbContext context) : base(context)
        {
            _context= context;
        }

        public async Task<IEnumerable<Quotations>> GetQuotationByUserCreateId(int userId)
        {
            return await _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.Contracts)
                .Include(q => q.OrderDetails)
                .Where(q => q.CreateBy == userId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }
        public async Task<IEnumerable<Quotations>> GetQuotationByAgencyId(int agencyId)
        {
            return await _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.Contracts)
                .Include(q => q.OrderDetails)
                .Where(q => q.AgencyId == agencyId)
                .OrderByDescending(q => q.CreatedAt)
                .ToListAsync();
        }
    }
}
