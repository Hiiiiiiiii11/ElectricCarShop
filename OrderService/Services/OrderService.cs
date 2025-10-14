using OrderRepository.Model.Request;
using OrderRepository.Model;
using OrderRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;

        public OrderService(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            var order = new Orders
            {
                UserId = request.UserId,
                CustomerId = request.CustomerId,
                QuotationId = request.QuotationId,
                TotalAmount = request.TotalAmount,
                Status = request.Status,
                OrderDate = DateTime.UtcNow
            };

            await _orderRepository.AddAsync(order);
            return MapToResponse(order);
        }
   

        public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllAsync();
            return orders.Select(MapToResponse);
        }

        public async Task<OrderResponse?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            return order == null ? null : MapToResponse(order);
        }

       

        public async Task<OrderResponse> UpdateOrderAsync(int id, UpdateOrderRequest request)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {id} not found.");

            order.TotalAmount = request.TotalAmount;
            order.Status = request.Status;

            _orderRepository.Remove(order);
            await _orderRepository.SaveChangesAsync();

            return MapToResponse(order);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return false;

             _orderRepository.Remove(order);
            await _orderRepository.SaveChangesAsync();
            return true;
        }

        // 🔹 Custom methods
        public async Task<IEnumerable<OrderResponse>> GetOrdersByCustomerIdAsync(int customerId)
        {
            var orders = await _orderRepository.GetByCustomerIdAsync(customerId);
            return orders.Select(MapToResponse);
        }

        public async Task<OrderResponse?> GetOrderByQuotationIdAsync(int quotationId)
        {
            var order = await _orderRepository.GetByQuotationIdAsync(quotationId);
            return order == null ? null : MapToResponse(order);
        }

        public async Task<IEnumerable<OrderResponse>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _orderRepository.GetByStatusAsync(status);
            return orders.Select(MapToResponse);
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
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
                QuotationId = order.QuotationId,
                QuotationName = order.Quotation?.QuotationName,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                Status = order.Status
            };
        }
    }
}
