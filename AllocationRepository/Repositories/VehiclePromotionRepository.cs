using AllocationRepository.Data;
using AllocationRepository.Model;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public class VehiclePromotionRepository
        : GenericRepository<VehiclePromotions>, IVehiclePromotionRepository
    {
        private readonly AllocationDbContext _context;

        public VehiclePromotionRepository(AllocationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<IEnumerable<VehiclePromotions>> GetPromotionsByVehicleIdAsync(int vehicleId)
        {
            return await _context.VehiclePromotions
                .Where(p => p.VehicleId == vehicleId)
                .OrderByDescending(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<VehiclePromotions>> GetActivePromotionsAsync()
        {
            return await _context.VehiclePromotions
                .Where(p => p.EndDate >= DateTime.Now)
                .OrderBy(p => p.StartDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<VehiclePromotions>> GetExpiredPromotionsAsync()
        {
            return await _context.VehiclePromotions
                .Where(p => p.EndDate < DateTime.Now)
                .OrderByDescending(p => p.EndDate)
                .ToListAsync();
        }
    }
}
