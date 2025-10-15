using Grpc.Core;
using Greet; // Namespace từ file .proto

namespace UserAPI.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name + " from UserAPI!"
            });
        }
    }
}