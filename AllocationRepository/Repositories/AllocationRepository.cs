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

        public async Task<Allocations?> GetByAgencyAndVehicleInstanceAsync(int agencyId, int vehicleInstanceId)
        {
            return await _context.Allocations
                .Include(a => a.VehicleInstance)
                .FirstOrDefaultAsync(a => a.AgencyId == agencyId && a.VehicleInstanceId == vehicleInstanceId);
        }

        public async Task<IEnumerable<Allocations>> GetByAgencyIdAsync(int agencyId)
        {
            return await _context.Allocations
                .Where(a => a.AgencyId == agencyId)
                .Include(a => a.VehicleInstance)
                .Include(a => a.EVInventory)
                .ToListAsync();
        }

        public async Task<Allocations?> GetByInventoryIdAsync(int evInventoryId)
        {
            return await _context.Allocations
                .Include(a => a.VehicleInstance)
                .Include(a => a.EVInventory)
                .FirstOrDefaultAsync(a => a.EvInventoryId == evInventoryId);
        }

        public async Task<IEnumerable<Allocations>> GetByVehicleInstanceIdAsync(int vehicleInstanceId)
        {
            return await _context.Allocations
               .Where(a => a.VehicleInstanceId == vehicleInstanceId)
               .Include(a => a.VehicleInstance)
               .Include(a => a.EvInventoryId)
               .ToListAsync();
        }
    }
}
