using Microsoft.EntityFrameworkCore;
using OrderRepository.Data;
using OrderRepository.Model;
using Share.ShareRepo;

namespace OrderRepository.Repositories
{
    public class DeliveryRepository : GenericRepository<Delivery>, IDeliveryRepository
    {
        private readonly OrderDbContext _context;
        public DeliveryRepository(OrderDbContext context) : base(context)
        {
            _context = context;
        }



        public Task<List<Delivery>> GetByOrderAsync(int orderId) =>
            _context.Deliveries
               .AsNoTracking()
               .Where(d => d.OrderId == orderId)
               .OrderByDescending(d => d.DeliveryDate)
               .ToListAsync();

        public async Task<(List<Delivery> items, int total)> FilterAsync(
            string? status, DateTime? from, DateTime? to, int skip, int take)
        {
            var q = _context.Deliveries.AsNoTracking().AsQueryable();

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
        public Task<bool> ExistsAsync(int id) =>
            _context.Deliveries.AnyAsync(d => d.Id == id);
        public async Task<List<Delivery>> GetByAgencyIdAsync(int agencyId)
        {
            return await _context.Deliveries
                .Where(d => d.OrderId != null)
                .Join(_context.Orders,
                      d => d.OrderId,
                      o => o.Id,
                      (d, o) => new { d, o })
                .Join(_context.OrderDetail,
                      x => x.o.Id,
                      od => od.OrderId,
                      (x, od) => new { x.d, x.o, od })
                .Join(_context.Quotations,
                      x => x.od.QuotationId,
                      q => q.Id,
                      (x, q) => new { x.d, q })
                .Where(x => x.q.AgencyId == agencyId)
                .Select(x => x.d)
                .Distinct()
                .ToListAsync();
        }

    }
}
