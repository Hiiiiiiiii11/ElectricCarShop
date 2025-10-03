using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public class AgencyInventoryService : IAgencyInventoryService
    {
        private readonly IAgencyInventoryRepository _AgencyInventoryRepository;
        public AgencyInventoryService(IAgencyInventoryRepository AgencyInventoryRepository)
        {
            _AgencyInventoryRepository = AgencyInventoryRepository;
        }

        public async Task<AgencyInventoryResponse> CreateAgencyInventoryAsync(int AgencyId, CreateAgencyInventoryRequest request)
        {
            var existingInventory = _AgencyInventoryRepository.GetInventoryAsync(AgencyId, request.VehicleId);
            if (existingInventory != null)
            {
                throw new Exception("Inventory item already exists for this variant in the Agency.");
            }
            var newInventory = new AgencyInventory
            {
                AgencyId = AgencyId,
                VehicleId = request.VehicleId,
                Quantity = request.Quantity
            };
            await _AgencyInventoryRepository.AddAsync(newInventory);
            await _AgencyInventoryRepository.SaveChangesAsync();
            return MapToResponse(newInventory);
        }

        public async Task<IEnumerable<AgencyInventoryResponse>> GetInventoriesByAgencyIdAsync(int AgencyId)
        {
            var inventories = await _AgencyInventoryRepository.GetInventoriesByAgencyIdAsync(AgencyId);
            return inventories.Select(MapToResponse);
        }

        public async Task<AgencyInventoryResponse?> GetInventoryAsync(int AgencyId, int variantId)
        {
            var inventory = await _AgencyInventoryRepository.GetInventoryAsync(AgencyId, variantId);
            return inventory == null ? null : MapToResponse(inventory);
        }

        public Task<bool> HasSufficientStockAsync(int AgencyId, int variantId, int requiredQuantity)
        {
            return _AgencyInventoryRepository.HasSufficientStockAsync(AgencyId, variantId, requiredQuantity);
        }

        public Task RemoveInventoryItemAsync(int AgencyId, int variantId)
        {
            var inventory = _AgencyInventoryRepository.GetInventoryAsync(AgencyId, variantId);
            if (inventory == null)
            {
                throw new Exception("Inventory item not found.");
            }
            return _AgencyInventoryRepository.RemoveInventoryItemAsync(AgencyId, variantId);
        }

        public Task SetQuantityAsync(int AgencyId, int variantId, int newQuantity)
        {
            var inventory = _AgencyInventoryRepository.GetInventoryAsync(AgencyId, variantId);
            if (inventory == null)
            {
                throw new Exception("Inventory item not found.");
            }
            return _AgencyInventoryRepository.SetQuantityAsync(AgencyId, variantId, newQuantity);
        }

        public async Task UpdateInventoryQuantityAsync(int AgencyId, UpdateAgencyInventoryRequest request)
        {
            var inventory = await _AgencyInventoryRepository.GetInventoryAsync(AgencyId, request.VehicleId);
            if (inventory == null)
            {
                throw new Exception("Inventory item not found.");
            }
             await _AgencyInventoryRepository.UpdateInventoryQuantityAsync(AgencyId, request.VehicleId, request.Quantity);
        }
        public AgencyInventoryResponse MapToResponse(AgencyInventory inventory)
        {
            return new AgencyInventoryResponse
            {
                Id = inventory.Id,
                AgencyId = inventory.AgencyId,
                VehicleId = inventory.VehicleId,
                Quantity = inventory.Quantity,
                Agency = inventory.Agency == null ? null : new AgencyResponse
                {
                    Id = inventory.Agency.Id,
                    AgencyName = inventory.Agency.AgencyName,
                    Email = inventory.Agency.Email,
                    Phone = inventory.Agency.Phone,
                    Address = inventory.Agency.Address,
                    Status = inventory.Agency.Status,
                    Created_At = inventory.Agency.Created_At,
                    Updated_At = inventory.Agency.Updated_At
                }
            };
        }
    }
}
