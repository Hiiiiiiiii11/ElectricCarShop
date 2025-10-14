using Microsoft.EntityFrameworkCore;
using OrderRepository.Data;
using OrderRepository.Model;
using Share.ShareRepo;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public class TransactionRepository : GenericRepository<Transaction>, ITransactionRepository
    {
        private readonly OrderDbContext _context;

        public TransactionRepository(OrderDbContext context) : base(context)
        {
            _context = context;
        }

        // 🔹 Lấy tất cả giao dịch theo PaymentId
        public async Task<IEnumerable<Transaction>> GetByPaymentIdAsync(int paymentId)
        {
            return await _context.Transactions
                .Where(t => t.PaymentId == paymentId)
                .ToListAsync();
        }

        // 🔹 Lấy giao dịch theo mã
        public async Task<Transaction?> GetByTransactionCodeAsync(string code)
        {
            return await _context.Transactions
                .FirstOrDefaultAsync(t => t.TransactionCode == code);
        }

        // 🔹 Lọc theo trạng thái
        public async Task<IEnumerable<Transaction>> GetByStatusAsync(string status)
        {
            return await _context.Transactions
                .Where(t => t.Status == status)
                .ToListAsync();
        }

        // 🔹 Tính tổng số tiền theo PaymentId
        public async Task<decimal> GetTotalAmountByPaymentAsync(int paymentId)
        {
            return await _context.Transactions
                .Where(t => t.PaymentId == paymentId)
                .SumAsync(t => t.Amount);
        }
    }
}
