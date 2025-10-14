
using AllocationRepository.Model;
using OrderRepository.Data;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public class QuotationRepository : GenericRepository<Quotations>, IQuotationRepository
    {
        public QuotationRepository(OrderDbContext context) : base(context)
        {
        }
    }
}
