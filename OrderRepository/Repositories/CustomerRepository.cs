using Microsoft.EntityFrameworkCore;
using OrderRepository.Data;
using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public class CustomerRepository : GenericRepository<Customers>, ICustomerRepository
    {
        private readonly OrderDbContext _context;

        public CustomerRepository(OrderDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Customers?> GetByEmailAsync(string email)
        {
            var normalized = email.Trim().ToLower();

            return await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.Email.Trim().ToLower() == normalized
                );
        }

        public async Task<Customers?> GetByPhoneAsync(string phone)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Phone == phone);
        }

        public async Task<IEnumerable<Customers>> SearchByNameAsync(string name)
        {
            return await _context.Customers
                .Where(c => c.FullName.Contains(name))
                .ToListAsync();
        }
        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            return await _context.Customers
                .AnyAsync(c => c.Email == email && (!excludeId.HasValue || c.Id != excludeId.Value));
        }

        public async Task<bool> PhoneExistsAsync(string phone, int? excludeId = null)
        {
            return await _context.Customers
                .AnyAsync(c => c.Phone == phone && (!excludeId.HasValue || c.Id != excludeId.Value));
        }
        public async Task<Customers?> GetByEmailAndAgencyAsync(string email, int agencyId)
        {
            var normalized = email.Trim().ToLower();

            return await _context.Customers
                .FirstOrDefaultAsync(c =>
                    c.Email.Trim().ToLower() == normalized &&
                    c.AgencyId == agencyId
                );
        }

    }
}
