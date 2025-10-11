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
    public interface IVehicleOptionRepository : IGenericRepository<VehicleOptions>
    {
       Task<VehicleOptions?> GetByModelNameAsync(string modelName);

    }
}
