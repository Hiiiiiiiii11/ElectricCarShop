using AllocationRepository.Data;
using AllocationRepository.Model;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public class EVInventoryRepository :GenericRepository<EVInventory>, IEVInventoryRepository
    {
        private readonly AllocationDbContext _context;
        public EVInventoryRepository(AllocationDbContext context) : base(context)
        {
            _context = context;
        }

        //public async Task DecreaseQuantityAsync(int vehicleId, int quantity)
        //{
        //    var inventory = await GetByVehicleInstanceIdAsync(vehicleId);
        //    if (inventory != null)
        //    {
        //        inventory.Quantity -= quantity;
        //        await _context.SaveChangesAsync();
        //    }
        //}

        public async Task<IEnumerable<EVInventory>> GetAllWithVehiclesAsync()
        {
            return await _context.EVInventories
                .Include(e => e.VehicleInstance)
                .ToListAsync();
        }

        public async Task<EVInventory?> GetByVehicleInstanceIdAsync(int vehicleInstanceId)
        {
            return await _context.EVInventories
                .Include(e => e.VehicleInstance)
                .FirstOrDefaultAsync(e => e.VehicleInstanceId == vehicleInstanceId);
        }

        //public async Task<int> GetTotalInventoryCountAsync()
        //{
        //    return await _context.EVInventories.SumAsync(e => e.Quantity);
        //}

        //public async Task<bool> HasEnoughStockAsync(int vehicleId, int requiredQuantity)
        //{
        //    var inventory = await GetByVehicleInstanceIdAsync(vehicleId);
        //    return inventory != null && inventory.Quantity >= requiredQuantity;
        //}

        //public async Task IncreaseQuantityAsync(int vehicleId, int quantity)
        //{
        //    var inventory = await GetByVehicleInstanceIdAsync(vehicleId);
        //    if (inventory != null)
        //    {
        //        inventory.Quantity += quantity;
        //        await _context.SaveChangesAsync();
        //    }
        //}
    }
}
