using DealerRepository.Model;
using DealerRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerService.Services
{
    public interface IDealerContractService
    {
        //tạo hợp đồng
        Task<DealerContractResponse> CreateDealerContractAsync(int dealerId, CreateDealerContractRequest requestt);
        Task<IEnumerable<DealerContractResponse>> GetByDealerIdAsync(int dealerId);
        // Lấy hợp đồng còn hiệu lực
        Task<IEnumerable<DealerContractResponse>> GetActiveByDealerIdAsync(int dealerId);

        // Lấy hợp đồng đã hết hạn
        Task<IEnumerable<DealerContractResponse>> GetExpiredByDealerIdAsync(int dealerId);

        // Cập nhật trạng thái hợp đồng
        Task<DealerContractResponse> UpdateStatusAsync(int contractId, UpdateStatusDealerContractRequest request);

        // Gia hạn hợp đồng
        Task<DealerContractResponse> RenewContractAsync(int contractId,RenewContractRequest request);

        // Chấm dứt hợp đồng
        Task<DealerContractResponse> TerminateContractAsync(int contractId, TerminateContractRequest request);
        //search hợp đồng theo dk
        Task<IEnumerable<DealerContractResponse>> SearchAsync(
            string? contractNumber = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null
        );
    }
}
