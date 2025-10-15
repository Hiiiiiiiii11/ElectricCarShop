using AllocationRepository.Repositories;
using AllocationService.Services;
using GrpcService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using OrderAPIService.Services;
using OrderRepository.Data;
using OrderRepository.Repositories;
using OrderService.Service;
using OrderService.Services;
using Share.Setting;
using Share.ShareServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace OrderAPI
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
                        o.UseHttps("/https/certs/orderapi.pfx", pfxPassword);
                    });
                });
            }


            builder.Services.AddDbContext<OrderDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("OrderDbConnection"),
                sqlServerOptionsAction: sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                }));

            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
            builder.Services.AddSingleton(jwtSettings);
            builder.Services.AddControllers();

            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();
            builder.Services.AddScoped<IQuotationRepository, QuotationRepository>();
            builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
            builder.Services.AddScoped<IPaymentRepository, PaymentRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository.Repositories.OrderRepository>();
            builder.Services.AddScoped<IContractRepository, ContractRepository>();
            builder.Services.AddScoped<IEmailVerificationGrpcServiceClient, EmailVerificationGrpcServiceClient>();

            builder.Services.AddScoped<ICustomerService, CustomerService>();
            builder.Services.AddScoped<IFeedbackService, FeedbackService>();
            builder.Services.AddScoped<IQuotationService, QuotationService>();
            builder.Services.AddScoped<ITransactionService, TransactionService>();
            builder.Services.AddScoped<IOrderService, OrderService.Services.OrderService>();
            builder.Services.AddScoped<IPaymentService, PaymentService>();
            builder.Services.AddScoped<IContractService, ContractService>();

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

            builder.Services.AddScoped<IAgencyGrpcServiceClient, AgencyGrpcServiceClient>();
            builder.Services.AddScoped<IVehicleGrpcServiceClient, VehicleGrpcServiceClient>();
            builder.Services.AddGrpc();

            var emailServiceUrl = builder.Environment.IsDevelopment()
               ? "https://localhost:7022"
               : "https://userapi:443";
            var agencyServiceUrl = builder.Environment.IsDevelopment()
               ? "https://localhost:7198"
               : "https://agencyapi:443";
            var vehicleServiceUrl = builder.Environment.IsDevelopment()
               ? "https://localhost:7055"
               : "https://allocationapi:443";

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

            builder.Services.AddGrpcClient<EmailVerificationGrpcService.EmailVerificationGrpcServiceClient>(o =>
            {
                o.Address = new Uri(emailServiceUrl);
            }).ConfigurePrimaryHttpMessageHandler(() => handler);

            builder.Services.AddGrpcClient<AgencyGrpcService.AgencyGrpcServiceClient>(o =>
            {
                o.Address = new Uri(agencyServiceUrl);
            }).ConfigurePrimaryHttpMessageHandler(() => handler);

            builder.Services.AddGrpcClient<VehicleGrpcService.VehicleGrpcServiceClient>(o =>
            {
                o.Address = new Uri(vehicleServiceUrl);
            }).ConfigurePrimaryHttpMessageHandler(() => handler);

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
                        var dbContext = services.GetRequiredService<OrderDbContext>();
                        var defaultConnStr = builder.Configuration.GetConnectionString("OrderDbConnection");
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
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API V1");
                    c.RoutePrefix = string.Empty;
                });
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

