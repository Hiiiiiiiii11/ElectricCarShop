using DealerRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Repositories
{
    public interface IDealerDebtRepository : IGenericRepository<DealerDebts>
    {
        // Lấy công nợ theo DealerId
        Task<DealerDebts?> GetByDealerIdAsync(int dealerId);

        // Cập nhật số tiền đã trả
        Task UpdatePaymentAsync(int dealerId, decimal paidAmount);

        // Cập nhật nợ phát sinh thêm
        Task AddDebtAsync(int dealerId, decimal newDebt);

        // Xoá công nợ (nếu tất toán hoặc reset)
        Task ClearDebtAsync(int dealerId);

        // Lấy danh sách đại lý còn nợ
        Task<IEnumerable<DealerDebts>> GetDealersWithRemainingDebtAsync();

        // Tìm kiếm công nợ theo khoảng thời gian
        Task<IEnumerable<DealerDebts>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate);
    }
}
