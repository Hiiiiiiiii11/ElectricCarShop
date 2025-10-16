using AllocationRepository.Model;
using Microsoft.EntityFrameworkCore;

namespace AllocationRepository.Data
{
    public class AllocationDbContext : DbContext
    {
        public AllocationDbContext(DbContextOptions<AllocationDbContext> options) : base(options) { }

        public DbSet<Allocations> Allocations { get; set; }
        public DbSet<EVInventory> EVInventories { get; set; }
        public DbSet<VehicleOptions> VehicleOptions { get; set; }
        public DbSet<Vehicles> Vehicles { get; set; }
        public DbSet<VehiclePrices> VehiclePrices { get; set; }
        public DbSet<VehiclePromotions> VehiclePromotions { get; set; }
        public DbSet<VehicleInstance> VehicleInstances { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // === CÁC QUAN HỆ LIÊN QUAN ĐẾN VEHICLE (MẪU XE CHUNG) ===

            // VehicleOption -> Vehicle
            modelBuilder.Entity<Vehicles>()
                .HasOne(v => v.VehicleOption)
                .WithMany(o => o.Vehicles)
                .HasForeignKey(v => v.VehicleOptionId);

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

            // === CÁC QUAN HỆ MỚI LIÊN QUAN ĐẾN VEHICLE INSTANCE (XE CỤ THỂ) ===

            // 1. Vehicle -> VehicleInstance (Quan hệ cha-con chính)
            modelBuilder.Entity<VehicleInstance>()
                .HasOne(vi => vi.Vehicle)
                .WithMany(v => v.VehicleInstances)
                .HasForeignKey(vi => vi.VehicleId);

            // 2. VehicleInstance -> EVInventory
            modelBuilder.Entity<EVInventory>()
                .HasOne(e => e.VehicleInstance)
                .WithMany(vi => vi.EVInventories) // Thêm ICollection<EVInventory> vào model VehicleInstance
                .HasForeignKey(e => e.VehicleInstanceId);

            // 3. VehicleInstance -> Allocations
            modelBuilder.Entity<Allocations>()
                .HasOne(a => a.VehicleInstance)
                .WithMany(vi => vi.Allocations) // Thêm ICollection<Allocations> vào model VehicleInstance
                .HasForeignKey(a => a.VehicleInstanceId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. EVInventory -> Allocations
            modelBuilder.Entity<Allocations>()
                .HasOne(a => a.EVInventory)
                .WithMany(e => e.Allocations)
                .HasForeignKey(a => a.EvInventoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Bỏ AutoInclude không còn hợp lệ
            modelBuilder.Entity<Vehicles>()
                .Navigation(v => v.VehicleOption)
                .AutoInclude();
        }
    }
}

//dotnet ef migrations add InitialCreate --project AllocationRepository --startup-project AllocationAPI --context AllocationDbContext
//dotnet ef database update --project AllocationRepository --startup-project AllocationAPI --context AllocationDbContext
