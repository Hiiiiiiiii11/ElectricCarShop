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
                .Include(o => o.Details)                 // ✅ collection trên Orders tên "Details"
                    .ThenInclude(od => od.Quotation)      // ✅ navigation trên OrderDetail tên "Quotation"
                .Where(o => o.CustomerId == customerId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Orders>> GetByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Details)                 // ✅
                    .ThenInclude(od => od.Quotation)     // ✅
                .Where(o => o.Status == status)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Where(o => o.Status == "Completed"
                            && o.OrderDate >= startDate
                            && o.OrderDate <= endDate)
                .SumAsync(o => o.TotalAmount);
        }

        // ✅ Lấy Orders theo Quotation qua OrderDetails (collection "Details")
        public async Task<IEnumerable<Orders>> GetByQuotationIdAsync(int quotationId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Details)                 // ✅
                    .ThenInclude(od => od.Quotation)     // ✅
                .Where(o => o.Details.Any(od => od.QuotationId == quotationId)) // ✅
                .ToListAsync();
        }

        // ✅ Load đầy đủ 1 Order với Details + Quotation
        public async Task<Orders?> GetWithDetailsAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Details)                 // ✅
                    .ThenInclude(od => od.Quotation)     // ✅
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }
        public async Task<IEnumerable<Orders?>> GetAllWithDetailsAsync()
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Details)                 // ✅
                .ThenInclude(od => od.Quotation).ToListAsync();    // ✅
        }

        // ✅ DbSet trong DbContext là "OrderDetail" (số ít) → giữ nguyên
        public async Task<decimal> GetTotalRevenueFromDetailsAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.OrderDetail
                .Where(od => od.Orders.Status == "Completed"   // ✅ navigation trên OrderDetail tên "Orders"
                             && od.Orders.OrderDate >= startDate
                             && od.Orders.OrderDate <= endDate)
                .SumAsync(od => od.UnitPrice);
        }
        public async Task<IEnumerable<Orders>> GetByAgencyIdAsync(int agencyId)
        {
            return await _context.Orders
                .Include(o => o.Customer)
                .Include(o => o.Details)
                    .ThenInclude(d => d.Quotation)
                .Where(o => o.Details.Any(d => d.Quotation.AgencyId == agencyId))
                .ToListAsync();
        }

    }
}
