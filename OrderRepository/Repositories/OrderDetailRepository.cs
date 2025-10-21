using OrderRepository.Data;
using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace OrderRepository.Repositories
{
    public class OrderDetailRepository : GenericRepository<OrderDetail>, IOrderDetailRepository
{
    private readonly OrderDbContext _context;

    public OrderDetailRepository(OrderDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrderDetail>> GetByOrderIdAsync(int orderId)
    {
        return await _context.OrderDetail
            .Include(od => od.Quotation)
            .Where(od => od.OrderId == orderId)
            .ToListAsync();
    }

    public async Task<IEnumerable<OrderDetail>> GetByQuotationIdAsync(int quotationId)
    {
        return await _context.OrderDetail
            .Include(od => od.Orders).ThenInclude(o => o.Customer)
            .Where(od => od.QuotationId == quotationId)
            .ToListAsync();
    }

    public async Task<OrderDetail?> GetByOrderAndQuotationAsync(int orderId, int quotationId)
    {
        return await _context.OrderDetail
            .Include(od => od.Orders)
            .Include(od => od.Quotation)
            .FirstOrDefaultAsync(od => od.OrderId == orderId && od.QuotationId == quotationId);
    }

    public async Task<decimal> GetTotalAmountByOrderAsync(int orderId)
    {
        return await _context.OrderDetail
            .Where(od => od.OrderId == orderId)
            .SumAsync(od => od.UnitPrice);
    }
}

}
