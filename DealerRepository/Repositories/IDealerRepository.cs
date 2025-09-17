using DealerRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Repositories
{
    public interface IDealerRepository : IGenericRepository<Dealers>
    {
        Task<Dealers> GetDealerByNameAsync(string dealerName);
        Task<IEnumerable<Dealers>> SearchDealersAsync(string searchTerm);

    }
}
