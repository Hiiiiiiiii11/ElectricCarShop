using OrderRepository.Model;
using OrderRepository.Model.Request;
using OrderRepository.Repositories;

namespace OrderService.Services
{
    public class DeliveryService : IDeliveryService
    {
        private readonly IDeliveryRepository _repo;

        public DeliveryService(IDeliveryRepository repo)
        {
            _repo = repo;
        }

        public async Task<DeliveryDto?> GetAsync(int id)
        {
            var e = await _repo.GetByIdAsync(id);
            return e == null ? null : Map(e);
        }

        public async Task<IReadOnlyList<DeliveryDto>> GetByOrderAsync(int orderId)
        {
            var list = await _repo.GetByOrderAsync(orderId);
            return list.Select(Map).ToList();
        }

        public async Task<(IReadOnlyList<DeliveryDto> items, int total)> ListAsync(
            string? status, DateTime? from, DateTime? to, int page, int pageSize)
        {
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            var (items, total) = await _repo.FilterAsync(
                status, from, to, (page - 1) * pageSize, pageSize);

            return (items.Select(Map).ToList(), total);
        }

        public async Task<DeliveryDto> CreateAsync(DeliveryCreateRequest req)
        {
            // Validate cơ bản
            if (string.IsNullOrWhiteSpace(req.DeliveryStatus))
                throw new InvalidOperationException("DeliveryStatus is required.");

            var e = new Delivery
            {
                OrderId = req.OrderId,
                DeliveryDate = req.DeliveryDate,
                DeliveryStatus = req.DeliveryStatus.Trim(),
                Notes = req.Notes,
                ImgUrlBefore = req.ImgUrlBefore,
                ImgUrlAfter = req.ImgUrlAfter
            };

            e = await _repo.AddAsync(e);
            return Map(e);
        }

        public async Task<DeliveryDto> UpdateAsync(int id, DeliveryUpdateRequest req)
        {
            var e = await _repo.GetByIdAsync(id)
                ?? throw new KeyNotFoundException("Delivery not found.");

            if (req.DeliveryDate.HasValue) e.DeliveryDate = req.DeliveryDate.Value;
            if (!string.IsNullOrWhiteSpace(req.DeliveryStatus)) e.DeliveryStatus = req.DeliveryStatus.Trim();
            if (req.Notes != null) e.Notes = req.Notes;
            if (req.ImgUrlBefore != null) e.ImgUrlBefore = req.ImgUrlBefore;
            if (req.ImgUrlAfter != null) e.ImgUrlAfter = req.ImgUrlAfter;

            await _repo.UpdateAsync(e);
            return Map(e);
        }

        public Task<bool> DeleteAsync(int id) => _repo.DeleteAsync(id);

        private static DeliveryDto Map(Delivery e) => new()
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
