using AllocationRepository.Data;
using AllocationRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public class VehiclePromotionRepository : GenericRepository<VehiclePromotions>, IVehiclePromotionRepository
    {
        public VehiclePromotionRepository(AllocationDbContext context) : base(context)
        {
        }
    }
}
