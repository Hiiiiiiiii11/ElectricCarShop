using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public class CustomerGrpcServiceClient : ICustomerGrpcServiceClient
    {
        private readonly CustomerGrpcService.CustomerGrpcServiceClient _client;

        public CustomerGrpcServiceClient(CustomerGrpcService.CustomerGrpcServiceClient client)
        {
            _client = client;
        }

        public async Task<CustomerReply> GetCustomerByIdAsync(int customerId)
        {
            return await _client.GetCustomerByIdAsync(new GetCustomerByIdRequest
            {
                Id = customerId
            });
        }
    }
}
