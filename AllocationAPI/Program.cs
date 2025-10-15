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
using System.Security.Cryptography.X509Certificates; // Thêm using này
using System.Text;

namespace AllocationAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            if (builder.Environment.IsProduction())
            {
                // Dòng này không còn cần thiết khi dùng HTTPS
                // AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
            }

            builder.Services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });

            if (builder.Environment.IsProduction())
            {
                var pfxPassword = builder.Configuration["Kestrel:CertificatePassword"];

                builder.WebHost.ConfigureKestrel(options =>
                {
                    // Endpoint cho REST API (HTTP/1.1) từ Nginx
                    options.ListenAnyIP(80, o => o.Protocols = HttpProtocols.Http1);

                    // Endpoint MỚI cho gRPC nội bộ (HTTP/2 qua HTTPS)
                    options.ListenAnyIP(443, o =>
                    {
                        o.Protocols = HttpProtocols.Http2;
                        o.UseHttps("/https/certs/allocationapi.pfx", pfxPassword);
                    });
                });
            }

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
                : "https://agencyapi:443"; // <-- SỬA URL

            // --- BỔ SUNG PHẦN CẤU HÌNH CLIENT SSL ---
            var handler = new HttpClientHandler();
            var caCert = new X509Certificate2("/https/certs/ca.crt");
            handler.ServerCertificateCustomValidationCallback = (message, serverCert, chain, errors) =>
            {
                if (serverCert == null) return false;
                using var customChain = new X509Chain();
                customChain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                customChain.ChainPolicy.CustomTrustStore.Add(caCert);
                return customChain.Build(serverCert);
            };
            // --- KẾT THÚC PHẦN BỔ SUNG ---

            builder.Services.AddGrpcClient<AgencyGrpcService.AgencyGrpcServiceClient>(o =>
            {
                o.Address = new Uri(agencyServiceUrl);
            }).ConfigurePrimaryHttpMessageHandler(() => handler); // <-- SỬ DỤNG HANDLER ĐÃ CẤU HÌNH

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
