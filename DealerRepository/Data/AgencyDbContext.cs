using AgencyRepository.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Data
{
    public class AgencyDbContext : DbContext
    {
        public AgencyDbContext(DbContextOptions<AgencyDbContext> options) : base(options) { }

        public DbSet<Agency> Agencys { get; set; }
        public DbSet<AgencyInventory> AgencyInventories { get; set; }
        public DbSet<AgencyTargets>AgencyTargets { get; set; }
        public DbSet<AgencyDebts> AgencyDebts { get; set; } 
        public DbSet<AgencyContracts>AgencyContracts { get; set; }
        public DbSet<TestDrive> TestDrives { get; set; }
        public DbSet<AgencyOrder> AgencyOrders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 🔹 AgencyContracts ↔ Agency
            modelBuilder.Entity<AgencyContracts>()
                .HasOne(c => c.Agency)
                .WithMany(a => a.Contracts)
                .HasForeignKey(c => c.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 AgencyTargets ↔ Agency
            modelBuilder.Entity<AgencyTargets>()
                .HasOne(t => t.Agency)
                .WithMany(a => a.Targets)
                .HasForeignKey(t => t.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 AgencyInventory ↔ Agency
            modelBuilder.Entity<AgencyInventory>()
                .HasOne(i => i.Agency)
                .WithMany(a => a.Inventories)
                .HasForeignKey(i => i.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 TestDrive ↔ Agency
            modelBuilder.Entity<TestDrive>()
                .HasOne(td => td.Agency)
                .WithMany(a => a.TestDrives)
                .HasForeignKey(td => td.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 AgencyDebts ↔ Agency
            modelBuilder.Entity<AgencyDebts>()
                .HasOne(d => d.Agency)
                .WithMany(a => a.Debts)
                .HasForeignKey(d => d.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 AgencyOrder ↔ Agency
            modelBuilder.Entity<AgencyOrder>()
                .HasOne(o => o.Agency)
                .WithMany(a => a.Orders)
                .HasForeignKey(o => o.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            // 🔹 AgencyOrder ↔ AgencyContracts
            modelBuilder.Entity<AgencyOrder>()
                .HasOne(o => o.AgencyContracts)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.AgencyContractId)
                .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
//dotnet ef migrations add InitialCreate --project DealerRepository --startup-project DealerAPI --context AgencyDbContext
//dotnet ef database update --project DealerRepository --startup-project DealerAPI --context AgencyDbContext