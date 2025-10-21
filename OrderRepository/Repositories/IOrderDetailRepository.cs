using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public interface IOrderDetailRepository : IGenericRepository<OrderDetail>
    {
        Task<IEnumerable<OrderDetail>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<OrderDetail>> GetByQuotationIdAsync(int quotationId);
        Task<OrderDetail?> GetByOrderAndQuotationAsync(int orderId, int quotationId);
        Task<decimal> GetTotalAmountByOrderAsync(int orderId);
    }

}
