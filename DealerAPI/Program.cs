

using AgencyRepository.Data;
using AgencyRepository.Repositories;
using AgencyService.Services;
using GrpcService;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Share.Setting;
using Share.ShareServices;
using System.Text;

namespace AgencyAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<AgencyDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("AgencyDbConnection")));

            builder.Services.AddScoped<IAgencyRepository,AgencyRepository.Repositories.AgencyRepository>();
            builder.Services.AddScoped<IAgencyContractRepository, AgencyContractRepository>();
            builder.Services.AddScoped<IAgencyDebtRepository, AgencyDebtRepository>();
            builder.Services.AddScoped<IAgencyTargetRepository, AgencyTargetRepository>();
            builder.Services.AddScoped<ITestDriveRepository, TestDriveRepository>();

            builder.Services.AddScoped<IAgencyInventoryRepository, AgencyInventoryRepository>();
            builder.Services.AddScoped<IAgencyService,AgencyService.Services.AgencyService>();
            builder.Services.AddScoped<IAgencyContractService, AgencyContractService>();
            builder.Services.AddScoped<IAgencyDebtService, AgencyDebtService>();
            builder.Services.AddScoped<IAgencyTargetService, AgencyTargetService>();
            builder.Services.AddScoped<IAgencyInventoryService, AgencyInventoryService>();
            builder.Services.AddScoped<ITestDriveService, TestDriveService>();

            builder.Services.AddScoped<IUserGrpcServiceClient, UserGrpcServiceClient>();
            builder.Services.AddScoped<IVehicleGrpcServiceClient, VehicleGrpcServiceClient>();


            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
            builder.Services.AddSingleton(jwtSettings);


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Agency API",
                    Version = "v1",
                    Description = "API for Agency Application"
                });
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
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policyBuilder =>
                {
                    policyBuilder.AllowAnyOrigin()
                                 .AllowAnyMethod()
                                 .AllowAnyHeader();
                });
            });

            //dang ký Jwt
            builder.Services.Configure<JwtSettings>(
            builder.Configuration.GetSection("Jwt")
            );

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
                            // Ngăn ASP.NET Core tự gửi 401 mặc định
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


            //add grpc
            builder.Services.AddGrpc();
            builder.Services.AddGrpcClient<UserGrpcService.UserGrpcServiceClient>(o =>
            {
                // URL của UserService (port gRPC)
                o.Address = new Uri("https://localhost:7022");
            });
            builder.Services.AddGrpcClient<VehicleGrpcService.VehicleGrpcServiceClient>(o =>
            {
                // URL của Vehicle (port gRPC)
                o.Address = new Uri("https://localhost:7055");
            });

            var app = builder.Build();
            app.MapGrpcService<AgencyGrpcServiceImpl>(); // ✅ Bắt buộc
            app.MapGet("/", () => "Use a gRPC client to communicate.");

            if (app.Environment.IsEnvironment("Production") || app.Environment.IsEnvironment("Docker"))
            {
                int maxRetries = 10;
                int delayInSeconds = 5;

                for (int i = 0; i < maxRetries; i++)
                {
                    try
                    {
                        using (var scope = app.Services.CreateScope())
                        {
                            var services = scope.ServiceProvider;
                            var dbContext = services.GetRequiredService<AgencyDbContext>();
                            var logger = services.GetRequiredService<ILogger<Program>>();

                            // Bước 1: Tự tạo DB nếu chưa có
                            var defaultConnStr = builder.Configuration.GetConnectionString("AgencyDbConnection");
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

                            // Bước 2: Tạo schema (các bảng)
                            dbContext.Database.EnsureCreated();
                            logger.LogInformation("✅ Step 2/2: Schema has been created successfully.");

                            break; // Thoát vòng lặp nếu tất cả thành công
                        }
                    }
                    catch (SqlException ex)
                    {
                        var logger = app.Services.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning(ex, "❌ Attempt {Attempt} of {MaxRetries}: Database is not ready yet. Retrying in {Delay} seconds...", i + 1, maxRetries, delayInSeconds);
                        Thread.Sleep(TimeSpan.FromSeconds(delayInSeconds));
                    }
                    catch (Exception ex)
                    {
                        var logger = app.Services.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "❌ An unexpected error occurred during database setup.");
                        break;
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
