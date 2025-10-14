using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public interface ICustomerGrpcServiceClient
    {
        Task<CustomerReply> GetCustomerByIdAsync(int customerId);
    }
}
