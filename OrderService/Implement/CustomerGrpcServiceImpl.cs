using Grpc.Core;
using GrpcService;
using OrderRepository.Repositories;
using System.Threading.Tasks;

namespace OrderAPIService.Services
{
    public class CustomerGrpcServiceImpl : CustomerGrpcService.CustomerGrpcServiceBase
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerGrpcServiceImpl(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public override async Task<CustomerReply> GetCustomerById(GetCustomerByIdRequest request, ServerCallContext context)
        {
            var customer = await _customerRepository.GetByIdAsync(request.Id);
            if (customer == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Customer with ID {request.Id} not found."));
            }

            return new CustomerReply
            {
                Id = customer.Id,
                FullName = customer.FullName ?? "",
                Email = customer.Email ?? "",
                Phone = customer.Phone ?? "",
                Address = customer.Address ?? ""
            };
        }
    }
}
