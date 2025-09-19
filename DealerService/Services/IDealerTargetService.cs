using DealerRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerService.Services
{
    public interface IDealerTargetService
    {
        Task<DealerTargetReportResponse> CreateTargetAsync(int dealerId, CreateDealerTargetRequest request);
        Task<IEnumerable<DealerTargetReportResponse>> GetDealerTargetAsync(int dealerId, GetTargetReportRequest request);
        Task<DealerTargetReportResponse> UpdateAchievedSalesAsync(int dealerId,int targetId, UpdateDealerTargetRequest request);
        Task<DealerTargetReportResponse> GetCurrentTargetByDealerIdAsync(int dealerId);
        Task<IEnumerable<DealerTargetReportResponse>> GetAllTargetsAsync(GetTargetReportRequest request);
        Task<IEnumerable<DealerTargetReportResponse>> GetTargetsReportAsync(GetTargetReportRequest request);
        Task RemoveDealerTarget(int dealerId, int targetId);
    }
}
