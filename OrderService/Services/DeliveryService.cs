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

        public DeliveryService(IDeliveryRepository deliveryRepository, IUploadPhotoService uploadPhotoService)
        {
            _deliveryRepository = deliveryRepository;
            _uploadPhotoService = uploadPhotoService;
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

            return MapToRespons(e);
        }

        public async Task<DeliveryResponse> UpdateAsync(int id, DeliveryUpdateRequest req)
        {
            var e = await _deliveryRepository.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Delivery not found.");

            if (req.DeliveryDate.HasValue)
                e.DeliveryDate = req.DeliveryDate.Value;

            if (!string.IsNullOrWhiteSpace(req.DeliveryStatus))
                e.DeliveryStatus = req.DeliveryStatus.Trim();

            if (req.Notes != null)
                e.Notes = req.Notes;

            // ✅ Upload new image if provided
            if (req.ImgUrlBefore != null)
                e.ImgUrlBefore = _uploadPhotoService.UploadPhoto(req.ImgUrlBefore);

            if (req.ImgUrlAfter != null)
                e.ImgUrlAfter = _uploadPhotoService.UploadPhoto(req.ImgUrlAfter);

            _deliveryRepository.Update(e);
            await _deliveryRepository.SaveChangesAsync();

            return MapToRespons(e);
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
