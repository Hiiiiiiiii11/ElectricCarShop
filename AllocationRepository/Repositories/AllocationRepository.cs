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

        public async Task<Allocations?> GetByAgencyAndVehicleAsync(int agencyId, int vehicleId)
        {
            return await _context.Allocations
                .Include(a => a.Vehicle)
                .FirstOrDefaultAsync(a => a.AgencyId == agencyId && a.VehicleId == vehicleId);
        }

        public async Task<IEnumerable<Allocations>> GetByAgencyIdAsync(int agencyId)
        {
            return await _context.Allocations
                .Where(a => a.AgencyId == agencyId)
                .Include(a => a.Vehicle)
                .Include(a => a.EVInventory)
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
               .Include(a => a.Vehicle)
               .Include(a => a.EvInventoryId)
               .ToListAsync();
        }
    }
}
