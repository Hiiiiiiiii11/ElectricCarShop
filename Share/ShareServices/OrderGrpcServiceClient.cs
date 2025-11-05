using Grpc.Core;
using GrpcService;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public class OrderGrpcServiceClient : IOrderGrpcServiceClient
    {
        // Đây là client "gốc" được sinh ra từ file .proto
        private readonly OrderGrpcService.OrderGrpcServiceClient _client;

        // Tiêm (inject) client gRPC "gốc"
        public OrderGrpcServiceClient(OrderGrpcService.OrderGrpcServiceClient client)
        {
            _client = client;
        }

        public AsyncServerStreamingCall<OrderReply> GetAllOrders(GetAllOrderRequest request)
        {
            // Không dùng "await" ở đây. 
            // Chúng ta trả về stream để worker tự xử lý.
            return _client.GetAllOrders(request);
        }

        public AsyncServerStreamingCall<QuotationReply> GetAllQuotations(GetAllQuotationRequest request)
        {
            // Trả về stream
            return _client.GetAllQuotations(request);
        }
        public async Task<CheckQuotationExistsReply> CheckQuotationExistsForVehicleAsync(int vehicleInstanceId)
        {
            var request = new CheckQuotationExistsRequest { VehicleInstanceId = vehicleInstanceId };
            return await _client.CheckQuotationExistsForVehicleAsync(request);
        }
    }
}