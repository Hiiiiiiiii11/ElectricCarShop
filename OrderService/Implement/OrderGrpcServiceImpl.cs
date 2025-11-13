using AllocationRepository.Repositories;
using Grpc.Core;
using GrpcService;
using OrderRepository.Repositories; // Namespace của repo Order
using System;
using System.Threading.Tasks;

namespace OrderService.Implement
{
    // Kế thừa từ class base của .proto
    public class OrderGrpcServiceImpl : OrderGrpcService.OrderGrpcServiceBase
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IQuotationRepository _quotationRepo;

        public OrderGrpcServiceImpl(IOrderRepository orderRepo, IQuotationRepository quotationRepo)
        {
            _orderRepo = orderRepo;
            _quotationRepo = quotationRepo;
        }

        // === TRIỂN KHAI CHO ETL (STREAMING) ===

        public override async Task GetAllOrders(GetAllOrderRequest request,IServerStreamWriter<OrderReply> responseStream,ServerCallContext context)
        {
            // Lấy Order + OrderDetails
            var orders = await _orderRepo.GetAllWithDetailsAsync();

            foreach (var order in orders)
            {
                var orderReply = new OrderReply
                {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    OrderDate = order.OrderDate.ToString("o"),
                    Status = order.Status ?? "",
                    TotalAmount = (double)order.TotalAmount
                };

                // ⭐ THÊM DETAILS VÀO ORDERREPLY
                foreach (var d in order.Details)
                {
                    orderReply.Details.Add(new OrderDetailReply
                    {
                        Id = d.Id,
                        OrderId = d.OrderId,
                        QuotationId = d.QuotationId,
                        UnitPrice = (double)d.UnitPrice
                    });
                }

                await responseStream.WriteAsync(orderReply);
            }
        }

        public override async Task GetAllQuotations(GetAllQuotationRequest request, IServerStreamWriter<QuotationReply> responseStream, ServerCallContext context)
        {
            // AI chỉ quan tâm đến các báo giá đã được chấp nhận
            var quotations = await _quotationRepo.GetAllAsync();

            foreach (var quote in quotations)
            {
                await responseStream.WriteAsync(new QuotationReply
                {
                    Id = quote.Id,
                    AgencyId = quote.AgencyId,
                    CustomerId = quote.CustomerId,
                    VehicleInstanceId = quote.VehicleInstanceId,
                    QuotedPrice = (double)quote.QuotedPrice, // Chuyển decimal sang double
                    CreatedDate = quote.CreatedAt.ToString("o"), // Chuyển DateTime sang string
                    Status = quote.Status ?? ""
                });
            }
        }
        public override async Task<CheckQuotationExistsReply> CheckQuotationExistsForVehicle(
            CheckQuotationExistsRequest request, ServerCallContext context)
        {
            // Tìm bất kỳ báo giá nào (đang chờ hoặc đã chấp nhận)
            // cho chiếc xe này.
            var existingQuotation = await _quotationRepo.FindAsync(q =>
                q.VehicleInstanceId == request.VehicleInstanceId &&
                (q.Status == "Pending" || q.Status == "Accepted")
            );

            return new CheckQuotationExistsReply
            {
                Exists = existingQuotation.Any()
            };
        }
    }
}