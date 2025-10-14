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
        private readonly OrderDBContext _context;

        public CustomerRepository(OrderDBContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Customers?> GetByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);
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
    }
}
