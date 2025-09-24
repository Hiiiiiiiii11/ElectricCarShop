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
    public class AllocationRepository : GenericRepository<Allocations>, IAllocationRepository
    {
        public AllocationRepository(AllocationDbContext context) : base(context)
        {
        }
    }
    {
    }
}
