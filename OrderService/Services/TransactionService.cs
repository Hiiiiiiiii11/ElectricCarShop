using OrderRepository.Model;
using OrderRepository.Model.Request;
using OrderRepository.Repositories;
using OrderService.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderAPIService.Services
{
    public class TransactionService : ITransactionService
    {
        private readonly ITransactionRepository _transactionRepository;

        public TransactionService(ITransactionRepository transactionRepository)
        {
            _transactionRepository = transactionRepository;
        }

        public async Task<IEnumerable<TransactionResponse>> GetByPaymentIdAsync(int paymentId)
        {
            var list = await _transactionRepository.GetByPaymentIdAsync(paymentId);
            return list.Select(MapToResponse);
        }

        public async Task<TransactionResponse?> GetByIdAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");
            return MapToResponse(transaction);
        }

        public async Task<TransactionResponse?> GetByTransactionCodeAsync(string code)
        {
            var transaction = await _transactionRepository.GetByTransactionCodeAsync(code);
            if (transaction == null)
                throw new KeyNotFoundException($"Transaction with code {code} not found.");
            return MapToResponse(transaction);
        }

        public async Task<TransactionResponse> CreateTransactionAsync(CreateTransactionRequest request)
        {
            var transaction = new Transaction
            {
                PaymentId = request.PaymentId,
                TransactionCode = request.TransactionCode,
                TransactionDate = request.TransactionDate,
                Amount = request.Amount,
                Status = request.Status
            };

            await _transactionRepository.AddAsync(transaction);
            return MapToResponse(transaction);
        }

        public async Task<TransactionResponse> UpdateTransactionAsync(int id, UpdateTransactionRequest request)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");

            transaction.TransactionCode = request.TransactionCode ?? transaction.TransactionCode;
            transaction.TransactionDate = request.TransactionDate ?? transaction.TransactionDate;
            transaction.Amount = request.Amount ?? transaction.Amount;
            transaction.Status = request.Status ?? transaction.Status;

             _transactionRepository.Update(transaction);
            await _transactionRepository.SaveChangesAsync();
            return MapToResponse(transaction);
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _transactionRepository.GetByIdAsync(id);
            if (transaction == null)
                throw new KeyNotFoundException($"Transaction with ID {id} not found.");

            _transactionRepository.Remove(transaction);
            await _transactionRepository.SaveChangesAsync();
            return true;
        }

        private TransactionResponse MapToResponse(Transaction t)
        {
            return new TransactionResponse
            {
                Id = t.Id,
                PaymentId = t.PaymentId,
                TransactionCode = t.TransactionCode,
                TransactionDate = t.TransactionDate,
                Amount = t.Amount,
                Status = t.Status
            };
        }

        public async Task<IEnumerable<TransactionResponse>> GetAllTransactionAsync()
        {
            var transactions = await _transactionRepository.GetAllAsync();
            return transactions.Select(MapToResponse);

        }
    }
}
