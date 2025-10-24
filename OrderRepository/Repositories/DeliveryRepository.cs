using Microsoft.EntityFrameworkCore;
using OrderRepository.Data;
using OrderRepository.Model;

namespace OrderRepository.Repositories
{
    public class DeliveryRepository : IDeliveryRepository
    {
        private readonly OrderDbContext _db;
        public DeliveryRepository(OrderDbContext db) => _db = db;

        public Task<Delivery?> GetByIdAsync(int id) =>
            _db.Deliveries
               .AsNoTracking()
               .FirstOrDefaultAsync(d => d.Id == id);

        public Task<List<Delivery>> GetByOrderAsync(int orderId) =>
            _db.Deliveries
               .AsNoTracking()
               .Where(d => d.OrderId == orderId)
               .OrderByDescending(d => d.DeliveryDate)
               .ToListAsync();

        public async Task<(List<Delivery> items, int total)> FilterAsync(
            string? status, DateTime? from, DateTime? to, int skip, int take)
        {
            var q = _db.Deliveries.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(d => d.DeliveryStatus == status);

            if (from.HasValue)
                q = q.Where(d => d.DeliveryDate >= from.Value);

            if (to.HasValue)
                q = q.Where(d => d.DeliveryDate <= to.Value);

            var total = await q.CountAsync();

            var items = await q.OrderByDescending(d => d.DeliveryDate)
                               .ThenByDescending(d => d.Id)
                               .Skip(skip)
                               .Take(take)
                               .ToListAsync();

            return (items, total);
        }

        public async Task<Delivery> AddAsync(Delivery entity)
        {
            _db.Deliveries.Add(entity);
            await _db.SaveChangesAsync();
            return entity;
        }

        public async Task UpdateAsync(Delivery entity)
        {
            // entity được service/upper layer nạp sẵn; cập nhật full
            _db.Deliveries.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing = await _db.Deliveries.FirstOrDefaultAsync(d => d.Id == id);
            if (existing == null) return false;

            _db.Deliveries.Remove(existing);
            await _db.SaveChangesAsync();
            return true;
        }

        public Task<bool> ExistsAsync(int id) =>
            _db.Deliveries.AnyAsync(d => d.Id == id);
    }
}
