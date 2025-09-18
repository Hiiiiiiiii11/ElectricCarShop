using DealerRepository.Data;
using DealerRepository.Model;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Repositories
{
    public class DealerDebtRepository : GenericRepository<DealerDebts>, IDealerDebtRepository
    {
        private readonly DealerDbContext _context;
        public DealerDebtRepository(DealerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddDebtAsync(int dealerId, decimal newDebt)
        {

            // tao mới nếu chưa có công nợ
            var debt = await _context.DealerDebts.FirstOrDefaultAsync(d => d.DealerId == dealerId);
            if(debt == null)
            {
                debt = new DealerDebts
                {
                    DealerId = dealerId,
                    TotalDebt = newDebt,
                    PaidAmount = 0,
                    LastUpdate = DateTime.UtcNow,

                };
                await _context.DealerDebts.AddAsync(debt);
            }
            // thêm công nợ nếu đã có
            else
            {
                debt.TotalDebt += newDebt;
                debt.RemainingDebt

            }
        }

        public Task ClearDebtAsync(int dealerId)
        {
            throw new NotImplementedException();
        }

        public Task<DealerDebts?> GetByDealerIdAsync(int dealerId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DealerDebts>> GetDealersWithRemainingDebtAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<DealerDebts>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate)
        {
            throw new NotImplementedException();
        }

        public Task UpdatePaymentAsync(int dealerId, decimal paidAmount)
        {
            throw new NotImplementedException();
        }
    }
}
