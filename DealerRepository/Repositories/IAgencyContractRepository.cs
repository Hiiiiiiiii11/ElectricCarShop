using AgencyRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Repositories
{
    public interface IAgencyContractRepository : IGenericRepository<AgencyContracts>
    {
        Task<IEnumerable<AgencyContracts>> GetAllContractAgencyIdAsync(int AgencyId);
        Task<IEnumerable<AgencyContracts>> GetByAgencyIdAsync(int AgencyId);
        // Lấy hợp đồng còn hiệu lực
        Task<IEnumerable<AgencyContracts>> GetActiveByAgencyIdAsync(int AgencyId);

        // Lấy hợp đồng đã hết hạn
        Task<IEnumerable<AgencyContracts>> GetExpiredByAgencyIdAsync(int AgencyId);
        Task<AgencyContracts?> GetByContractNumberAsync(string contractNumber);

        // Cập nhật trạng thái hợp đồng
        Task UpdateStatusAsync(int contractId, string status);

        // Gia hạn hợp đồng
        Task RenewContractAsync(int contractId, DateTime newDate, string newTerms);

        // Chấm dứt hợp đồng
        Task TerminateContractAsync(int contractId, string reason);
        //search hợp đồng theo dk
        Task<IEnumerable<AgencyContracts>> SearchAsync(
            string? contractNumber = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null
        );
    }
}
