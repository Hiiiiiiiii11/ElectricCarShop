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

        public async Task<Allocations?> GetByAgencyAndVehicleInstanceAsync(int agencycontractId, int vehicleInstanceId)
        {
            return await _context.Allocations
                .Include(a => a.VehicleInstance)
                .FirstOrDefaultAsync(a => a.AgencyContractId == agencycontractId && a.VehicleInstanceId == vehicleInstanceId);
        }

        public async Task<IEnumerable<Allocations>> GetByAgencyIdAsync(int agencycontractId)
        {
            return await _context.Allocations
                .Where(a => a.AgencyContractId == agencycontractId)
                .Include(a => a.VehicleInstance)
                .ToListAsync();
        }


        public async Task<IEnumerable<Allocations>> GetByVehicleInstanceIdAsync(int vehicleInstanceId)
        {
            return await _context.Allocations
               .Where(a => a.VehicleInstanceId == vehicleInstanceId)
               .Include(a => a.VehicleInstance)
               .ToListAsync();
        }
        public async Task<IEnumerable<Allocations>> GetByAgencyOrderIdAsync(int agencyOrderId)
        {
            return await _context.Allocations
                .Where(a => a.AgencyOrderId == agencyOrderId) // Lọc theo khóa ngoại mới
                .Include(a => a.VehicleInstance)
                .ToListAsync();
        }
        public async Task<IEnumerable<Allocations>> GetAllWithDetailsAsync()
        {
            return await _context.Allocations
                .Include(a => a.VehicleInstance)       // Tải VehicleInstance
                    .ThenInclude(vi => vi.Vehicle)      // Tải Vehicle (từ VehicleInstance)
                        .ThenInclude(v => v.VehicleOption) // Tải VehicleOption (từ Vehicle)
                .ToListAsync();
        }
    }
}
