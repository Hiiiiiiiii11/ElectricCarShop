using DealerRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerService.Services
{
    public interface IDealerUserService
    {
        Task<DealerUserResponse> GetDealerUserByIdAsync(int userId);
        Task<DealerUserResponse> CreateDealerUserAsync(CreateDealerUserRequest request);
        Task RemoveDealerUserAsync(int userId, int dealerId);
        Task<IEnumerable<DealerUserResponse>> GetDealerUsersByDealerIdAsync(int dealerId);


    }
}
