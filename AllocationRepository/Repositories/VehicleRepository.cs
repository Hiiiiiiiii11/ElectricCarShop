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
    public class VehicleRepository : GenericRepository<Vehicles>, IVehicleRepository
    {
        public VehicleRepository(AllocationDbContext context) : base(context)
        {
        }
    }
}
