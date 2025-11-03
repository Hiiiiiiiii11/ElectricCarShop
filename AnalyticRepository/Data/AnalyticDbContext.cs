using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AnalyticRepository.Model;

namespace AnalyticRepository.Data
{
    public class AnalyticDbContext : DbContext
    {
        public AnalyticDbContext(DbContextOptions<AnalyticDbContext> options) : base(options) { }

        public DbSet<Monthly_Demand_Features> MonthlyDemandFeatures { get; set; }
        public DbSet<EtlLogs> EtlLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Khởi tạo 1 dòng log duy nhất
            modelBuilder.Entity<EtlLogs>().HasData(
                new EtlLogs { Id = 1, IsRunning = false, LastStatusMessage = "Service starting." }
            );
        }
       
    }
}
//dotnet ef migrations add InitialCreate --project AnalyticRepository --startup-project AnalyticAPI --context AnalyticDbContext
//dotnet ef database update --project AnalyticRepository --startup-project AnalyticAPI --context AnalyticDbContext