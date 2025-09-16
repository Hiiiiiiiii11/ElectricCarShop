
using DealerRepository.Data;
using DealerRepository.Repositories;
using Microsoft.EntityFrameworkCore;

namespace DealerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<DealerDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DealerDbConnection")));

            builder.Services.AddScoped<IDealerRepository,DealerRepository.Repositories.DealerRepository>();
            builder.Services.AddScoped<IDealerContractRepository, DealerContractRepository>();
            builder.Services.AddScoped<IDealerDebtRepository, DealerDebtRepository>();
            builder.Services.AddScoped<IDealerTargetRepository, DealerTargetRepository>();
            builder.Services.AddScoped<IDealerUserRepository, DealerUserRepository>();
            builder.Services.AddScoped<IDealerInventoryRepository, DealerInventoryRepository>();

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
