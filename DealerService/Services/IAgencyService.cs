using AgencyRepository.Model.DTO;
using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public interface IAgencyService
    {
        Task<AgencyResponse> CreateAgencyAsync(CreateAgencyRequest request);
        Task<AgencyResponse> UpdateAgencyAsync(int id, UpdateAgencyRequest request);
        Task<AgencyResponse> GetAgencyByIdAsync(int id);
        Task<IEnumerable<AgencyResponse>> GetAllAgencysAsync();
        Task<bool> DeleteAgencyAsync(int id);
        Task<IEnumerable<AgencyResponse>> SearchAgencysAsync(string searchTerm);
        Task<bool> AssignUserAsync( AssignUserAgencyRequest request, int agencyId);
        Task<bool> RemoveUserAsync( RemoveUserAgencyRequest request, int agencyId);
    }
}
