using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserRepository.Model;

namespace UserRepository.Data
{
    public class UserDbContext : DbContext
    {
        // This class would typically inherit from DbContext and include DbSet properties for Users, Roles, and UserRoles.
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }
        // Initialize your database context here

        public DbSet<Users> Users { get; set; }
        public DbSet<Roles> Roles { get; set; }
        public DbSet<UserRoles> UserRoles { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UserRoles>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            // Quan hệ Roles - UserRoles
            modelBuilder.Entity<UserRoles>()
                .HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            // Unique constraint để tránh User có trùng Role
            modelBuilder.Entity<UserRoles>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId })
                .IsUnique();
        }
    }
}
//dotnet ef migrations add InitialCreate --project UserRepository --startup-project UserAPI --context UserDbContext
//dotnet ef database update --project UserRepository --startup-project UserAPI --context UserDbContext