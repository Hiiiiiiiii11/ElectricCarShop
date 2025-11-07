using OrderRepository.Model.OrderDTO;

namespace OrderService.Services
{
    public interface IDeliveryService
    {
        Task<DeliveryResponse?> GetByIdAsync(int id);
        Task<IEnumerable<DeliveryResponse>> GetByOrderIdAsync(int orderId);
        Task<(IEnumerable<DeliveryResponse> items, int total)> ListAsync(
            string? status, DateTime? from, DateTime? to, int page, int pageSize);
        Task<IEnumerable<DeliveryResponse>> GetByAgencyAsync(int agencyId);

        Task<DeliveryResponse> CreateAsync(DeliveryCreateRequest req);
        Task<DeliveryResponse> UpdateAsync(int id, DeliveryUpdateRequest req);
        Task DeleteAsync(int id);
    }
}
