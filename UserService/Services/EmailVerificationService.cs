using Share.Setting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using UserRepository.Model;
using UserRepository.Model.DTO;
using UserRepository.Repositories;


namespace UserService.Services
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IEmailVerificationRepository _emailVerificationRepository;
        private readonly EmailSetting _emailSetting;
        public EmailVerificationService(IEmailVerificationRepository emailVerificationRepository,EmailSetting emailSetting)
        {
            _emailVerificationRepository = emailVerificationRepository;
            _emailSetting = emailSetting;
        }

        public async Task SendVerificationCodeAsync(string email)
        {
            var existing = await _emailVerificationRepository.GetByEmailAsync(email);

            if (existing != null && existing.IsVerified)
            {
                throw new InvalidOperationException("Email already verified");
            }
            if (existing != null)
            {
                // Check 1 phút kể từ lần gửi gần nhất
                var timeSinceLastSend = DateTime.UtcNow - (existing.ExpiredAt.AddMinutes(-10));
                if (timeSinceLastSend < TimeSpan.FromMinutes(1))
                {
                    throw new InvalidOperationException("You can request a new code only after 1 minute.");
                }
            }
            var codeOTP = new Random().Next(100000, 999999).ToString();
            var expiredAt = DateTime.UtcNow.AddMinutes(10);

            if (existing != null)
            {
                // Cập nhật record cũ
                existing.Code = codeOTP;
                existing.ExpiredAt = expiredAt;
                existing.IsVerified = false;

                _emailVerificationRepository.Update(existing);
            }
            else
            {
                // Tạo record mới
                var verificationEmail = new EmailVerification
                {
                    Email = email,
                    Code = codeOTP,
                    ExpiredAt = expiredAt,
                    IsVerified = false
                };
                await _emailVerificationRepository.AddAsync(verificationEmail);
            }

            await _emailVerificationRepository.SaveChangesAsync();

            await SendEmailAsync(email, "Email Verification Code",
                $"Your verification code is: <b>{codeOTP}</b>. It will expire in 10 minutes.");
        }


        public async Task<bool> VerifyCodeAsync(VerifyOTPRequest request)
        {
            var verification = await _emailVerificationRepository.GetByEmailAndCodeAsync(request.Email, request.Code);
            if (verification == null)
            {
                return false;
                throw new KeyNotFoundException("Invalid email or code");
                
            }

            if (verification.IsVerified) {
                return true; 
                throw new InvalidOperationException("Email already verified");
            }
            // Đã verify trước đó
            if (verification.ExpiredAt < DateTime.UtcNow)
            {
                return false;
                throw new InvalidOperationException("Verification code has expired");
            } // Hết hạn

            await _emailVerificationRepository.MarkAsVerifiedAsync(verification);
            return true;
        }

        private Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var client = new SmtpClient(_emailSetting.SmtpServer, _emailSetting.SmtpPort)
            {
                Credentials = new System.Net.NetworkCredential(_emailSetting.SenderEmail, _emailSetting.SenderPassword),
                EnableSsl = true
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSetting.SenderEmail, _emailSetting.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);
            return client.SendMailAsync(mailMessage);
        }
    }
}
