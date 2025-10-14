using Microsoft.EntityFrameworkCore;
using OrderRepository.Data;
using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public class OrderRepository : GenericRepository<Orders>, IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Orders>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Quotation)
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<Orders?> GetByQuotationIdAsync(int quotationId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Quotation)
                .FirstOrDefaultAsync(o => o.QuotationId == quotationId);
        }

        public async Task<IEnumerable<Orders>> GetByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Quotation)
                .Where(o => o.Status == status)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.Status == "Completed" &&
                            o.OrderDate >= startDate &&
                            o.OrderDate <= endDate)
                .SumAsync(o => o.TotalAmount);
        }
    }
}
