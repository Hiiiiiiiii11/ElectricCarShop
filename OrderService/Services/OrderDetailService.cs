using AllocationRepository.Repositories;
using Microsoft.EntityFrameworkCore;
using OrderRepository.Data;
using OrderRepository.Model;
using OrderRepository.Repositories; // nơi có IOrderDetailRepository, IOrderRepository, IQuotationRepository
using OrderService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class OrderDetailService : IOrderDetailService
{
    private readonly OrderDbContext _context;
    private readonly IOrderDetailRepository _orderDetailRepo;
    private readonly IOrderRepository _orderRepo;
    private readonly IQuotationRepository _quotationRepo;

    public OrderDetailService(
        OrderDbContext context,
        IOrderDetailRepository orderDetailRepo,
        IOrderRepository orderRepo,
        IQuotationRepository quotationRepo)
    {
        _context = context;
        _orderDetailRepo = orderDetailRepo;
        _orderRepo = orderRepo;
        _quotationRepo = quotationRepo;
    }

    public async Task<OrderDetail> AddAsync(int orderId, int quotationId, decimal? unitPrice = null)
    {
        var order = await _orderRepo.GetByIdAsync(orderId)
                    ?? throw new KeyNotFoundException("Order not found.");
        if (string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(order.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Order is not editable.");

        var quotation = await _quotationRepo.GetByIdAsync(quotationId)
                        ?? throw new KeyNotFoundException("Quotation not found.");

        // Chống trùng (OrderId, QuotationId)
        var existed = await _orderDetailRepo.GetByOrderAndQuotationAsync(orderId, quotationId);
        if (existed != null) return existed;

        // ==== DateTime (không nullable) check ====
        // Nếu cột DB cho phép null nhưng entity đang là DateTime (non-nullable),
        // EF sẽ set default(DateTime) khi null => kiểm tra '!= default'
        var orderDate = order.OrderDate.Date;

        if (quotation.StartDate != default && orderDate < quotation.StartDate.Date)
            throw new InvalidOperationException("Quotation is not effective for this order date.");

        if (quotation.EndDate != default && orderDate > quotation.EndDate.Date)
            throw new InvalidOperationException("Quotation expired for this order date.");
        // ==========================================

        var price = unitPrice ?? quotation.QuotedPrice; // đổi tên nếu property khác
        var detail = new OrderDetail
        {
            OrderId = orderId,
            QuotationId = quotationId,
            UnitPrice = price
        };

        await using var tx = await _context.Database.BeginTransactionAsync();
        await _orderDetailRepo.AddAsync(detail);
        await _context.SaveChangesAsync();

        await RecalculateOrderTotalAsync(orderId);

        await tx.CommitAsync();
        return detail;
    }

    public async Task<bool> UpdateUnitPriceAsync(int detailId, decimal newUnitPrice)
    {
        var detail = await _orderDetailRepo.GetByIdAsync(detailId)
                     ?? throw new KeyNotFoundException("OrderDetail not found.");
        var order = await _orderRepo.GetByIdAsync(detail.OrderId)
                    ?? throw new KeyNotFoundException("Order not found.");

        if (string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(order.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Order is not editable.");

        await using var tx = await _context.Database.BeginTransactionAsync();

        detail.UnitPrice = newUnitPrice;
        _orderDetailRepo.Update(detail);
        await _context.SaveChangesAsync();

        await RecalculateOrderTotalAsync(detail.OrderId);

        await tx.CommitAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(int detailId)
    {
        var detail = await _orderDetailRepo.GetByIdAsync(detailId)
                     ?? throw new KeyNotFoundException("OrderDetail not found.");
        var order = await _orderRepo.GetByIdAsync(detail.OrderId)
                    ?? throw new KeyNotFoundException("Order not found.");

        if (string.Equals(order.Status, "Completed", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(order.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Order is not editable.");

        await using var tx = await _context.Database.BeginTransactionAsync();

        // IOrderDetailRepository không có Delete(...) → dùng DbContext trực tiếp
        _context.OrderDetail.Remove(detail);
        await _context.SaveChangesAsync();

        await RecalculateOrderTotalAsync(detail.OrderId);

        await tx.CommitAsync();
        return true;
    }

    public async Task<IReadOnlyList<OrderDetail>> GetByOrderAsync(int orderId)
    {
        var list = await _orderDetailRepo.GetByOrderIdAsync(orderId);
        return list.ToList();
    }

    public async Task<decimal> RecalculateOrderTotalAsync(int orderId)
    {
        var total = await _orderDetailRepo.GetTotalAmountByOrderAsync(orderId);

        var order = await _context.Orders.FindAsync(orderId)
                    ?? throw new KeyNotFoundException("Order not found.");
        order.TotalAmount = total;

        await _context.SaveChangesAsync();
        return total;
    }
}
