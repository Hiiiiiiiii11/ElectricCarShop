using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public interface IEVInventoryService
    {
        Task<IEnumerable<EVInventoryResponse>> GetAllInventoriesAsync();
        Task<EVInventoryResponse?> GetByVehicleIdAsync(int vehicleId);
        Task<EVInventoryResponse> CreateInventoryAsync(EVInventoryRequest request);
        Task DeleteInventoryAsync(int id);
        Task IncreaseInventoryAsync(int vehicleId, int quantity);
        Task DecreaseInventoryAsync(int vehicleId, int quantity);
        Task<bool> HasEnoughStockAsync(int vehicleId, int requiredQuantity);
        Task<int> GetTotalInventoryAsync();
    }
}
