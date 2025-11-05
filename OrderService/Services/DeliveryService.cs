using AllocationRepository.Repositories;
using Microsoft.Extensions.Logging;
using OrderRepository.Model;
using OrderRepository.Model.Request;
using OrderRepository.Repositories;
using Share.ShareServices;
using System.Net.WebSockets;

namespace OrderService.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IUploadPhotoService _uploadPhotoService;

        private readonly IOrderDetailRepository _orderDetailRepository;
        private readonly IQuotationRepository _quotationRepository;
        private readonly IAgencyGrpcServiceClient _agencyGrpcServiceClient;
        private readonly ILogger<DeliveryService> _logger;

        public DeliveryService(IDeliveryRepository deliveryRepository,
            IUploadPhotoService uploadPhotoService,
            IOrderDetailRepository orderDetailRepository,
            IQuotationRepository quotationRepository,
            IAgencyGrpcServiceClient agencyGrpcServiceClient,
            ILogger<DeliveryService> logger

            )
        {
            _deliveryRepository = deliveryRepository;
            _uploadPhotoService = uploadPhotoService;
            _orderDetailRepository = orderDetailRepository;
            _quotationRepository = quotationRepository;
            _agencyGrpcServiceClient = agencyGrpcServiceClient;
            _logger = logger;
        }

        public async Task<DeliveryResponse?> GetByIdAsync(int id)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(id);
            if (delivery == null)
                throw new KeyNotFoundException("Delivery not found.");
            return MapToRespons(delivery);
        }

        public async Task<IEnumerable<DeliveryResponse>> GetByOrderIdAsync(int orderId)
        {
            var deliveries = await _deliveryRepository.GetByOrderAsync(orderId);
            return deliveries.Select(MapToRespons).ToList();
        }

        public async Task<(IEnumerable<DeliveryResponse> items, int total)> ListAsync(
            string? status, DateTime? from, DateTime? to, int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            var (items, total) = await _deliveryRepository.FilterAsync(
                status, from, to, (page - 1) * pageSize, pageSize);
            return (items.Select(MapToRespons).ToList(), total);
        }

        public async Task<DeliveryResponse> CreateAsync(DeliveryCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.DeliveryStatus))
                throw new InvalidOperationException("DeliveryStatus is required.");

            var e = new Delivery
            {
                OrderId = req.OrderId,
                DeliveryDate = req.DeliveryDate,
                DeliveryStatus = req.DeliveryStatus.Trim(),
                Notes = req.Notes
            };

            // ✅ Upload image before saving
            if (req.ImgUrlBefore != null)
                e.ImgUrlBefore = _uploadPhotoService.UploadPhoto(req.ImgUrlBefore);

            if (req.ImgUrlAfter != null)
                e.ImgUrlAfter = _uploadPhotoService.UploadPhoto(req.ImgUrlAfter);

            await _deliveryRepository.AddAsync(e);
            await _deliveryRepository.SaveChangesAsync();

            if (e.DeliveryStatus.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
            {
                await RemoveVehicleFromInventoryAsync(e.OrderId);
            }

            return MapToRespons(e);
        }

        public async Task<DeliveryResponse> UpdateAsync(int id, DeliveryUpdateRequest req)
        {
            var e = await _deliveryRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Delivery not found.");

            string oldStatus = e.DeliveryStatus;
            string newStatus = string.IsNullOrWhiteSpace(req.DeliveryStatus) ? oldStatus : req.DeliveryStatus.Trim();

            if (req.DeliveryDate.HasValue)
                e.DeliveryDate = req.DeliveryDate.Value;

            e.DeliveryStatus = newStatus; // Cập nhật trạng thái

            if (req.Notes != null)
                e.Notes = req.Notes;

            if (req.ImgUrlBefore != null)
                e.ImgUrlBefore = _uploadPhotoService.UploadPhoto(req.ImgUrlBefore);

            if (req.ImgUrlAfter != null)
                e.ImgUrlAfter = _uploadPhotoService.UploadPhoto(req.ImgUrlAfter);

            _deliveryRepository.Update(e);
            await _deliveryRepository.SaveChangesAsync();

            // === BƯỚC 3: LOGIC XÓA KHO KHI "UPDATE" ===
            // Chỉ chạy khi trạng thái MỚI là "Delivered" và trạng thái CŨ KHÔNG PHẢI là "Delivered"
            if (newStatus.Equals("Delivered", StringComparison.OrdinalIgnoreCase) &&
                !oldStatus.Equals("Delivered", StringComparison.OrdinalIgnoreCase))
            {
                await RemoveVehicleFromInventoryAsync(e.OrderId);
            }

            return MapToRespons(e);
        }
        private async Task RemoveVehicleFromInventoryAsync(int orderId)
        {
            _logger.LogInformation("Order {OrderId} is 'Delivered'. Processing vehicle removal from agency inventory...", orderId);

            // 1. Tìm TẤT CẢ OrderDetails của đơn hàng
            var orderDetailsList = await _orderDetailRepository.GetByOrderIdAsync(orderId);

            if (orderDetailsList == null || !orderDetailsList.Any())
            {
                _logger.LogError("Không tìm thấy OrderDetail cho OrderID {OrderId}. Không thể xóa kho.", orderId);
                return;
            }

            _logger.LogInformation("Found {Count} vehicle(s) in OrderID {OrderId}. Starting gRPC calls...", orderDetailsList.Count(), orderId);

            // 2. Lặp qua TỪNG OrderDetail (mỗi detail là một xe)
            foreach (var detail in orderDetailsList)
            {
                // Bọc mỗi lệnh gọi gRPC trong try-catch riêng
                // để nếu 1 xe lỗi, các xe khác vẫn được xử lý
                try
                {
                    // 3. Tìm Quotation tương ứng của xe đó
                    var quotation = await _quotationRepository.GetByIdAsync(detail.QuotationId);

                    if (quotation == null)
                    {
                        _logger.LogWarning("Skipping OrderDetail {DetailId}: Cannot find matching Quotation {QuotationId}.", detail.Id, detail.QuotationId);
                        continue; // Bỏ qua item này, tiếp tục vòng lặp
                    }

                    // 4. Gọi gRPC đến DealerAPIService cho TỪNG xe
                    _logger.LogInformation("Calling gRPC RemoveVehicleFromInventory (Agency: {AgencyId}, VehicleInstance: {VehicleInstanceId})",
                        quotation.AgencyId, quotation.VehicleInstanceId);

                    var reply = await _agencyGrpcServiceClient.RemoveVehicleFromInventoryAsync(
                        quotation.AgencyId,
                        quotation.VehicleInstanceId
             );

                    _logger.LogInformation("gRPC Reply for VehicleInstance {VehicleInstanceId}: {Message}",
                        quotation.VehicleInstanceId, reply.Message);
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi cho item cụ thể này và TIẾP TỤC vòng lặp
                    _logger.LogError(ex, "Failed to remove VehicleInstance (from Quotation {QuotationId}) for OrderID {OrderId}. Manual check required.",
                        detail.QuotationId, orderId);

                    // Không ném lỗi ra ngoài, để các xe khác tiếp tục
                }
            }
        }
        public async Task DeleteAsync(int id)
        {
            var delivery = await _deliveryRepository.GetByIdAsync(id);
            if (delivery == null)
                throw new KeyNotFoundException("Delivery not found.");
             _deliveryRepository.Remove(delivery);
            await _deliveryRepository.SaveChangesAsync();
        }
        public async Task<IEnumerable<DeliveryResponse>> GetByAgencyAsync(int agencyId)
        {
            var deliveries = await _deliveryRepository.GetByAgencyIdAsync(agencyId);
            return deliveries.Select(MapToRespons).ToList();
        }

        private static DeliveryResponse MapToRespons(Delivery e) => new()
        {
            Id = e.Id,
            OrderId = e.OrderId,
            DeliveryDate = e.DeliveryDate,
            DeliveryStatus = e.DeliveryStatus,
            Notes = e.Notes,
            ImgUrlBefore = e.ImgUrlBefore,
            ImgUrlAfter = e.ImgUrlAfter
        };
    }
}
