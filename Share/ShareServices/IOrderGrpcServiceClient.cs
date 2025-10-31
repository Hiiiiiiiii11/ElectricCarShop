using Grpc.Core;
using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public interface IOrderGrpcServiceClient
    {
        // Trả về một stream call cho Orders.
        // Worker sẽ dùng `ReadAllAsync` hoặc `await foreach` để đọc.
        AsyncServerStreamingCall<OrderReply> GetAllOrders(GetAllOrderRequest request);

        // Trả về một stream call cho Quotations.
        AsyncServerStreamingCall<QuotationReply> GetAllQuotations(GetAllQuotationRequest request);
    }
}
