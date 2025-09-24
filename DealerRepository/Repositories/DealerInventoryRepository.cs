using DealerRepository.Data;
using DealerRepository.Model;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Repositories
{
    public class DealerInventoryRepository :GenericRepository<DealerInventory>, IDealerInventoryRepository
    {
        private readonly DealerDbContext _context;
        public DealerInventoryRepository(DealerDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DealerInventory>> GetInventoriesByDealerIdAsync(int dealerId)
        {
            return await _context.DealerInventories
                .Include(di => di.Dealer)
                .Where(di => di.DealerId == dealerId)
                .ToListAsync();
        }

        public async Task<DealerInventory?> GetInventoryAsync(int dealerId, int vehicleId)
        {
           return await _context.DealerInventories
                .Include(di => di.Dealer)
                .FirstOrDefaultAsync(di => di.DealerId == dealerId && di.VehicleId == vehicleId);
        }

        public async Task<bool> HasSufficientStockAsync(int dealerId, int vehicleId, int requiredQuantity)
        {
            var item = await GetInventoryAsync(dealerId, vehicleId);
            return item != null && item.Quantity >= requiredQuantity;
        }

        public async Task RemoveInventoryItemAsync(int dealerId, int variantId)
        {
            var inventory = await GetInventoryAsync(dealerId, variantId);
            if (inventory != null)
            {
                _context.DealerInventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetQuantityAsync(int dealerId, int vehicleId, int newQuantity)
        {
            var inventory = await GetInventoryAsync(dealerId, vehicleId);
            if (inventory == null)
            {
                throw new Exception("Inventory item not found.");
            }
            inventory.Quantity = newQuantity;
            Update(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateInventoryQuantityAsync(int dealerId, int vehicleId, int quantity)
        {
            var inventory = await GetInventoryAsync(dealerId, vehicleId);
            if (inventory == null)
            {
                throw new Exception("Inventory item not found.");
            }
            inventory.Quantity += quantity;
            if(inventory.Quantity < 0)
            {
                throw new Exception("Insufficient stock.");
                inventory.Quantity = 0;
            }
            Update(inventory);
            await _context.SaveChangesAsync();
        }
    }
}
