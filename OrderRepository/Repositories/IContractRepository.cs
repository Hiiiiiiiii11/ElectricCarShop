using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public interface IContractRepository : IGenericRepository<Contracts>
    {
        Task<IEnumerable<Contracts>> GetByQuotationIdAsync(int quotationId);
        Task<Contracts?> GetByContractNumberAsync(string contractNumber);
        Task<IEnumerable<Contracts>> GetContractsByAgencyIdAsync(int agencyId);
    }
}
