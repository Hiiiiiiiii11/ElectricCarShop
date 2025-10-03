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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AgencyContracts>()
                .HasOne(c => c.Agency)
                .WithMany(d => d.Contracts)
                .HasForeignKey(c => c.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AgencyTargets>()
                .HasOne(t => t.Agency)
                .WithMany(d => d.Targets)
                .HasForeignKey(t => t.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AgencyInventory>()
                .HasOne(i => i.Agency)
                .WithMany(d => d.Inventories)
                .HasForeignKey(i => i.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TestDrive>()
                 .HasOne(t => t.Agency)
                 .WithMany(d => d.TestDrives)
                 .HasForeignKey(t => t.AgencyId)
                 .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<AgencyDebts>()
                 .HasOne(t => t.Agency)
                 .WithMany(d => d.Debts)
                 .HasForeignKey(t => t.AgencyId)
                 .OnDelete(DeleteBehavior.Restrict);
        }


    }
}
//dotnet ef migrations add InitialCreate --project DealerRepository --startup-project DealerAPI --context AgencyDbContext
//dotnet ef database update --project DealerRepository --startup-project DealerAPI --context AgencyDbContext