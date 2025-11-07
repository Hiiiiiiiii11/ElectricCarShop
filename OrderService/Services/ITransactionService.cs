using OrderRepository.Model.OrderDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionResponse>> GetByPaymentIdAsync(int paymentId);
        Task<IEnumerable<TransactionResponse>> GetAllTransactionAsync();
        Task<TransactionResponse?> GetByIdAsync(int id);
        Task<TransactionResponse?> GetByTransactionCodeAsync(string code);
        Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request);
        Task<TransactionResponse> UpdateTransactionAsync(int id, UpdateTransactionRequest request);
        Task<bool> DeleteTransactionAsync(int id);
    }
}
