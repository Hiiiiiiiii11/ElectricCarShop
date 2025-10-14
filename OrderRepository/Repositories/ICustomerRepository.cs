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
    }
}
