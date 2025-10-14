using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public interface IPaymentRepository : IGenericRepository<Payments>
    {
        Task<IEnumerable<Payments>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<Payments>> GetByStatusAsync(string status);
        Task<decimal> GetTotalPaidByOrderAsync(int orderId);
    }
}
