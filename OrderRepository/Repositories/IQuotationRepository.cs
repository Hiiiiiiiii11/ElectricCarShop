using AllocationRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public interface IQuotationRepository : IGenericRepository<Quotations>
    {
        Task<IEnumerable<Quotations>> GetQuotationByUserCreateId(int userId);
        Task<IEnumerable<Quotations>> GetQuotationByAgencyId(int agencyId);
    }
}
