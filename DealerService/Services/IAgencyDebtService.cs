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
            Task<AgencyDebtResponse> AddDebtAsync(int AgencyId, AddAgencyDebtRequest request);
            Task<AgencyDebtResponse> MakePaymentAsync(int AgencyId, MakePaymentRequest request);
            Task<AgencyDebtResponse> ClearDebtAsync(int AgencyId);
            Task<IEnumerable<AgencyDebtResponse>> GetAllDebtsAsync();
            Task<IEnumerable<AgencyDebtResponse>> GetAgencysWithRemainingDebtAsync();
            Task<IEnumerable<AgencyDebtResponse>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate);
        }
}
