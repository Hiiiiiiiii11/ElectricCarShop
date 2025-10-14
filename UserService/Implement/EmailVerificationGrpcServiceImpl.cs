using Grpc.Core;
using GrpcService;
using System.Threading.Tasks;
using UserRepository.Repositories;

namespace UserAPIService.Services
{
    public class EmailVerificationGrpcServiceImpl : EmailVerificationGrpcService.EmailVerificationGrpcServiceBase
    {
        private readonly IEmailVerificationRepository _emailVerificationRepository;

        public EmailVerificationGrpcServiceImpl(IEmailVerificationRepository emailVerificationRepository)
        {
            _emailVerificationRepository = emailVerificationRepository;
        }

        public override async Task<IsEmailVerifiedReply> IsEmailVerified(IsEmailVerifiedRequest request, ServerCallContext context)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Email cannot be empty."));
            }

            var verificationRecord = await _emailVerificationRepository.GetByEmailAsync(request.Email);

            // Trả về true chỉ khi record tồn tại VÀ đã được xác thực (IsVerified = true)
            var isVerified = verificationRecord != null && verificationRecord.IsVerified;

            return new IsEmailVerifiedReply { IsVerified = isVerified };
        }
    }
}
