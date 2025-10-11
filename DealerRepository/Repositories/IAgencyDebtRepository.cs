using AgencyRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Repositories
{
    public interface IAgencyDebtRepository : IGenericRepository<AgencyDebts>
    {
        // Lấy công nợ theo AgencyId
        Task<IEnumerable<AgencyDebts>> GetAllDebtAgencyIdAsync(int AgencyId);
        Task<IEnumerable<AgencyDebts>> GetAgencysWithRemainingDebtByAgencyIdAsync(int AgencyId);
        Task<AgencyDebts?> GetByAgencyIdAsync(int AgencyId);

        // Cập nhật số tiền đã trả
        Task UpdatePaymentAsync(int agencyId, int contractId, decimal paidAmount);

        // Cập nhật nợ phát sinh thêm
        Task AddOrUpdateDebtAsync(int agencyId, int contractId, decimal newDebt, string note);

        // Xoá công nợ (nếu tất toán hoặc reset)
        Task ClearDebtAsync(int AgencyId);

        // Lấy danh sách đại lý còn nợ
        Task<IEnumerable<AgencyDebts>> GetAgencysWithRemainingDebtAsync();

        // Tìm kiếm công nợ theo khoảng thời gian
        Task<IEnumerable<AgencyDebts>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate);

        Task<IEnumerable<AgencyDebts>> GetDebtsByContractAsync(int contractId);
    }
}
