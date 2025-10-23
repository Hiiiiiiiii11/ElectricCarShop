using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public interface IOrderRepository : IGenericRepository<Orders>
    {
        Task<IEnumerable<Orders>> GetByCustomerIdAsync(int customerId);
        //Task<Orders?> GetByQuotationIdAsync(int quotationId);
        Task<IEnumerable<Orders>> GetByStatusAsync(string status);
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
        // ✅ Mới: lấy Orders theo QuotationId qua OrderDetails
        Task<IEnumerable<Orders>> GetByQuotationIdAsync(int quotationId);

        // ✅ Mới: load đầy đủ 1 Order với Details + Quotations
        Task<Orders?> GetWithDetailsAsync(int orderId);
        Task<IEnumerable<Orders?>> GetAllWithDetailsAsync();

        // ✅ (Tùy chọn) doanh thu tính từ OrderDetails thay vì Orders.TotalAmount
        Task<decimal> GetTotalRevenueFromDetailsAsync(DateTime startDate, DateTime endDate);
        Task<IEnumerable<Orders>> GetByAgencyIdAsync(int agencyId);
    }
}
