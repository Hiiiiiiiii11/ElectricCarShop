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
    }
}
