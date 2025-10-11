using AgencyRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public interface IAgencyTargetService
    {
        Task<AgencyTargetReportResponse> CreateTargetAsync(int AgencyId, CreateAgencyTargetRequest request);
        Task<IEnumerable<AgencyTargetReportResponse>> GetAgencyTargetAsync(int AgencyId, GetTargetReportRequest request);
        Task<AgencyTargetReportResponse> UpdateAchievedSalesAsync(int AgencyId,int targetId, UpdateAgencyTargetRequest request);
        Task<AgencyTargetReportResponse> GetCurrentTargetByAgencyIdAsync(int AgencyId);
        Task<IEnumerable<AgencyTargetReportResponse>> GetAllTargetsAsync(GetTargetReportRequest request);
        Task<IEnumerable<AgencyTargetReportResponse>> GetTargetsReportAsync(GetTargetReportRequest request);
        Task RemoveAgencyTarget(int AgencyId, int targetId);
    }
}
