using AllocationRepository.Model;
using Microsoft.EntityFrameworkCore;
using OrderRepository.Model;
// Tránh đụng .NET Transaction
using TransactionEntity = OrderRepository.Model.Transaction;

namespace OrderRepository.Data
{
    public class OrderDbContext : DbContext
    {
        public OrderDbContext(DbContextOptions<OrderDbContext> options) : base(options) { }

        public DbSet<Orders> Orders { get; set; } = default!;
        public DbSet<Contracts> Contracts { get; set; } = default!;
        public DbSet<Payments> Payments { get; set; } = default!;
        public DbSet<Customers> Customers { get; set; } = default!;
        public DbSet<Feedback> Feedbacks { get; set; } = default!;
        public DbSet<TransactionEntity> Transactions { get; set; } = default!;
        public DbSet<Quotations> Quotations { get; set; } = default!;
        public DbSet<OrderDetail> OrderDetail { get; set; } = default!;
        public DbSet<Delivery> Deliveries { get; set; } = default!;
        public DbSet<InstallmentPlans> InstallmentPlans { get; set; } = default!;
        public DbSet<InstallmentItems> InstallmentItems { get; set; } = default!;
        public DbSet<InstallmentPayments> InstallmentPayments { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Customers ↔ Orders (1 - n)
            modelBuilder.Entity<Orders>()
                .HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customers ↔ Feedbacks (1 - n)
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Customer)
                .WithMany(c => c.Feedbacks)
                .HasForeignKey(f => f.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Orders ↔ OrderDetails (1 - n)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Orders)
                .WithMany(o => o.Details)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Quotations ↔ OrderDetails (1 - n)
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Quotation)
                .WithMany(q => q.OrderDetails)
                .HasForeignKey(od => od.QuotationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Customers ↔ Quotations (1 - n)
            modelBuilder.Entity<Quotations>()
                .HasOne(q => q.Customer)
                .WithMany(c => c.Quotations)
                .HasForeignKey(q => q.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Quotations ↔ Contracts (1 - n)
            modelBuilder.Entity<Contracts>()
                .HasOne(c => c.Quotation)
                .WithMany(q => q.Contracts)
                .HasForeignKey(c => c.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Deliveries mapping
            modelBuilder.Entity<Delivery>(e =>
            {
                e.ToTable("Deliveries");
                e.HasKey(d => d.Id);

                e.Property(d => d.OrderId).IsRequired();
                e.Property(d => d.DeliveryDate).IsRequired();

                e.Property(d => d.DeliveryStatus)
                    .HasMaxLength(50)
                    .IsRequired(); // Manager sẽ nhập, không set default.

                e.Property(d => d.Notes).HasMaxLength(2000);
                e.Property(d => d.ImgUrlBefore).HasMaxLength(500);
                e.Property(d => d.ImgUrlAfter).HasMaxLength(500);

                e.HasIndex(d => new { d.OrderId, d.DeliveryDate });

                // FK -> Orders (không cần navigation property)
                e.HasOne<Orders>()
                 .WithMany()
                 .HasForeignKey(d => d.OrderId)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // (Tuỳ chọn) thiết lập precision cho tiền
            modelBuilder.Entity<Payments>(e =>
            {
                e.Property(p => p.Amount).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<TransactionEntity>(e =>
            {
                e.Property(t => t.Amount).HasColumnType("decimal(18,2)");
                e.HasIndex(t => t.TransactionCode).IsUnique();
            });
            modelBuilder.Entity<InstallmentPlans>(e =>
            {
                // 1. Quan hệ (Contracts 1 -> n InstallmentPlans)
                e.HasOne(p => p.Contract)              // Một Plan có một Contract
                 .WithMany(c => c.Installments)      // Một Contract có nhiều Plan
                 .HasForeignKey(p => p.ContractId)     // Khóa ngoại
                 .OnDelete(DeleteBehavior.Restrict); // Không cho xóa Hợp đồng nếu còn Kế hoạch trả góp

                // 2. RÀNG BUỘC CHECK (Logic quan trọng)
                // Đảm bảo chỉ 1 trong 2 ContractId hoặc AgencyContractId có giá trị
                e.HasCheckConstraint("CK_InstallmentPlans_ContractType",
                    "([ContractId] IS NOT NULL AND [AgencyContractId] IS NULL) OR ([ContractId] IS NULL AND [AgencyContractId] IS NOT NULL)");

                // 3. Cấu hình tiền tệ
                e.Property(p => p.PrincipalAmount).HasColumnType("decimal(18,2)");
                e.Property(p => p.DepositAmount).HasColumnType("decimal(18,2)");
                e.Property(p => p.InterestRate).HasColumnType("decimal(5,2)"); // Ví dụ: 12.50%
            });

            modelBuilder.Entity<InstallmentItems>(e =>
            {
                // 1. Quan hệ (InstallmentPlans 1 -> n InstallmentItems)
                e.HasOne(i => i.InstallmentPlans)         // Một Item thuộc một Plan
                 .WithMany(p => p.Items)               // Một Plan có nhiều Item
                 .HasForeignKey(i => i.InstallmentPlanId) // Khóa ngoại
                 .OnDelete(DeleteBehavior.Cascade);   // Xóa Plan thì xóa luôn các Item

                // 2. Cấu hình tiền tệ
                e.Property(i => i.Percentage).HasColumnType("decimal(5,2)"); // Ví dụ: 50.00%
                e.Property(i => i.AmountDue).HasColumnType("decimal(18,2)");
                e.Property(i => i.PrincipalComponent).HasColumnType("decimal(18,2)");
                e.Property(i => i.InterestComponent).HasColumnType("decimal(18,2)");
                e.Property(i => i.FeeComponent).HasColumnType("decimal(18,2)");
            });
        }
    }
}

//dotnet ef migrations add InitialCreate1 --project OrderRepository --startup-project OrderAPI --context OrderDbContext
//dotnet ef database update --project OrderRepository --startup-project OrderAPI --context OrderDbContext
