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
    public class VehiclePriceRepository :GenericRepository<VehiclePrices>, IVehiclePriceRepository
    {
        private readonly AllocationDbContext _context;
        public VehiclePriceRepository(AllocationDbContext context) : base(context)
        {
            _context = context;
        }

        //public async Task<IEnumerable<VehiclePrices>> GetPriceHistoryAsync(int vehicleId)
        //{
        //    return await _context.VehiclePrices
        //        .Where(v => v.VehicleId == vehicleId)
        //        .OrderBy(p => p.StartDate)
        //        .ToListAsync();
        //}

        //public async Task<IEnumerable<VehiclePrices>> GetPricesByAgencyAsync(int agencyId)
        //{
        //    return await _context.VehiclePrices
        //        .Where(v => v.AgencyId == agencyId)
        //        .Include(p => p.Vehicle)
        //        .OrderByDescending(p => p.StartDate)
        //        .ToListAsync();

        //}

        //public async Task<IEnumerable<VehiclePrices>> GetPricesByVehicleIdAsync(int vehicleId)
        //{
        //    return await _context.VehiclePrices
        //        .Where(p => p.VehicleId == vehicleId)
        //        .Include(v => v.Vehicle)
        //        .OrderByDescending(p => p.StartDate)
        //        .ToListAsync();
        //}
    }
}
