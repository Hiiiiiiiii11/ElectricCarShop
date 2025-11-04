using Microsoft.EntityFrameworkCore;
using OrderRepository.Data;
using OrderRepository.Model;
using Share.ShareRepo;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public class PaymentRepository : GenericRepository<Payments>, IPaymentRepository
    {
        private readonly OrderDbContext _context;

        public PaymentRepository(OrderDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Payments>> GetByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderId == orderId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Payments>> GetByAgencyOrderIdAsync(int agencyOrderId)
        {
            return await _context.Payments
                .Where(p => p.AgencyOrderId == agencyOrderId)
                .ToListAsync();
        }
        public async Task<List<Payments>> GetCustomerPaymentsByAgencyIdAsync(int agencyId)
        {
            // Bước 1: Lấy tất cả ID báo giá (QuotationId) từ Agency
            var quotationIds = await _context.Quotations
                .Where(q => q.AgencyId == agencyId)
                .Select(q => q.Id)
                .ToListAsync();

            if (!quotationIds.Any())
            {
                return new List<Payments>(); // Không có báo giá, không có thanh toán
            }

            // Bước 2: Lấy tất cả ID đơn hàng (OrderId) từ các báo giá đó
            var orderIds = await _context.OrderDetail
                .Where(od => quotationIds.Contains(od.QuotationId))
                .Select(od => od.OrderId)
                .Distinct()
                .ToListAsync();

            if (!orderIds.Any())
            {
                return new List<Payments>(); // Không có đơn hàng, không có thanh toán
            }

            // Bước 3: Lấy tất cả Payments từ các ID đơn hàng đó
            var payments = await _context.Payments
                .Where(p => p.OrderId.HasValue && orderIds.Contains(p.OrderId.Value))
                .ToListAsync();

            return payments;
        }

        public async Task<IEnumerable<Payments>> GetByStatusAsync(string status)
        {
            return await _context.Payments
                .Where(p => p.Status == status)
                .ToListAsync();
        }
        public async Task<decimal> GetTotalPaidByOrderAsync(int orderId)
        {
            return await _context.Payments
                .Where(p => p.OrderId == orderId)
                .SumAsync(p => p.Amount);
        }
    }
}
