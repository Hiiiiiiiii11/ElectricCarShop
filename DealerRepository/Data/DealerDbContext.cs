using DealerRepository.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Data
{
    public class DealerDbContext : DbContext
    {
        public DealerDbContext(DbContextOptions<DealerDbContext> options) : base(options) { }

        DbSet<Dealers> Dealers { get; set; }
        DbSet<DealerDebts>DealerDebts { get; set; }
        DbSet<DealerInventory> DealerInventories { get; set; }
        DbSet<DealerTargets>DealerTargets { get; set; }
        DbSet<DealerContracts>DealerContracts { get; set; }
        DbSet<DealerUser> DealerUsers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<DealerContracts>()
                .HasOne(c => c.Dealer)
                .WithMany(d => d.Contracts)
                .HasForeignKey(c => c.DealerId);

            modelBuilder.Entity<DealerDebts>()
                .HasOne(d => d.Dealer)
                .WithMany(p => p.Debts)
                .HasForeignKey(d => d.DealerId);

            modelBuilder.Entity<DealerTargets>()
                .HasOne(t => t.Dealer)
                .WithMany(d => d.Targets)
                .HasForeignKey(t => t.DealerId);

            modelBuilder.Entity<DealerInventory>()
                .HasOne(i => i.Dealer)
                .WithMany(d => d.Inventories)
                .HasForeignKey(i => i.DealerId);

            modelBuilder.Entity<DealerUser>()
                .HasOne(u => u.Dealer)
                .WithMany(d => d.DealerUsers)
                .HasForeignKey(u => u.DealerId);
        }

    }
}
