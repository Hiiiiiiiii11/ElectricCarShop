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
        DbSet<Orders> Orders { get; set; }
        DbSet<Contracts> Contracts { get; set; }
        DbSet<Payments> Payments { get; set; }
        DbSet<Customers> Customers { get; set; }

    }
}
//dotnet ef migrations add InitialCreate --project OrderRepository --startup-project OrderAPI --context OrderDbContext
//dotnet ef database update --project OrderRepository --startup-project OrderAPI --context OrderDbContext