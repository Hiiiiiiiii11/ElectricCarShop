using AgencyRepository.Data;
using AgencyRepository.Model;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Repositories
{
    public class AgencyInventoryRepository :GenericRepository<AgencyInventory>, IAgencyInventoryRepository
    {
        private readonly AgencyDbContext _context;
        public AgencyInventoryRepository(AgencyDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<AgencyInventory>> GetInventoriesByAgencyIdAsync(int AgencyId)
        {
            return await _context.AgencyInventories
                .Include(di => di.Agency)
                .Where(di => di.AgencyId == AgencyId)
                .ToListAsync();
        }

        public async Task<AgencyInventory?> GetInventoryAsync(int AgencyId, int vehicleId)
        {
           return await _context.AgencyInventories
                .Include(di => di.Agency)
                .FirstOrDefaultAsync(di => di.AgencyId == AgencyId && di.VehicleId == vehicleId);
        }

        public async Task<bool> HasSufficientStockAsync(int AgencyId, int vehicleId, int requiredQuantity)
        {
            var item = await GetInventoryAsync(AgencyId, vehicleId);
            return item != null && item.Quantity >= requiredQuantity;
        }

        public async Task RemoveInventoryItemAsync(int AgencyId, int variantId)
        {
            var inventory = await GetInventoryAsync(AgencyId, variantId);
            if (inventory != null)
            {
                _context.AgencyInventories.Remove(inventory);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SetQuantityAsync(int AgencyId, int vehicleId, int newQuantity)
        {
            var inventory = await GetInventoryAsync(AgencyId, vehicleId);
            if (inventory == null)
            {
                throw new Exception("Inventory item not found.");
            }
            inventory.Quantity = newQuantity;
            Update(inventory);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateInventoryQuantityAsync(int AgencyId, int vehicleId, int quantity)
        {
            var inventory = await GetInventoryAsync(AgencyId, vehicleId);
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
