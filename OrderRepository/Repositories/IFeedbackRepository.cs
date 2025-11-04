using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public interface IFeedbackRepository : IGenericRepository<Feedback>
    {
        Task<IEnumerable<Feedback>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<Feedback>> GetByStatusAsync(string status);
        Task<IEnumerable<Feedback>> GetFeedbackByAgencyId(int agencyId);
    }
}
