using AllocationRepository.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Data
{
    public class AllocationDbContext :DbContext
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

            modelBuilder.Entity<Vehicles>()
         .HasMany(v => v.VehicleOptions)
         .WithMany(o => o.Vehicles)
         .UsingEntity<Dictionary<string, object>>(
             "VehicleVehicleOption", // tên bảng trung gian
             j => j.HasOne<VehicleOptions>().WithMany().HasForeignKey("VehicleOptionId"),
             j => j.HasOne<Vehicles>().WithMany().HasForeignKey("VehicleId"),
             j =>
             {
                 j.HasKey("VehicleId", "VehicleOptionId");
                 j.ToTable("VehicleVehicleOptions");
             });

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
                .HasForeignKey(a => a.VehicleId);

            // EVInventory -> Allocations
            modelBuilder.Entity<Allocations>()
                .HasOne(a => a.EVInventory)
                .WithMany(e => e.Allocations)
                .HasForeignKey(a => a.EvInventoryId);

            // Vehicle -> Quotations
            modelBuilder.Entity<Quotations>()
                .HasOne(q => q.Vehicle)
                .WithMany(v => v.Quotations)
                .HasForeignKey(q => q.VehicleId);
        }

    }
}
