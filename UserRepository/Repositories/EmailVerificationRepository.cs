using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserRepository.Data;
using UserRepository.Model;

namespace UserRepository.Repositories
{
    public class EmailVerificationRepository : GenericRepository<EmailVerification>, IEmailVerificationRepository
    {
        private readonly UserDbContext _context;
        public EmailVerificationRepository(UserDbContext context) : base(context)
        {
            _context = context;
        }
        public async Task<EmailVerification?> GetByEmailAndCodeAsync(string email, string code)
        {
            return await _context.EmailVerifications
                .FirstOrDefaultAsync(ev => ev.Email == email && ev.Code == code && ev.IsVerified == false);
        }

        public async Task<EmailVerification?> GetByEmailAsync(string email)
        {
            return await _context.EmailVerifications
                .Where(ev => ev.Email == email)
                .OrderByDescending(ev => ev.ExpiredAt)
                .FirstOrDefaultAsync();
        }

        public async Task MarkAsVerifiedAsync(EmailVerification verification)
        {
            verification.IsVerified = true;
            _context.EmailVerifications.Update(verification);
            await _context.SaveChangesAsync();
        }
    }
}
