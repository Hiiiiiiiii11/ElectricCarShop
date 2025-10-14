using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public interface ITransactionRepository : IGenericRepository<Transaction>
    {
        Task<IEnumerable<Transaction>> GetByPaymentIdAsync(int paymentId);
        Task<Transaction?> GetByTransactionCodeAsync(string code);
        Task<IEnumerable<Transaction>> GetByStatusAsync(string status);
        Task<decimal> GetTotalAmountByPaymentAsync(int paymentId);
    }; 
}
