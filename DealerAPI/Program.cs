using AgencyRepository.Data;
using AgencyRepository.Repositories;
using AgencyService.Services;
using GrpcService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Share.Setting;
using Share.ShareServices;
using System.Text;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace AgencyAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // =================== REVERSE PROXY ===================
            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            if (builder.Environment.IsProduction())
            {
                builder.WebHost.ConfigureKestrel(options =>
                {
                    options.ListenAnyIP(80, o =>
                    {
                        o.Protocols = HttpProtocols.Http1AndHttp2;
                    });
                });
            }
            // =================== DB ===================
            builder.Services.AddDbContext<AgencyDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AgencyDbConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null);
                }));

            // =================== Cloudinary ===================
            builder.Services.Configure<CloudDinarySetting>(
                builder.Configuration.GetSection("CloudinarySettings"));
            builder.Services.AddSingleton(provider =>
            {
                var config = builder.Configuration.GetSection("CloudinarySettings").Get<CloudDinarySetting>();
                var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
                return new Cloudinary(account);
            });
            builder.Services.AddScoped<IUploadPhotoService, UpLoadPhotoService>();

            // =================== JWT ===================
            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
            builder.Services.AddSingleton(jwtSettings);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    var key = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
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

            // =================== Services ===================
            builder.Services.AddScoped<IAgencyRepository, AgencyRepository.Repositories.AgencyRepository>();
            builder.Services.AddScoped<IAgencyContractRepository, AgencyContractRepository>();
            builder.Services.AddScoped<IAgencyDebtRepository, AgencyDebtRepository>();
            builder.Services.AddScoped<IAgencyTargetRepository, AgencyTargetRepository>();
            builder.Services.AddScoped<ITestDriveRepository, TestDriveRepository>();
            builder.Services.AddScoped<IAgencyInventoryRepository, AgencyInventoryRepository>();

            builder.Services.AddScoped<IAgencyService, AgencyService.Services.AgencyService>();
            builder.Services.AddScoped<IAgencyContractService, AgencyContractService>();
            builder.Services.AddScoped<IAgencyDebtService, AgencyDebtService>();
            builder.Services.AddScoped<IAgencyTargetService, AgencyTargetService>();
            builder.Services.AddScoped<IAgencyInventoryService, AgencyInventoryService>();
            builder.Services.AddScoped<ITestDriveService, TestDriveService>();

            builder.Services.AddScoped<IUserGrpcServiceClient, UserGrpcServiceClient>();
            builder.Services.AddScoped<IVehicleGrpcServiceClient, VehicleGrpcServiceClient>();
            builder.Services.AddScoped<ICustomerGrpcServiceClient, CustomerGrpcServiceClient>();

            // =================== gRPC ===================
            builder.Services.AddGrpc();

            var userServiceUrl = builder.Environment.IsDevelopment()
                ? "https://localhost:7022"
                : "http://userapi:80";
            var vehicleServiceUrl = builder.Environment.IsDevelopment()
                ? "https://localhost:7055"
                : "http://allocationapi:80";
            var customerServiceUrl = builder.Environment.IsDevelopment()
                ? "https://localhost:7114"
                : "http://orderapi:80";

            builder.Services.AddGrpcClient<UserGrpcService.UserGrpcServiceClient>(o =>
            {
                o.Address = new Uri(userServiceUrl);
            });
            builder.Services.AddGrpcClient<VehicleGrpcService.VehicleGrpcServiceClient>(o =>
            {
                o.Address = new Uri(vehicleServiceUrl);
            });
            builder.Services.AddGrpcClient<CustomerGrpcService.CustomerGrpcServiceClient>(o =>
            {
                o.Address = new Uri(customerServiceUrl);
            });

            // =================== Controllers & Swagger ===================
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Agency API", Version = "v1" });
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

            var app = builder.Build();
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            app.UseForwardedHeaders();

            // =================== DB Auto Create ===================
            if (app.Environment.IsProduction())
            {
                Thread.Sleep(TimeSpan.FromSeconds(15));
                using var scope = app.Services.CreateScope();
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                try
                {
                    var dbContext = services.GetRequiredService<AgencyDbContext>();
                    var defaultConnStr = builder.Configuration.GetConnectionString("AgencyDbConnection");
                    var dbName = new SqlConnectionStringBuilder(defaultConnStr).InitialCatalog;
                    var masterConnStr = defaultConnStr.Replace($"Database={dbName}", "Database=master");

                    using var connection = new SqlConnection(masterConnStr);
                    connection.Open();
                    using var command = connection.CreateCommand();
                    command.CommandText = $"IF DB_ID('{dbName}') IS NULL CREATE DATABASE [{dbName}]";
                    command.ExecuteNonQuery();
                    logger.LogInformation("✅ Database '{DbName}' created or already exists.", dbName);

                    dbContext.Database.EnsureCreated();
                    logger.LogInformation("✅ Schema created successfully.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "❌ Error during database setup.");
                }
            }

            if (app.Environment.IsDevelopment() || app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Agency API V1");
                    c.RoutePrefix = string.Empty;
                });
            }

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapGrpcService<AgencyGrpcServiceImpl>();
            app.Run();
        }
    }
}
