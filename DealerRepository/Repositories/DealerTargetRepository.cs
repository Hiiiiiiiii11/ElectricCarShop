using DealerRepository.Data;
using DealerRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Repositories
{
    public class DealerTargetRepository : GenericRepository<DealerTargets>, IDealerTargetRepository
    {
        private readonly DealerDbContext _context;
        public DealerTargetRepository(DealerDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
