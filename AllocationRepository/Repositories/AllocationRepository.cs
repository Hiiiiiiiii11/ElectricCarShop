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
    public class AllocationRepository : GenericRepository<Allocations>, IAllocationRepository
    {
        private readonly AllocationDbContext _context;
        public AllocationRepository(AllocationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Allocations?> GetByDealerAndVehicleAsync(int dealerId, int vehicleId)
        {
            return await _context.Allocations
                .Include(a => a.DealerId)
                .Include(a => a.VehicleId)
                .FirstOrDefaultAsync(a => a.DealerId == dealerId && a.VehicleId == vehicleId);
        }

        public async Task<IEnumerable<Allocations>> GetByDealerIdAsync(int dealerId)
        {
            return await _context.Allocations
                .Where(a => a.DealerId == dealerId)
                .Include(a => a.VehicleId)
                .Include(a => a.EvInventoryId)
                .ToListAsync();
        }

        public async Task<Allocations?> GetByInventoryIdAsync(int evInventoryId)
        {
            return await _context.Allocations
                .Include(a => a.Vehicle)
                .Include(a => a.EVInventory)
                .FirstOrDefaultAsync(a => a.EvInventoryId == evInventoryId);
        }

        public async Task<IEnumerable<Allocations>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _context.Allocations
               .Where(a => a.VehicleId == vehicleId)
               .Include(a => a.VehicleId)
               .Include(a => a.EvInventoryId)
               .ToListAsync();
        }
    }
}
