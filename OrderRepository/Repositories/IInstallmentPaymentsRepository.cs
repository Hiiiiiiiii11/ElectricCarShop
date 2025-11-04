using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public interface IInstallmentPaymentsRepository : IGenericRepository<InstallmentPayments>
    {
        Task<List<InstallmentPayments>> GetByPlanIdAsync(int planId);
        Task<List<InstallmentPayments>> GetByItemIdAsync(int itemId);
    }
}
