using AllocationRepository.Data;
using AllocationRepository.Model;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public class VehicleInstanceRepository : GenericRepository<VehicleInstance>, IVehicleInstanceRepository
    {
        private readonly AllocationDbContext _context;

        public VehicleInstanceRepository(AllocationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehicleInstance>> GetByVehicleIdAsync(int vehicleId)
        {
            return await _context.VehicleInstances
                .Include(v => v.Vehicle)
                .Where(v => v.VehicleId == vehicleId)
                .ToListAsync();
        }

        public async Task<VehicleInstance?> GetByVinAsync(string vin)
        {
            return await _context.VehicleInstances
                .Include(v => v.Vehicle)
                .FirstOrDefaultAsync(v => v.Vin == vin);
        }

        public async Task<VehicleInstance?> GetByEngineNumberAsync(string engineNumber)
        {
            return await _context.VehicleInstances
                .Include(v => v.Vehicle)
                .FirstOrDefaultAsync(v => v.EngineNumber == engineNumber);
        }

        public async Task<bool> IsVinExistAsync(string vin)
        {
            return await _context.VehicleInstances.AnyAsync(v => v.Vin == vin);
        }

        public async Task<bool> IsEngineNumberExistAsync(string engineNumber)
        {
            return await _context.VehicleInstances.AnyAsync(v => v.EngineNumber == engineNumber);
        }

        public async Task<int> CountByVehicleIdAsync(int vehicleId)
        {
            return await _context.VehicleInstances.CountAsync(v => v.VehicleId == vehicleId);
        }
        public async Task<VehicleInstance> GetByIdWithDetailsAsync(int id)
        {
            return await _context.VehicleInstances
                                 .Include(vi => vi.Vehicle) // Nạp thông tin từ Vehicles
                                    .ThenInclude(v => v.VehicleOption) // Từ Vehicles, nạp tiếp VehicleOption
                                 .FirstOrDefaultAsync(vi => vi.Id == id);
        }
        public async Task<IEnumerable<VehicleInstance>> GetAllWithDetailsAsync()
        {
            return await _context.VehicleInstances
                                 .Include(vi => vi.Vehicle) // N���p thông tin từ Vehicles
                                    .ThenInclude(v => v.VehicleOption) // Từ Vehicles, nạp tiếp VehicleOption
                                 .ToListAsync();
        }
    }
}
