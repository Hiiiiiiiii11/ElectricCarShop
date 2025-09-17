using DealerRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Repositories
{
    public interface IDealerUserRepository : IGenericRepository<DealerUser>
    {
        Task<DealerUser> GetDealerUserByUserId(int userId);
        Task<IEnumerable<DealerUser>> GetUsersByDealerIdAsync(int dealerId);
        Task<DealerUser?> GetByUserAndDealerAsync(int userId, int dealerId);

        Task<IEnumerable<DealerUser>> SearchDealerUsersAsync(string searchTerm);
    }
}
