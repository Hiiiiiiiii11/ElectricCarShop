﻿using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserRepository.Model;

namespace UserRepository.Repositories
{
    public interface IEmailVerificationRepository: IGenericRepository<EmailVerification>
    {
        Task<EmailVerification?> GetByEmailAndCodeAsync(string email, string code);
        Task MarkAsVerifiedAsync(EmailVerification verification);
        Task<EmailVerification?> GetByEmailAsync(string email);
    }
}
