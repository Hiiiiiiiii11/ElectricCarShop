using DealerRepository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerService.Services
{
    public interface IDealerDebtService
    {
        public interface IDealerDebtService
        {
            Task<DealerDebts> GetDebtByDealerIdAsync(int dealerId);
            Task<DealerDebts> AddDebtAsync(int dealerId, decimal amount);
            Task<DealerDebts> MakePaymentAsync(int dealerId, decimal amount);
            Task<DealerDebts> ClearDebtAsync(int dealerId);
            Task<IEnumerable<DealerDebts>> GetAllDebtsAsync();
            Task<IEnumerable<DealerDebts>> GetDealersWithRemainingDebtAsync();
            Task<IEnumerable<DealerDebts>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate);
        }

    }
}
