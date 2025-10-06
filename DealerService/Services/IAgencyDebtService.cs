using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
        public interface IAgencyDebtService
        {
            Task<AgencyDebtResponse> GetDebtByAgencyIdAsync(int AgencyId);
            Task<AgencyDebtResponse> AddDebtAsync(int agencyId, int contractId, AddAgencyDebtRequest request);
            Task<AgencyDebtResponse> MakePaymentAsync(int agencyId, int contractId, MakePaymentRequest request);
            Task<AgencyDebtResponse> ClearDebtAsync(int AgencyId);
            Task<IEnumerable<AgencyDebtResponse>> GetAllDebtsAsync();
            Task<IEnumerable<AgencyDebtResponse>> GetAgencysWithRemainingDebtAsync();
            Task<IEnumerable<AgencyDebtResponse>> GetAllDebtsByAgencyIdAsync(int agencyId);
            Task<IEnumerable<AgencyDebtResponse>> GetAgencysWithRemainingDebtByAgencyIdAsync(int agencyId);
            Task<IEnumerable<AgencyDebtResponse>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate);
        }
}
