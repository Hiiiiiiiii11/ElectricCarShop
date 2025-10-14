using OrderRepository.Model.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponse>> GetAllOrdersAsync();
        Task<OrderResponse?> GetOrderByIdAsync(int id);
        Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
        Task<OrderResponse> UpdateOrderAsync(int id, UpdateOrderRequest request);
        Task<bool> DeleteOrderAsync(int id);

        // 4 hàm mở rộng
        Task<IEnumerable<OrderResponse>> GetOrdersByCustomerIdAsync(int customerId);
        Task<OrderResponse?> GetOrderByQuotationIdAsync(int quotationId);
        Task<IEnumerable<OrderResponse>> GetOrdersByStatusAsync(string status);
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);
    }
}
