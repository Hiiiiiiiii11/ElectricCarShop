using OrderRepository.Model;

namespace OrderRepository.Repositories
{
    public interface IDeliveryRepository
    {
        Task<Delivery?> GetByIdAsync(int id);
        Task<List<Delivery>> GetByOrderAsync(int orderId);

        Task<(List<Delivery> items, int total)> FilterAsync(
            string? status, DateTime? from, DateTime? to, int skip, int take);

        Task<Delivery> AddAsync(Delivery entity);
        Task UpdateAsync(Delivery entity);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
