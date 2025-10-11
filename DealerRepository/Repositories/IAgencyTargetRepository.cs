using AgencyRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Repositories
{
    public interface IAgencyTargetRepository : IGenericRepository<AgencyTargets>
    {
        Task<AgencyTargets?> GetAgencyTargetsByAgencyId(int AgencyId);
        Task<AgencyTargets?> GetByAgencyAndPeriodAsync(int AgencyId, int year, int month);
        Task<IEnumerable<AgencyTargets>> GetTargetsByAgencyAsync(int AgencyId, int? year = null, int? month = null);
        Task UpdateAchievedSalesAsync(int targetId, int achievedSales);
        Task<IEnumerable<AgencyTargets>> GetTargetsReportAsync(int? year = null, int? month = null);
    }
}
