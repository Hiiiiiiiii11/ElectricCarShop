using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public interface IInstallmentPlansRepository : IGenericRepository<InstallmentPlans>
    {
        Task<InstallmentPlans?> GetByContractIdAsync(int contractId);
        Task<InstallmentPlans?> GetByAgencyContractIdAsync(int agencyContractId);
        Task<InstallmentPlans?> GetWithItemsByIdAsync(int id);
        Task<InstallmentPlans?> GetPlanWithDetailsAsync(int id);
        Task<IEnumerable<InstallmentPlans>> GetAllPlansWithDetailsAsync();
    }
}
