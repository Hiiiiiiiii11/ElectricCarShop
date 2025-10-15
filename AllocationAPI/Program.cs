using AllocationAPIService.Services;
using AllocationRepository.Data;
using AllocationRepository.Repositories;
using AllocationService.Services;
using GrpcService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Share.Setting;
using Share.ShareServices;
using System.Text;

namespace AllocationAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(5000, o => o.Protocols = HttpProtocols.Http1);
                options.ListenAnyIP(5001, o => o.Protocols = HttpProtocols.Http2);
            });

            builder.Services.AddDbContext<AllocationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AllocationDbConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

            builder.Services.AddScoped<IAllocationRepository, AllocationRepository.Repositories.AllocationRepository>();
            builder.Services.AddScoped<IEVInventoryRepository, EVInventoryRepository>();
            builder.Services.AddScoped<IVehicleOptionRepository, VehicleOptionRepository>();
            builder.Services.AddScoped<IVehiclePriceRepository, VehiclePriceRepository>();
            builder.Services.AddScoped<IVehiclePromotionRepository, VehiclePromotionRepository>();
            builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();
            builder.Services.AddScoped<IAgencyGrpcServiceClient, AgencyGrpcServiceClient>();
            builder.Services.AddScoped<IAllocationService, AllocationService.Services.AllocationService>();
            builder.Services.AddScoped<IEVInventoryService, EVInventoryService>();
            builder.Services.AddScoped<IVehiclePriceService, VehiclePriceService>();
            builder.Services.AddScoped<IVehicleService, VehicleService>();
            builder.Services.AddScoped<IVehicleOptionService, VehicleOptionService>();
            builder.Services.AddScoped<IVehiclePromotionService, VehiclePromotionService>();

            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
            builder.Services.AddSingleton(jwtSettings);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Allocation API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter JWT."
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policyBuilder =>
                {
                    policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                });
            });

            var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(key)
                    };
                });

            builder.Services.AddGrpc();

            var agencyServiceUrl = builder.Environment.IsDevelopment()
                ? "https://localhost:7198"
                : "http://agencyapi:80";
            builder.Services.AddGrpcClient<AgencyGrpcService.AgencyGrpcServiceClient>(o =>
            {
                o.Address = new Uri(agencyServiceUrl);
            });

            var app = builder.Build();

            app.UseForwardedHeaders();

            if (app.Environment.IsProduction())
            {
                Thread.Sleep(TimeSpan.FromSeconds(15));
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    try
                    {
                        var dbContext = services.GetRequiredService<AllocationDbContext>();
                        var defaultConnStr = builder.Configuration.GetConnectionString("AllocationDbConnection");
                        var dbName = new SqlConnectionStringBuilder(defaultConnStr).InitialCatalog;
                        var masterConnStr = defaultConnStr.Replace($"Database={dbName}", "Database=master");

                        using (var connection = new SqlConnection(masterConnStr))
                        {
                            connection.Open();
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = $"IF DB_ID('{dbName}') IS NULL CREATE DATABASE [{dbName}]";
                                command.ExecuteNonQuery();
                            }
                            logger.LogInformation("✅ Step 1/2: Database '{DbName}' created or already exists.", dbName);
                        }

                        dbContext.Database.EnsureCreated();
                        logger.LogInformation("✅ Step 2/2: Schema has been created successfully.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "❌ An error occurred during database setup.");
                    }
                }
            }

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Allocation API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapGrpcService<VehicleGrpcServiceImpl>();

            app.Run();
        }
    }
}

