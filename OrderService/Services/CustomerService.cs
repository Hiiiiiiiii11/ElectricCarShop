using AllocationRepository.Model;
using OrderRepository.Model;
using OrderRepository.Model.OrderDTO;
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

        public async Task<Customers> FindOrCreateCustomerAsync(CustomerRequest request)
        {
            var normalizedEmail = request.Email.Trim().ToLower();

            // 1) Verify email
            var isEmailVerified = await _emailVerificationGrpcClient.IsEmailVerifiedAsync(request.Email);
            if (!isEmailVerified)
                throw new InvalidOperationException($"Email '{request.Email}' has not been verified.");

            // 2) Tìm customer theo Email + AgencyId
            var existingCustomer = await _customerRepository.GetByEmailAndAgencyAsync(
                normalizedEmail,
                request.AgencyId.Value
            );

            if (existingCustomer != null)
            {
                // Kiểm tra phone khớp
                if (existingCustomer.Phone != request.Phone)
                    throw new InvalidOperationException(
                        $"Email '{request.Email}' đã đăng ký tại đại lý này nhưng số điện thoại không khớp."
                    );

                // Có thể update profile
                bool needUpdate = false;

                if (existingCustomer.FullName != request.FullName)
                {
                    existingCustomer.FullName = request.FullName;
                    needUpdate = true;
                }
                if (existingCustomer.Address != request.Address)
                {
                    existingCustomer.Address = request.Address;
                    needUpdate = true;
                }

                if (needUpdate)
                {
                    _customerRepository.Update(existingCustomer);
                    await _customerRepository.SaveChangesAsync();
                }

                return existingCustomer;
            }

            // 3) Tạo mới nếu chưa có trong Agency này
            var newCustomer = new Customers
            {
                FullName = request.FullName,
                Email = normalizedEmail,
                Phone = request.Phone.Trim(),
                Address = request.Address,
                AgencyId = request.AgencyId,
                Class = request.Class ?? "Normal",
                CreateAt = DateTime.UtcNow
            };

            await _customerRepository.AddAsync(newCustomer);
            await _customerRepository.SaveChangesAsync();

            return newCustomer;
        }






        public async Task<CustomerResponse> UpdateAsync(int id, CustomerUpdateRequest request)
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {id} not found.");

            //if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != customer.Email)
            //{
            //    if (await _customerRepository.EmailExistsAsync(request.Email, excludeId: id))
            //        throw new InvalidOperationException($"Email '{request.Email}' already exists.");
            //}

            //if (!string.IsNullOrWhiteSpace(request.Phone) && request.Phone != customer.Phone)
            //{
            //    if (await _customerRepository.PhoneExistsAsync(request.Phone, excludeId: id))
            //        throw new InvalidOperationException($"Phone '{request.Phone}' already exists.");
            //}

            if (!string.IsNullOrWhiteSpace(request.FullName))
                customer.FullName = request.FullName;

            if (!string.IsNullOrWhiteSpace(request.Email))
                customer.Email = request.Email;

            if (!string.IsNullOrWhiteSpace(request.Phone))
                customer.Phone = request.Phone;

            if (!string.IsNullOrWhiteSpace(request.Address))
                customer.Address = request.Address;
            if (!string.IsNullOrWhiteSpace(request.Class))
                customer.Class = request.Class;

            if (request.AgencyId.HasValue)
                customer.AgencyId = request.AgencyId.Value;
            else if (request.AgencyId == null)
                customer.AgencyId = null;

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
                AgencyId = c.AgencyId,
                Class = c.Class,
                CreateAt = c.CreateAt
            };
        }
    }
}
