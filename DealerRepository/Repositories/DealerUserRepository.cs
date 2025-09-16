using DealerRepository.Data;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Repositories
{
    public class DealerUserRepository : GenericRepository<DealerUserRepository>, IDealerUserRepository
    {
        private readonly DealerDbContext _context;
        public DealerUserRepository(DealerDbContext context) : base(context)
        {
            _context = context;
        }
    }
}
