using OrderRepository.Model.Request;
using OrderRepository.Model;
using OrderRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailService _orderDetailService; // ✅ new

        public OrderService(IOrderRepository orderRepository,
                            IOrderDetailService orderDetailService) // ✅ new
        {
            _orderRepository = orderRepository;
            _orderDetailService = orderDetailService; // ✅ new
        }

        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            // 1) Tạo order với tổng tiền = 0; sẽ tính lại sau khi thêm chi tiết
            var order = new Orders
            {
                UserId = request.UserId,
                CustomerId = request.CustomerId,
                TotalAmount = 0m,                // ✅ set 0
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Draft" : request.Status,
                OrderDate = DateTime.UtcNow
            };

            await _orderRepository.AddAsync(order);
            await _orderRepository.SaveChangesAsync();   // ✅ để có Id

            // 2) Nếu request có chi tiết (QuotationId + UnitPrice), thêm vào và recalc
            if (request.Details != null && request.Details.Any())
            {
                foreach (var d in request.Details)
                {
                    // d.QuotationId là bắt buộc, d.UnitPrice có thể null -> service sẽ fallback QuotedPrice
                    await _orderDetailService.AddAsync(order.Id, d.QuotationId, d.UnitPrice);
                }

                order.TotalAmount = await _orderDetailService.RecalculateOrderTotalAsync(order.Id); // ✅
            }

            return MapToResponse(order);
        }

        public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToResponse);
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(int id)
        {
            // Nếu muốn kèm details, có thể dùng _orderRepository.GetWithDetailsAsync(id)
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found.");
            return MapToResponse(order);
        }

        public async Task<OrderResponse> UpdateOrderAsync(int id, UpdateOrderRequest request)
        {
            var order = await _orderRepository.GetByIdAsync(id)
                        ?? throw new KeyNotFoundException($"Order with ID {id} not found.");

            // Không cho sửa nếu Completed/Cancelled (tuỳ chính sách)
            if (string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(order.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Order is not editable.");

            // Cho phép cập nhật trạng thái; TotalAmount sẽ tính lại từ chi tiết
            if (!string.IsNullOrWhiteSpace(request.Status))
                order.Status = request.Status;

            // ❌ KHÔNG set order.TotalAmount từ request; ✅ tính lại từ chi tiết
            order.TotalAmount = await _orderDetailService.RecalculateOrderTotalAsync(order.Id);

            _orderRepository.Update(order);               // ✅ FIX: Update thay vì Remove
            await _orderRepository.SaveChangesAsync();    // ✅ nhớ SaveChanges

            return MapToResponse(order);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id)
                        ?? throw new KeyNotFoundException($"Order with ID {id} not found.");

            _orderRepository.Remove(order);
            await _orderRepository.SaveChangesAsync();
            return true;
        }

        // Custom methods
        public async Task<IEnumerable<OrderResponse>> GetOrdersByCustomerIdAsync(int customerId)
        {
            var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
            return orders.Select(MapToResponse);
        }

        public async Task<IEnumerable<OrderResponse>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _orderRepository.GetByStatusAsync(status);
            return orders.Select(MapToResponse);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            // Nếu muốn tính theo chi tiết: dùng GetTotalRevenueFromDetailsAsync
            return await _orderRepository.GetTotalRevenueAsync(startDate, endDate);
        }

        private static OrderResponse MapToResponse(Orders order)
        {
            return new OrderResponse
            {
                Id = order.Id,
                UserId = order.UserId,
                CustomerId = order.CustomerId,
                CustomerName = order.Customer?.FullName,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status
            };
        }
    }
}
