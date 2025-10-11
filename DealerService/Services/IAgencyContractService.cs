using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public interface IAgencyContractService
    {
        //tạo hợp đồng
        Task<AgencyContractResponse> CreateAgencyContractAsync(int AgencyId, CreateAgencyContractRequest requestt);
        Task<IEnumerable<AgencyContractResponse>> GetByAgencyIdAsync(int AgencyId);
        // Lấy hợp đồng còn hiệu lực
        Task<IEnumerable<AgencyContractResponse>> GetActiveByAgencyIdAsync(int AgencyId);

        // Lấy hợp đồng đã hết hạn
        Task<IEnumerable<AgencyContractResponse>> GetExpiredByAgencyIdAsync(int AgencyId);

        // Cập nhật trạng thái hợp đồng
        Task<AgencyContractResponse> UpdateStatusAsync(int contractId, UpdateStatusAgencyContractRequest request);

        // Gia hạn hợp đồng
        Task<AgencyContractResponse> RenewContractAsync(int contractId,RenewContractRequest request);

        // Chấm dứt hợp đồng
        Task<AgencyContractResponse> TerminateContractAsync(int contractId, TerminateContractRequest request);
        //search hợp đồng theo dk
        Task<IEnumerable<AgencyContractResponse>> SearchAsync(
            string? contractNumber = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null
        );
    }
}
