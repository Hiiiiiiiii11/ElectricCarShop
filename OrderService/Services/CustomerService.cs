using OrderRepository.Model;
using OrderRepository.Model.Request;
using OrderRepository.Repositories;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IEmailVerificationGrpcServiceClient _emailVerificationGrpcClient;
        public CustomerService(ICustomerRepository customerRepository, IEmailVerificationGrpcServiceClient emailVerificationGrpcServiceClient )
        {
            _customerRepository = customerRepository;
            _emailVerificationGrpcClient = emailVerificationGrpcServiceClient;
        }

        public async Task<CustomerResponse> CreateAsync(CustomerRequest request)
        {
            // ===== BƯỚC KIỂM TRA MỚI =====
            // 1. Kiểm tra xem email đã được xác thực chưa bằng gRPC
            var isEmailVerified = await _emailVerificationGrpcClient.IsEmailVerifiedAsync(request.Email);
            if (!isEmailVerified)
            {
                // Nếu chưa, ném ra lỗi và không cho tạo
                throw new InvalidOperationException($"Email '{request.Email}' has not been verified. Please verify the email before creating a customer.");
            }
            // =============================

            // 2. Nếu đã xác thực, tiếp tục tạo customer như cũ
            var newCustomer = new Customers
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Address = request.Address,
                CreateAt = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            };

            await _customerRepository.AddAsync(newCustomer);
            await _customerRepository.SaveChangesAsync();

            return MapToResponse(newCustomer);
        }
        public async Task<CustomerResponse> UpdateAsync(int id, CustomerRequest request)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {id} not found.");

            // chỉ update nếu có giá trị mới
            if (!string.IsNullOrWhiteSpace(request.FullName))
                customer.FullName = request.FullName;

            if (!string.IsNullOrWhiteSpace(request.Email))
                customer.Email = request.Email;

            if (!string.IsNullOrWhiteSpace(request.Phone))
                customer.Phone = request.Phone;

            if (!string.IsNullOrWhiteSpace(request.Address))
                customer.Address = request.Address;

            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync();

            return MapToResponse(customer);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {id} not found.");

            _customerRepository.Remove(customer);
            await _customerRepository.SaveChangesAsync();

            return true;
        }


        public async Task<CustomerResponse?> GetByEmailAsync(string email)
        {
            var customer = await _customerRepository.GetByEmailAsync(email);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with Email {email} not found.");
            return  MapToResponse(customer);
        }
        public async Task<IEnumerable<CustomerResponse>> GetAllAsync()
        {
            var customers = await _customerRepository.GetAllAsync();
            return customers.Select(MapToResponse);
        }

        // ✅ READ (Get by ID)
        public async Task<CustomerResponse?> GetByIdAsync(int id)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {id} not found.");
            return  MapToResponse(customer);
        }

        public async Task<CustomerResponse?> GetByPhoneAsync(string phone)
        {
            var customer = await _customerRepository.GetByPhoneAsync(phone);
            return customer == null ? null : MapToResponse(customer);
        }

        public async Task<IEnumerable<CustomerResponse>> SearchByNameAsync(string name)
        {
            var customers = await _customerRepository.SearchByNameAsync(name);
            return customers.Select(MapToResponse);
        }

       
        private CustomerResponse MapToResponse(Customers c)
        {
            return new CustomerResponse
            {
                Id = c.Id,
                FullName = c.FullName,
                Email = c.Email,
                Phone = c.Phone,
                Address = c.Address,
                CreateAt = c.CreateAt
            };
        }
    }
}
