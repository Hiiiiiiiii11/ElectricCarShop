using AllocationRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using OrderRepository.Data;
using OrderRepository.Model;
using OrderRepository.Model.OrderDTO;
using OrderRepository.Repositories; // nơi có IOrderDetailRepository, IOrderRepository, IQuotationRepository
using OrderService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OrderDetailService : IOrderDetailService
{
    private readonly IOrderDetailRepository _orderDetailRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IQuotationRepository _quotationRepository;

    public OrderDetailService(IOrderDetailRepository orderDetailRepository, IOrderRepository orderRepository, IQuotationRepository quotationRepository)
    {
        _orderDetailRepository = orderDetailRepository;
        _orderRepository = orderRepository;
        _quotationRepository = quotationRepository;
    }

    public async Task<OrderDetailResponse> CreateOrderDetailAsync(CreateOrderDetailRequest request)
    {
        // 1. Kiểm tra Order cha
        var order = await _orderRepository.GetByIdAsync(request.OrderId);
        if (order == null)
        {
            throw new KeyNotFoundException($"Order with ID {request.OrderId} not found.");
        }

        // 2. Lấy giá
        decimal unitPrice;
        if (request.UnitPrice.HasValue)
        {
            unitPrice = request.UnitPrice.Value;
        }
        else
        {
            var quotation = await _quotationRepository.GetByIdAsync(request.QuotationId);
            if (quotation == null)
            {
                throw new KeyNotFoundException($"Quotation with ID {request.QuotationId} not found.");
            }
            unitPrice = quotation.QuotedPrice; // Giả định
        }

        // 3. Tạo và lưu OrderDetail
        var newDetail = new OrderDetail
        {
            OrderId = request.OrderId,
            QuotationId = request.QuotationId,
            UnitPrice = unitPrice
        };
        await _orderDetailRepository.AddAsync(newDetail);

        // ******** SỬA LỖI (FIX) ********
        // Phải lưu detail mới TRƯỚC KHI tính toán lại tổng tiền
        await _orderDetailRepository.SaveChangesAsync();

        // 4. Tính toán và cập nhật lại tổng tiền Order cha
        await RecalculateOrderTotalAsync(request.OrderId);

        // 5. Map và trả về
        return MapToResponse(newDetail);
    }

    /// <summary>
    /// Cập nhật giá của một Detail và cập nhật lại tổng tiền
    /// </summary>
    public async Task<OrderDetailResponse> UpdateOrderDetailPriceAsync(int orderDetailId, UpdateOrderDetailPriceRequest request)
    {
        // 1. Tìm detail
        var detail = await _orderDetailRepository.GetByIdAsync(orderDetailId);
        if (detail == null)
        {
            throw new KeyNotFoundException($"OrderDetail with ID {orderDetailId} not found.");
        }

        // 2. Cập nhật giá và lưu
        detail.UnitPrice = request.NewUnitPrice;
         _orderDetailRepository.Update(detail);

        // ******** SỬA LỖI (FIX) ********
        // Phải lưu thay đổi TRƯỚC KHI tính toán lại tổng tiền
        await _orderDetailRepository.SaveChangesAsync();

        // 3. Tính toán và cập nhật lại tổng tiền Order cha
        await RecalculateOrderTotalAsync(detail.OrderId);

        // 4. Map và trả về
        return MapToResponse(detail);
    }

    /// <summary>
    /// Xóa một Detail và cập nhật lại tổng tiền
    /// </summary>
    public async Task DeleteOrderDetailAsync(int orderDetailId)
    {
        // 1. Tìm detail
        var detail = await _orderDetailRepository.GetByIdAsync(orderDetailId);
        if (detail == null)
        {
           throw new KeyNotFoundException($"OrderDetail with ID {orderDetailId} not found.");
        }

        int orderId = detail.OrderId;

        // 2. Xóa detail
        _orderDetailRepository.Remove(detail);

        // ******** SỬA LỖI (FIX) ********
        // Phải lưu thay đổi (xóa) TRƯỚC KHI tính toán lại tổng tiền
        await _orderDetailRepository.SaveChangesAsync();

        // 3. Tính toán và cập nhật lại tổng tiền Order cha
        await RecalculateOrderTotalAsync(orderId);
    }

    /// <summary>
    /// Hàm helper private để đóng gói logic tính toán lại tổng tiền
    /// </summary>
    private async Task RecalculateOrderTotalAsync(int orderId)
    {
        // 1. Lấy tổng mới từ repo
        decimal newTotal = await _orderDetailRepository.GetTotalAmountByOrderAsync(orderId);

        // 2. Lấy Order cha
        var order = await _orderRepository.GetByIdAsync(orderId);

        // 3. Cập nhật và lưu
        if (order != null)
        {
            order.TotalAmount = newTotal;
            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();
        }
    }

    public async Task<OrderDetailResponse> GetOrderDetailByIdAsync(int orderDetailId)
    {
        var detail = await _orderDetailRepository.GetByIdAsync(orderDetailId);
        if (detail == null)
        {
            throw new KeyNotFoundException($"OrderDetail with ID {orderDetailId} not found.");
        }
        return MapToResponse(detail);
    }

    public async Task<IEnumerable<OrderDetailResponse>> GetOrderDetailsByOrderIdAsync(int orderId)
    {
        // Giả định bạn có hàm GetByOrderIdAsync trong repository
        var details = await _orderDetailRepository.GetByOrderIdAsync(orderId);
        return details.Select(MapToResponse);
    }
    private static OrderDetailResponse MapToResponse(OrderDetail detail)
    {
        if (detail == null) return null;

        return new OrderDetailResponse
        {
            Id = detail.Id,
            OrderId = detail.OrderId,
            QuotationId = detail.QuotationId,
            UnitPrice = detail.UnitPrice
        };
    }
}
