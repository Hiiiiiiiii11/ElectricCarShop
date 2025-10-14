using OrderRepository.Model.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface ICustomerService
    {
        Task<IEnumerable<CustomerResponse>> GetAllAsync();
        Task<CustomerResponse?> GetByIdAsync(int id);
        Task<CustomerResponse> CreateAsync(CustomerRequest request);
        Task<CustomerResponse> UpdateAsync(int id, CustomerRequest request);
        Task<bool> DeleteAsync(int id);

        // Extra features
        Task<CustomerResponse?> GetByEmailAsync(string email);
        Task<CustomerResponse?> GetByPhoneAsync(string phone);
        Task<IEnumerable<CustomerResponse>> SearchByNameAsync(string name);
    }
}
