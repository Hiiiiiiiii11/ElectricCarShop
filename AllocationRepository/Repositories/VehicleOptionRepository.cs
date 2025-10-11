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
    public class VehicleOptionRepository : GenericRepository<VehicleOptions>, IVehicleOptionRepository
    {
        private readonly AllocationDbContext _context;
        public VehicleOptionRepository(AllocationDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<VehicleOptions?> GetByModelNameAsync(string modelName)
        {
            return await _context.VehicleOptions
                .FirstOrDefaultAsync(vo => vo.ModelName == modelName);
        }
    }
}
