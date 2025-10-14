using AllocationRepository.Model;
using Microsoft.EntityFrameworkCore;
using OrderRepository.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Data
{
    public class OrderDBContext :DbContext
    {
        public OrderDBContext(DbContextOptions<OrderDBContext> options) : base(options)
        {
        }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Contracts> Contracts { get; set; }
        public DbSet<Payments> Payments { get; set; }
        public DbSet<Customers> Customers { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Quotations> Quotations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Customers ↔ Orders
            modelBuilder.Entity<Orders>()
                .HasOne<Customers>()
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
    .HasOne(f => f.Customer)
    .WithMany(c => c.Feedbacks)
    .HasForeignKey(f => f.CustomerId)
    .OnDelete(DeleteBehavior.Restrict);

            // Quotations ↔ Orders
            modelBuilder.Entity<Orders>()
                .HasOne<Quotations>()
                .WithMany()
                .HasForeignKey(o => o.QuotationId)
                .OnDelete(DeleteBehavior.Restrict);


            // Giữ lại duy nhất dòng này:
            modelBuilder.Entity<Quotations>()
                .HasOne(q => q.Customer)
                .WithMany(c => c.Quotations)
                .HasForeignKey(q => q.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Contracts>()
                .HasOne(c => c.Quotation)
                .WithMany(q => q.Contracts)
                .HasForeignKey(c => c.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);
        }


    }
}
//dotnet ef migrations add InitialCreate --project OrderRepository --startup-project OrderAPI --context OrderDbContext
//dotnet ef database update --project OrderRepository --startup-project OrderAPI --context OrderDbContext