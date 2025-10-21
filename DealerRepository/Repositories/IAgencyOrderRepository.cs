using AgencyRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Repositories
{
    public interface IAgencyOrderRepository : IGenericRepository<AgencyOrder>
    {
        Task<IEnumerable<AgencyOrder>> GetByAgencyIdAsync(int agencyId);
        Task<IEnumerable<AgencyOrder>> GetByContractIdAsync(int contractId);
    }
}
