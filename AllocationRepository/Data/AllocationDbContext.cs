using AllocationRepository.Model;
using Microsoft.EntityFrameworkCore;

namespace AllocationRepository.Data
{
    public class AllocationDbContext : DbContext
    {
        public AllocationDbContext(DbContextOptions<AllocationDbContext> options) : base(options) { }

        public DbSet<Allocations> Allocations { get; set; }
        public DbSet<EVInventory> EVInventories { get; set; }
        public DbSet<Quotations> Quotations { get; set; }
        public DbSet<VehicleOptions> VehicleOptions { get; set; }
        public DbSet<Vehicles> Vehicles { get; set; }
        public DbSet<VehiclePrices> VehiclePrices { get; set; }
        public DbSet<VehiclePromotions> VehiclePromotions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Vehicle -> EVInventory
            modelBuilder.Entity<EVInventory>()
                .HasOne(e => e.Vehicle)
                .WithMany(v => v.EVInventories)
                .HasForeignKey(e => e.VehicleId);

            // VehicleOption -> Vehicle
            modelBuilder.Entity<Vehicles>()
                .HasOne(v => v.VehicleOption)
                .WithMany(o => o.Vehicles)
                .HasForeignKey(v => v.VehicleOptionId)
                .OnDelete(DeleteBehavior.Restrict); // tránh multiple cascade paths

            // Vehicle -> VehiclePrices
            modelBuilder.Entity<VehiclePrices>()
                .HasOne(p => p.Vehicle)
                .WithMany(v => v.VehiclePrices)
                .HasForeignKey(p => p.VehicleId);

            // Vehicle -> VehiclePromotions
            modelBuilder.Entity<VehiclePromotions>()
                .HasOne(p => p.Vehicle)
                .WithMany(v => v.VehiclePromotions)
                .HasForeignKey(p => p.VehicleId);

            // Vehicle -> Allocations
            modelBuilder.Entity<Allocations>()
                .HasOne(a => a.Vehicle)
                .WithMany(v => v.Allocations)
                .HasForeignKey(a => a.VehicleId)
                .OnDelete(DeleteBehavior.Restrict); // tránh cycle

            // EVInventory -> Allocations
            modelBuilder.Entity<Allocations>()
                .HasOne(a => a.EVInventory)
                .WithMany(e => e.Allocations)
                .HasForeignKey(a => a.EvInventoryId)
                .OnDelete(DeleteBehavior.Cascade); // cho phép cascade

            // Vehicle -> Quotations
            modelBuilder.Entity<Quotations>()
                .HasOne(q => q.Vehicle)
                .WithMany(v => v.Quotations)
                .HasForeignKey(q => q.VehicleId);
        }
    }
}

//dotnet ef migrations add InitialCreate --project AllocationRepository --startup-project AllocationAPI --context AllocationDbContext
//dotnet ef database update --project AllocationRepository --startup-project AllocationAPI --context AllocationDbContext
