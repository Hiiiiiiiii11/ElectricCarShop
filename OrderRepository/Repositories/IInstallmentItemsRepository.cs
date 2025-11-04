using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public interface IInstallmentItemsRepository : IGenericRepository<InstallmentItems>
    {
        Task<IEnumerable<InstallmentItems>> GetByPlanIdAsync(int planId);
    }
}
