using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public class EmailVerificationGrpcServiceClient : IEmailVerificationGrpcServiceClient
    {
        private readonly EmailVerificationGrpcService.EmailVerificationGrpcServiceClient _client;

        public EmailVerificationGrpcServiceClient(EmailVerificationGrpcService.EmailVerificationGrpcServiceClient client)
        {
            _client = client;
        }

        public async Task<bool> IsEmailVerifiedAsync(string email)
        {
            try
            {
                var response = await _client.IsEmailVerifiedAsync(new IsEmailVerifiedRequest
                {
                    Email = email
                });
                return response.IsVerified;
            }
            catch (System.Exception ex)
            {
                // Log lỗi và trả về false để ngăn việc tạo customer nếu service kia bị lỗi
                Console.WriteLine($"Error checking email verification for {email}: {ex.Message}");
                return false;
            }
        }
    }
}
