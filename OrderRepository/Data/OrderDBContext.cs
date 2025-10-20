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
    public class OrderDbContext :DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options)
        {
        }
        public DbSet<Orders> Orders { get; set; }
        public DbSet<Contracts> Contracts { get; set; }
        public DbSet<Payments> Payments { get; set; }
        public DbSet<Customers> Customers { get; set; }
        public DbSet<Feedback> Feedbacks { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Quotations> Quotations { get; set; }
        public DbSet<OrderDetail> OrderDetail { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Customers ↔ Orders
            modelBuilder.Entity<Orders>()
                .HasOne(o => o.Customer) 
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Customer)
    .WithMany(c => c.Feedbacks)
    .HasForeignKey(f => f.CustomerId)
    .OnDelete(DeleteBehavior.Restrict);

            
            // 🔹 Orders ↔ OrderDetails
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Orders)
                .WithMany(o => o.Details)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // 🔹 Quotations ↔ OrderDetails
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Quotation)
                .WithMany(q => q.OrderDetails)
                .HasForeignKey(od => od.QuotationId)
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