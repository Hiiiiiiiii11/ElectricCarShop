using AgencyRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Repositories
{
    public interface IAgencyRepository : IGenericRepository<Agency>
    {
        Task<Agency> GetAgencyByNameAsync(string AgencyName);
        Task<IEnumerable<Agency>> SearchAgencysAsync(string searchTerm);

    }
}
