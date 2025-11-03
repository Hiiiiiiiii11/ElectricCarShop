
using AnalyticRepository.Data;
using AnalyticRepository.Repositories;
using AnalyticService.Services;
using CloudinaryDotNet;
using GrpcService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Share.Setting;
using Share.ShareServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AnalyticAPI
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

            builder.Services.AddDbContext<AnalyticDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("AnalyticDbConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));
            builder.Services.Configure<CloudDinarySetting>(
                builder.Configuration.GetSection("CloudDinarySetting"));
            builder.Services.AddSingleton(provider =>
            {
                var config = builder.Configuration.GetSection("CloudinarySettings").Get<CloudDinarySetting>();
                var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
                return new Cloudinary(account);
            });

            builder.Services.AddScoped<IEmailVerificationGrpcServiceClient, EmailVerificationGrpcServiceClient>();
            builder.Services.AddScoped<IUserGrpcServiceClient, UserGrpcServiceClient>();
            builder.Services.AddScoped<IAgencyGrpcServiceClient, AgencyGrpcServiceClient>();
            builder.Services.AddScoped<IVehicleInstanceGrpcServiceClient, VehicleInstanceGrpcServiceClient>();
            builder.Services.AddScoped<ICustomerGrpcServiceClient, CustomerGrpcServiceClient>();
            builder.Services.AddScoped<IOrderGrpcServiceClient, OrderGrpcServiceClient>();
            builder.Services.AddScoped<IAnalyticRepository, AnalyticRepository.Repositories.AnalyticRepository>();
            builder.Services.AddScoped<IAnalyticService, AnalyticService.Services.AnalyticsService>();
            builder.Services.AddHostedService<ETLWorkerService>();

            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
            builder.Services.AddSingleton(jwtSettings);
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Analytic API", Version = "v1" });
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
            builder.Services
                 .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                 .AddJwtBearer(options =>
                 {
                     options.RequireHttpsMetadata = false;
                     options.SaveToken = true;
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
                     options.Events = new JwtBearerEvents
                     {
                         OnChallenge = async context =>
                         {
                             context.HandleResponse();
                             context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                             context.Response.ContentType = "application/json";
                             await context.Response.WriteAsync(
                                 "{\"message\":\"Unauthorized - Token is missing or invalid.\"}");
                         },
                         OnForbidden = async context =>
                         {
                             context.Response.StatusCode = StatusCodes.Status403Forbidden;
                             context.Response.ContentType = "application/json";
                             await context.Response.WriteAsync(
                                 "{\"message\":\"Forbidden - You do not have permission to access this resource.\"}");
                         },
                     };
                 });

            builder.Services.AddGrpc();

            var userServiceUrl = builder.Environment.IsDevelopment()
                   ? "https://localhost:7022" : "https://userapi:443";
            var agencyServiceUrl = builder.Environment.IsDevelopment()
                ? "https://localhost:7198" : "https://agencyapi:443";
            var allocationServiceUrl = builder.Environment.IsDevelopment()
                ? "https://localhost:7055" : "https://allocationapi:443";
            var orderServiceUrl = builder.Environment.IsDevelopment()
                ? "https://localhost:7114" : "https://orderapi:443";
            if (builder.Environment.IsProduction())
            {
                // Cấu hình chỉ dành cho PRODUCTION
                var handler = new HttpClientHandler();
                // Đường dẫn này chỉ tồn tại trong môi trường production (Docker)
                var caCert = new X509Certificate2("/https/certs/ca.crt");
                handler.ServerCertificateCustomValidationCallback = (message, serverCert, chain, errors) =>
                {
                    if (serverCert == null) return false;
                    using var customChain = new X509Chain();
                    customChain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                    customChain.ChainPolicy.CustomTrustStore.Add(caCert);
                    customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    return customChain.Build(serverCert);
                };

                builder.Services.AddGrpcClient<UserGrpcService.UserGrpcServiceClient>(o =>
                    o.Address = new Uri(userServiceUrl)).ConfigurePrimaryHttpMessageHandler(() => handler);

                builder.Services.AddGrpcClient<EmailVerificationGrpcService.EmailVerificationGrpcServiceClient>(o =>
                    o.Address = new Uri(userServiceUrl)).ConfigurePrimaryHttpMessageHandler(() => handler);

                builder.Services.AddGrpcClient<AgencyGrpcService.AgencyGrpcServiceClient>(o =>
                    o.Address = new Uri(agencyServiceUrl)).ConfigurePrimaryHttpMessageHandler(() => handler);

                builder.Services.AddGrpcClient<VehicleInstanceGrpcService.VehicleInstanceGrpcServiceClient>(o =>
                    o.Address = new Uri(allocationServiceUrl)).ConfigurePrimaryHttpMessageHandler(() => handler);

                builder.Services.AddGrpcClient<CustomerGrpcService.CustomerGrpcServiceClient>(o =>
                    o.Address = new Uri(orderServiceUrl)).ConfigurePrimaryHttpMessageHandler(() => handler);

                builder.Services.AddGrpcClient<OrderGrpcService.OrderGrpcServiceClient>(o => // <-- THÊM VÀO
                    o.Address = new Uri(orderServiceUrl)).ConfigurePrimaryHttpMessageHandler(() => handler);
                // ...
            }
            else
            {
                // Cấu hình đơn giản cho LOCAL DEVELOPMENT
                // Không cần handler tùy chỉnh, hệ thống sẽ tin tưởng cert của localhost
                builder.Services.AddGrpcClient<UserGrpcService.UserGrpcServiceClient>(o =>
                    o.Address = new Uri(userServiceUrl));
                builder.Services.AddGrpcClient<EmailVerificationGrpcService.EmailVerificationGrpcServiceClient>(o =>
                    o.Address = new Uri(userServiceUrl));
                builder.Services.AddGrpcClient<AgencyGrpcService.AgencyGrpcServiceClient>(o =>
                    o.Address = new Uri(agencyServiceUrl));
                builder.Services.AddGrpcClient<VehicleInstanceGrpcService.VehicleInstanceGrpcServiceClient>(o =>
                    o.Address = new Uri(allocationServiceUrl));
                builder.Services.AddGrpcClient<CustomerGrpcService.CustomerGrpcServiceClient>(o =>
                    o.Address = new Uri(orderServiceUrl));
                builder.Services.AddGrpcClient<OrderGrpcService.OrderGrpcServiceClient>(o => // <-- THÊM VÀO
                    o.Address = new Uri(orderServiceUrl));
            }

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
                        var dbContext = services.GetRequiredService<AnalyticDbContext>();
                        var defaultConnStr = builder.Configuration.GetConnectionString("AnalyticDbConnection");
                        var dbName = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(defaultConnStr).InitialCatalog;
                        var masterConnStr = defaultConnStr.Replace($"Database={dbName}", "Database=master");

                        using (var connection = new Microsoft.Data.SqlClient.SqlConnection(masterConnStr))
                        {
                            connection.Open();
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = $"IF DB_ID('{dbName}') IS NULL CREATE DATABASE [{dbName}]";
                                command.ExecuteNonQuery();
                            }
                            logger.LogInformation("✅ Step 1/2: Database '{DbName}' created or already exists.", dbName);
                        }

                        dbContext.Database.Migrate();
                        logger.LogInformation("✅ Step 2/2: Database schema has been migrated to the latest version.");
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "❌ An error occurred during database setup.");
                    }
                }
            }

            if (app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Allocation API V1");
                    c.RoutePrefix = string.Empty;
                });
            }
            else
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
