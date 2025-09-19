using DealerRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Repositories
{
    public interface IDealerTargetRepository : IGenericRepository<DealerTargets>
    {
        Task<DealerTargets?> GetDealerTargetsByDealerId(int dealerId);
        Task<DealerTargets?> GetByDealerAndPeriodAsync(int dealerId, int year, int month);
        Task<IEnumerable<DealerTargets>> GetTargetsByDealerAsync(int dealerId, int? year = null, int? month = null);
        Task UpdateAchievedSalesAsync(int targetId, int achievedSales);
        Task<IEnumerable<DealerTargets>> GetTargetsReportAsync(int? year = null, int? month = null);
    }
}
