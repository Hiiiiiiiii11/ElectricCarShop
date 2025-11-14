using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public interface ICustomerRepository :IGenericRepository<Customers>
    {
        Task<IEnumerable<Customers>> SearchByNameAsync(string name);

        Task<Customers?> GetByPhoneAsync(string phone);

        Task<Customers?> GetByEmailAsync(string email);

        Task<bool> EmailExistsAsync(string email, int? excludeId = null);
        Task<bool> PhoneExistsAsync(string phone, int? excludeId = null);
        Task<Customers?> GetByEmailAndAgencyAsync(string email, int agencyId);
    }
}
