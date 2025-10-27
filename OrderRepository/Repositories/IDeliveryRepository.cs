using OrderRepository.Model;
using Share.ShareRepo;

namespace OrderRepository.Repositories
{
    public interface IDeliveryRepository:IGenericRepository<Delivery>
    {
        Task<List<Delivery>> GetByOrderAsync(int orderId);
        Task<List<Delivery>> GetByAgencyIdAsync(int agencyId);

        Task<(List<Delivery> items, int total)> FilterAsync(
            string? status, DateTime? from, DateTime? to, int skip, int take);
    }
}
