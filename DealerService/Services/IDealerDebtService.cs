using DealerRepository.Model;
using DealerRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerService.Services
{
        public interface IDealerDebtService
        {
            Task<DealerDebtResponse> GetDebtByDealerIdAsync(int dealerId);
            Task<DealerDebtResponse> AddDebtAsync(int dealerId, AddDealerDebtRequest request);
            Task<DealerDebtResponse> MakePaymentAsync(int dealerId, MakePaymentRequest request);
            Task<DealerDebtResponse> ClearDebtAsync(int dealerId);
            Task<IEnumerable<DealerDebtResponse>> GetAllDebtsAsync();
            Task<IEnumerable<DealerDebtResponse>> GetDealersWithRemainingDebtAsync();
            Task<IEnumerable<DealerDebtResponse>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate);
        }
}
