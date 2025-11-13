using OrderRepository.Model.OrderDTO;
using OrderRepository.Model;
using OrderRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AllocationRepository.Repositories;
using Share.ShareServices;
using Microsoft.EntityFrameworkCore;

namespace OrderService.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IQuotationRepository _quotationRepository;
        private readonly IAgencyGrpcServiceClient _agencyGrpcServiceClient;
        private readonly IVehicleInstanceGrpcServiceClient _vehicleInstanceGrpcServiceClient;

        public OrderService(IOrderRepository orderRepository , IOrderDetailRepository orderDetailRepository, IQuotationRepository quotationRepository, IAgencyGrpcServiceClient agencyGrpcServiceClient,IVehicleInstanceGrpcServiceClient vehicleInstanceGrpcServiceClient)
        {
            _orderRepository = orderRepository;
            _orderDetailRepository = orderDetailRepository;
            _quotationRepository = quotationRepository;
            _agencyGrpcServiceClient = agencyGrpcServiceClient;
            _vehicleInstanceGrpcServiceClient = vehicleInstanceGrpcServiceClient;

        }
        public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
        {
            // 1. Tạo đối tượng Order ban đầu
            var newOrder = new Orders
            {
                CustomerId = request.CustomerId,
                Status = "Pending", // Gán một status mặc định
                CreateBy = request.CreateBy ?? 0,
                OrderDate = DateTime.UtcNow,
                TotalAmount = 0 // Khởi tạo tổng tiền bằng 0
            };

            // 2. Lưu Order vào DB để lấy Id
            await _orderRepository.AddAsync(newOrder);
            await _orderRepository.SaveChangesAsync();
            // Giờ newOrder.Id đã có giá trị

            // 3. Xử lý và tạo các OrderDetail
            if (request.Details != null && request.Details.Any())
            {
                var newDetailsList = new List<OrderDetail>();
                foreach (var detailRequest in request.Details)
                {
                    decimal unitPrice;

                    // Kiểm tra xem UnitPrice có được cung cấp không
                    if (detailRequest.UnitPrice.HasValue)
                    {
                        unitPrice = detailRequest.UnitPrice.Value;
                    }
                    else
                    {
                        // Nếu không, lấy giá từ Quotation
                        var quotation = await _quotationRepository.GetByIdAsync(detailRequest.QuotationId);
                        if (quotation == null)
                        {
                            // Nếu quotation không tìm thấy, bạn nên có cơ chế rollback
                            // hoặc throw exception để dừng tiến trình
                            throw new KeyNotFoundException($"Quotation with ID {detailRequest.QuotationId} not found.");
                        }
                        // Giả định model Quotation có thuộc tính QuotedPrice
                        unitPrice = quotation.QuotedPrice;
                    }

                    // Tạo đối tượng OrderDetail
                    var newDetail = new OrderDetail
                    {
                        OrderId = newOrder.Id, // Gán Id của Order vừa tạo
                        QuotationId = detailRequest.QuotationId,
                        UnitPrice = unitPrice
                    };

                    // Lưu OrderDetail vào DB
                    newDetailsList.Add(newDetail);
                }
                if (newDetailsList.Any())
                {
                    await _orderDetailRepository.AddRangeAsync(newDetailsList);
                    await _orderDetailRepository.SaveChangesAsync();
                }
            }

            // 4. Tính toán lại tổng giá của Order DỰA TRÊN repo
            // (Đúng theo yêu cầu "sau đó tính toán lại")
            decimal finalTotalAmount = await _orderDetailRepository.GetTotalAmountByOrderAsync(newOrder.Id);

            // 5. Cập nhật lại Order với tổng giá trị đúng
            newOrder.TotalAmount = finalTotalAmount;
             _orderRepository.Update(newOrder);
            await _orderRepository.SaveChangesAsync();

            // 6. Lấy lại đầy đủ thông tin Order (bao gồm Customer và Details) để trả về
            var completeOrder = await _orderRepository.GetWithDetailsAsync(newOrder.Id);

            // 7. Map sang OrderResponse
            return MapToResponse(completeOrder);
        }
        public async Task<OrderResponse> GetOrderByIdAsync(int orderId)
        {
            var order = await _orderRepository.GetWithDetailsAsync(orderId);
            if (order == null)
            {
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");
            }
            return MapToResponse(order);
        }
        public async Task<IEnumerable<OrderResponse>> GetAllOrdersAsync()
        {
            // Giả định bạn có hàm GetAllWithDetailsAsync hoặc tương tự
            // Nếu không, bạn cần Get All rồi lặp qua để Get Details
            var orders = await _orderRepository.GetAllWithDetailsAsync();
            return orders.Select(MapToResponse);
        }

        public async Task<OrderResponse> UpdateOrderStatusAsync(int orderId, UpdateOrderStatusRequest request)
        {
            // Lấy đầy đủ Order + Details + Quotation
            var order = await _orderRepository.GetWithDetailsAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            // Cập nhật trạng thái đơn hàng
            order.Status = request.Status;
            _orderRepository.Update(order);
            await _orderRepository.SaveChangesAsync();

            // Nếu đơn hàng hoàn tất hoặc đang chờ thanh toán
            if (request.Status == "Completed" || request.Status == "Pending-Payment")
            {
                var orderDate = order.OrderDate;

                foreach (var detail in order.Details)
                {
                    // Lấy quotation
                    var quotation = await _quotationRepository.GetByIdAsync(detail.QuotationId);
                    if (quotation == null)
                        continue;

                    int agencyId = quotation.AgencyId;
                    int vehicleInstanceId = quotation.VehicleInstanceId;

                    // Lấy vehicleInstance (gRPC)
                    var vehicleInstance = await _vehicleInstanceGrpcServiceClient
                        .GetVehicleInstanceByIdAsync(vehicleInstanceId);

                    if (vehicleInstance == null)
                        continue;

                    int vehicleId = vehicleInstance.VehicleId;

                    // 🔥 Gọi gRPC để tăng AchievedUnits (+1 cho mỗi xe)
                    await _agencyGrpcServiceClient.IncreaseAchievedUnitsAsync(
                        agencyId,
                        vehicleId,
                        orderDate.Year,
                        orderDate.Month,
                        1
                    );
                }
            }

            // Lấy lại để trả về
            var updated = await _orderRepository.GetWithDetailsAsync(orderId);
            return MapToResponse(updated);
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Order with ID {orderId} not found.");

            _orderRepository.Remove(order);

            try
            {
                await _orderRepository.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Không thể xóa đơn hàng vì đang được tham chiếu ở bảng khác.", ex);
            }
        }
        public async Task<IEnumerable<OrderResponse>> GetOrdersByAgencyIdAsync(int agencyId)
        {
            var orders = await _orderRepository.GetByAgencyIdAsync(agencyId);
            if (!orders.Any())
                return new List<OrderResponse>();

            var agencyInfo = await _agencyGrpcServiceClient.GetAgencyByIdAsync(agencyId)
                ?? throw new KeyNotFoundException($"Agency with ID {agencyId} not found.");

            return orders.Select(o => new OrderResponse
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                CreateBy = o.CreateBy,
                AgencyReply = agencyInfo,
                Details = o.Details.Select(d => new CreateOrderDetailItem
                {
                    QuotationId = d.QuotationId,
                    UnitPrice = d.UnitPrice
                }).ToList()
            });
        }


        private static OrderResponse MapToResponse(Orders order) => new OrderResponse
        {
            Id = order.Id,
            CustomerId = order.CustomerId,
            OrderDate = order.OrderDate,
            TotalAmount = order.TotalAmount, // Đây là tổng tiền đã được tính lại
            Status = order.Status,
            CreateBy = order.CreateBy,
            // Map lại danh sách Details
            Details = order.Details?.Select(d => new CreateOrderDetailItem
            {
                QuotationId = d.QuotationId,
                UnitPrice = d.UnitPrice
            }).ToList()
        };
    }
}
