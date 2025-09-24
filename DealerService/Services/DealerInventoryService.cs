using DealerRepository.Model;
using DealerRepository.Model.DTO;
using DealerRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerService.Services
{
    public class DealerInventoryService : IDealerInventoryService
    {
        private readonly IDealerInventoryRepository _dealerInventoryRepository;
        public DealerInventoryService(IDealerInventoryRepository dealerInventoryRepository)
        {
            _dealerInventoryRepository = dealerInventoryRepository;
        }

        public async Task<DealerInventoryResponse> CreateDealerInventoryAsync(int dealerId, CreateDealerInventoryRequest request)
        {
            var existingInventory = _dealerInventoryRepository.GetInventoryAsync(dealerId, request.VehicleId);
            if (existingInventory != null)
            {
                throw new Exception("Inventory item already exists for this variant in the dealer.");
            }
            var newInventory = new DealerInventory
            {
                DealerId = dealerId,
                VehicleId = request.VehicleId,
                Quantity = request.Quantity
            };
            await _dealerInventoryRepository.AddAsync(newInventory);
            await _dealerInventoryRepository.SaveChangesAsync();
            return MapToResponse(newInventory);
        }

        public async Task<IEnumerable<DealerInventoryResponse>> GetInventoriesByDealerIdAsync(int dealerId)
        {
            var inventories = await _dealerInventoryRepository.GetInventoriesByDealerIdAsync(dealerId);
            return inventories.Select(MapToResponse);
        }

        public async Task<DealerInventoryResponse?> GetInventoryAsync(int dealerId, int variantId)
        {
            var inventory = await _dealerInventoryRepository.GetInventoryAsync(dealerId, variantId);
            return inventory == null ? null : MapToResponse(inventory);
        }

        public Task<bool> HasSufficientStockAsync(int dealerId, int variantId, int requiredQuantity)
        {
            return _dealerInventoryRepository.HasSufficientStockAsync(dealerId, variantId, requiredQuantity);
        }

        public Task RemoveInventoryItemAsync(int dealerId, int variantId)
        {
            var inventory = _dealerInventoryRepository.GetInventoryAsync(dealerId, variantId);
            if (inventory == null)
            {
                throw new Exception("Inventory item not found.");
            }
            return _dealerInventoryRepository.RemoveInventoryItemAsync(dealerId, variantId);
        }

        public Task SetQuantityAsync(int dealerId, int variantId, int newQuantity)
        {
            var inventory = _dealerInventoryRepository.GetInventoryAsync(dealerId, variantId);
            if (inventory == null)
            {
                throw new Exception("Inventory item not found.");
            }
            return _dealerInventoryRepository.SetQuantityAsync(dealerId, variantId, newQuantity);
        }

        public async Task UpdateInventoryQuantityAsync(int dealerId, UpdateDealerInventoryRequest request)
        {
            var inventory = await _dealerInventoryRepository.GetInventoryAsync(dealerId, request.VehicleId);
            if (inventory == null)
            {
                throw new Exception("Inventory item not found.");
            }
             await _dealerInventoryRepository.UpdateInventoryQuantityAsync(dealerId, request.VehicleId, request.Quantity);
        }
        public DealerInventoryResponse MapToResponse(DealerInventory inventory)
        {
            return new DealerInventoryResponse
            {
                Id = inventory.Id,
                DealerId = inventory.DealerId,
                VehicleId = inventory.VehicleId,
                Quantity = inventory.Quantity,
                Dealer = inventory.Dealer == null ? null : new DealerResponse
                {
                    Id = inventory.Dealer.Id,
                    DealerName = inventory.Dealer.DealerName,
                    Email = inventory.Dealer.Email,
                    Phone = inventory.Dealer.Phone,
                    Address = inventory.Dealer.Address,
                    Status = inventory.Dealer.Status,
                    Created_At = inventory.Dealer.Created_At,
                    Updated_At = inventory.Dealer.Updated_At
                }
            };
        }
    }
}
