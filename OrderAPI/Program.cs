using AllocationRepository.Repositories;
using AllocationService.Services;
using CloudinaryDotNet;
using GrpcService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderAPIService.Services; // nếu bạn có service riêng ở đây, giữ lại
using OrderRepository.Data;
using OrderRepository.Repositories;
using OrderService.Service;
using OrderService.Services;
using Share.Setting;       // CloudDinarySetting
using Share.ShareServices; // IUploadPhotoService, UpLoadPhotoService
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OrderAPI
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
                var pfxPassword = builder.Configuration["Kestrel:CertificatePassword"];
                builder.WebHost.ConfigureKestrel(options =>
                {
                    // HTTP/1.1 (qua Nginx)
                    options.ListenAnyIP(80, o => o.Protocols = HttpProtocols.Http1);
                    // HTTP/2 cho gRPC nội bộ (nếu cần)
                    options.ListenAnyIP(443, o =>
                    {
                        o.Protocols = HttpProtocols.Http2;
                        o.UseHttps("/https/certs/orderapi.pfx", pfxPassword);
                    });
                });
            }

            // =================== DB ===================
            builder.Services.AddDbContext<OrderDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("OrderDbConnection"),
                    sql => sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(30), null)
                ));

            // =================== JWT ===================
            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
            builder.Services.AddSingleton(jwtSettings);

            // =================== Cloudinary ===================
            // appsettings.json: "CloudinarySettings": { "CloudName": "", "ApiKey": "", "ApiSecret": "" }
            builder.Services.Configure<CloudDinarySetting>(builder.Configuration.GetSection("CloudinarySettings"));
            builder.Services.AddSingleton(provider =>
            {
                var cfg = provider.GetRequiredService<IOptions<CloudDinarySetting>>().Value;
                var account = new Account(cfg.CloudName, cfg.ApiKey, cfg.ApiSecret);
                return new Cloudinary(account);
            });
            builder.Services.AddScoped<IUploadPhotoService, UpLoadPhotoService>(); // service upload ảnh dùng Cloudinary

            builder.Services.AddControllers();

            // =================== REPOSITORIES ===================
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository.Repositories.OrderRepository>();
            builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            builder.Services.AddScoped<IContractRepository, ContractRepository>();
            builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
            // ===== gRPC service wrappers (Share.ShareServices) =====
            builder.Services.AddScoped<IEmailVerificationGrpcServiceClient, EmailVerificationGrpcServiceClient>();
            builder.Services.AddScoped<IUserGrpcServiceClient, UserGrpcServiceClient>();
            builder.Services.AddScoped<IAgencyGrpcServiceClient, AgencyGrpcServiceClient>();
            builder.Services.AddScoped<IVehicleInstanceGrpcServiceClient, VehicleInstanceGrpcServiceClient>();
            builder.Services.AddScoped<ICustomerGrpcServiceClient, CustomerGrpcServiceClient>();
            // IImageStorageService (ở OrderService) dùng Cloudinary đọc từ IConfiguration (env vars)
            builder.Services.AddScoped<OrderService.Services.IImageStorageService,
                                       OrderService.Services.CloudinaryImageStorageService>();

            // =================== SERVICES ===================
            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IFeedbackService, FeedbackService>();
            builder.Services.AddScoped<IQuotationService, QuotationService>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();
            builder.Services.AddScoped<IOrderDetailService, OrderDetailService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IContractService, ContractService>();
            builder.Services.AddScoped<IDeliveryService, DeliveryService>(); // <— thêm Delivery service

            // =================== AUTH ===================
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

            // =================== gRPC CLIENTS ===================
            builder.Services.AddGrpc();

            var emailServiceUrl = builder.Environment.IsDevelopment()
                ? "https://localhost:7022" : "https://userapi:443";
            var agencyServiceUrl = builder.Environment.IsDevelopment()
                ? "https://localhost:7198" : "https://agencyapi:443";
            var vehicleServiceUrl = builder.Environment.IsDevelopment()
                ? "https://localhost:7055" : "https://allocationapi:443";
            var customerServiceUrl = builder.Environment.IsDevelopment()
                ? "https://localhost:7114"      // cổng OrderAPI (nếu bạn host Customer gRPC trong OrderAPI)
                : "https://orderapi:443";

            if (builder.Environment.IsProduction())
            {
                var handler = new HttpClientHandler();
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
                    o.Address = new Uri(emailServiceUrl)).ConfigurePrimaryHttpMessageHandler(() => handler);

                builder.Services.AddGrpcClient<EmailVerificationGrpcService.EmailVerificationGrpcServiceClient>(o =>
                    o.Address = new Uri(emailServiceUrl)).ConfigurePrimaryHttpMessageHandler(() => handler);

                builder.Services.AddGrpcClient<AgencyGrpcService.AgencyGrpcServiceClient>(o =>
                    o.Address = new Uri(agencyServiceUrl)).ConfigurePrimaryHttpMessageHandler(() => handler);

                builder.Services.AddGrpcClient<VehicleInstanceGrpcService.VehicleInstanceGrpcServiceClient>(o =>
                    o.Address = new Uri(vehicleServiceUrl)).ConfigurePrimaryHttpMessageHandler(() => handler);
                builder.Services.AddGrpcClient<CustomerGrpcService.CustomerGrpcServiceClient>(o =>
                    o.Address = new Uri(customerServiceUrl)).ConfigurePrimaryHttpMessageHandler(() => handler);
            }
            else
            {
                builder.Services.AddGrpcClient<UserGrpcService.UserGrpcServiceClient>(o =>
                    o.Address = new Uri(emailServiceUrl));
                builder.Services.AddGrpcClient<EmailVerificationGrpcService.EmailVerificationGrpcServiceClient>(o =>
                    o.Address = new Uri(emailServiceUrl));
                builder.Services.AddGrpcClient<AgencyGrpcService.AgencyGrpcServiceClient>(o =>
                    o.Address = new Uri(agencyServiceUrl));
                builder.Services.AddGrpcClient<VehicleInstanceGrpcService.VehicleInstanceGrpcServiceClient>(o =>
                    o.Address = new Uri(vehicleServiceUrl));
                builder.Services.AddGrpcClient<CustomerGrpcService.CustomerGrpcServiceClient>(o =>
                    o.Address = new Uri(customerServiceUrl));
            }

            // =================== Swagger & CORS ===================
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Order API", Version = "v1" });
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
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            var app = builder.Build();
            app.UseForwardedHeaders();

            // =================== DB Auto Create & Migrate (PROD) ===================
            if (app.Environment.IsProduction())
            {
                Thread.Sleep(TimeSpan.FromSeconds(15));
                using var scope = app.Services.CreateScope();
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();
                try
                {
                    var dbContext = services.GetRequiredService<OrderDbContext>();
                    var defaultConnStr = builder.Configuration.GetConnectionString("OrderDbConnection");
                    var dbName = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(defaultConnStr).InitialCatalog;
                    var masterConnStr = defaultConnStr.Replace($"Database={dbName}", "Database=master");

                    using var connection = new Microsoft.Data.SqlClient.SqlConnection(masterConnStr);
                    connection.Open();
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"IF DB_ID('{dbName}') IS NULL CREATE DATABASE [{dbName}]";
                        command.ExecuteNonQuery();
                    }
                    logger.LogInformation("✅ Database '{DbName}' created or already exists.", dbName);

                    dbContext.Database.Migrate();
                    logger.LogInformation("✅ Step 2/2: Database schema migrated to latest version.");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "❌ Error during database setup.");
                }
            }

            // =================== Swagger ===================
            if (app.Environment.IsProduction())
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API V1");
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
            app.MapGrpcService<CustomerGrpcServiceImpl>();

            app.Run();
        }
    }
}
