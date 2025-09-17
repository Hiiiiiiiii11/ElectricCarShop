using DealerRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerService.Services
{
    public interface IDealerService
    {
        Task<DealerResponse> CreateDealerAsync(CreateDealerRequest request);
        Task<DealerResponse> UpdateDealerAsync(int id, UpdateDealerRequest request);
        Task<DealerResponse> GetDealerByIdAsync(int id);
        Task<IEnumerable<DealerResponse>> GetAllDealersAsync();
        Task<bool> DeleteDealerAsync(int id);
        Task<IEnumerable<DealerResponse>> SearchDealersAsync(string searchTerm);
    }
}
