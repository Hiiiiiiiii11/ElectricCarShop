using OrderRepository.Model.Request;

namespace OrderService.Services
{
    public interface IDeliveryService
    {
        Task<DeliveryDto?> GetAsync(int id);
        Task<IReadOnlyList<DeliveryDto>> GetByOrderAsync(int orderId);
        Task<(IReadOnlyList<DeliveryDto> items, int total)> ListAsync(
            string? status, DateTime? from, DateTime? to, int page, int pageSize);

        Task<DeliveryDto> CreateAsync(DeliveryCreateRequest req);
        Task<DeliveryDto> UpdateAsync(int id, DeliveryUpdateRequest req);
        Task<bool> DeleteAsync(int id);
    }
}
