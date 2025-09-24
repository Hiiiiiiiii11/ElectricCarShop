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
    public class VehicleRepository : GenericRepository<Vehicles>, IVehicleRepository
    {
        private readonly AllocationDbContext _context;

        public VehicleRepository(AllocationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<decimal?> GetCurrentPriceAsync(int vehicleId, DateTime date)
        {
           var price = await _context.VehiclePrices
                .Where(vp => vp.VehicleId == vehicleId && vp.StartDate <= date && vp.EndDate >=date)
                .OrderByDescending(vp => vp.StartDate)
                .FirstOrDefaultAsync();
            return price?.PriceAmount;

        }

        public async Task<IEnumerable<Vehicles>> GetVehiclesByStatusAsync(string status)
        {
            return await _context.Vehicles
                .Where(v => v.Status == status)
                .ToListAsync();
                
        }

        public async Task<IEnumerable<Vehicles>> GetVehiclesWithAvailableStockAsync()
        {
           return await _context.Vehicles
                .Include(v => v.EVInventories)
                .Where(v => v.EVInventories.Any(inv => inv.Quantity > 0))
                .ToListAsync();
        }

        public async Task<IEnumerable<Vehicles>> GetVehiclesWithPromotionsAsync()
        {
            var today = DateTime.UtcNow;
            return await _context.Vehicles
                .Include(v => v.VehiclePromotions)
                .Where(v => v.VehiclePromotions.Any(p => p.StartDate <= today && p.EndDate >= today))
                .ToListAsync();
        }

        public async Task<Vehicles?> GetVehicleWithDetailsAsync(int vehicleId)
        {
            return await _context.Vehicles
                .Include(v => v.EVInventories)
                .Include(v => v.Allocations)
                .Include(v => v.VehiclePrices)
                .Include(v => v.VehiclePromotions)
                .Include(v => v.Quotations)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);
        }

        public async Task<IEnumerable<Vehicles>> SearchVehiclesAsync(string? variantName, string? color, string? batteryCapacity)
        {
            var query = _context.Vehicles.AsQueryable();
            if (!string.IsNullOrEmpty(variantName))
            {
                query = query.Where(v => v.VariantName.Contains(variantName));
            }
            if (!string.IsNullOrEmpty(color))
            {
                query = query.Where(v => v.Color.Contains(color));
            }
            if (!string.IsNullOrEmpty(batteryCapacity))
            {
                query = query.Where(v => v.BatteryCapacity.Contains(batteryCapacity));
            }
            return await query.ToListAsync();
        }
    }
}
