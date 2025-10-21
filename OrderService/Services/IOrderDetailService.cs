using OrderRepository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface IOrderDetailService
    {
        Task<OrderDetail> AddAsync(int orderId, int quotationId, decimal? unitPrice = null);
        Task<bool> UpdateUnitPriceAsync(int detailId, decimal newUnitPrice);
        Task<bool> RemoveAsync(int detailId);
        Task<IReadOnlyList<OrderDetail>> GetByOrderAsync(int orderId);
        Task<decimal> RecalculateOrderTotalAsync(int orderId);
    }

}
