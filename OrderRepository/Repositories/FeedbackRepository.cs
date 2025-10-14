using Microsoft.EntityFrameworkCore;
using OrderRepository.Data;
using OrderRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Repositories
{
    public class FeedbackRepository : GenericRepository<Feedback>, IFeedbackRepository
    {
        private readonly OrderDbContext _context;
        public FeedbackRepository(OrderDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Feedback>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.Feedbacks
                .Where(f => f.CustomerId == customerId)
                .ToListAsync();
        }
        public async Task<IEnumerable<Feedback>> GetByStatusAsync(string status)
        {
            return await _context.Feedbacks
                .Where(f => f.Status.ToLower() == status.ToLower())
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}
