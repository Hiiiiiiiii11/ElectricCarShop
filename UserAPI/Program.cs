
using CloudinaryDotNet;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Share.Setting;
using Share.ShareServices;
using System.Text;
using UserRepository.Data;
using UserRepository.Model;
using UserRepository.Repositories;
using UserService.Implement;
using UserService.Services;

namespace UserAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);


            // Add services to the container.
            builder.Services.AddDbContext<UserDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("UserDbConnection")));

            builder.Services.AddSingleton(sp =>
            sp.GetRequiredService<IOptions<AdminAccountSettings>>().Value);

            builder.Services.Configure<AdminAccountSettings>(
            builder.Configuration.GetSection("AdminAccountSettings"));
            var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
            builder.Services.AddSingleton(jwtSettings);

            builder.Services.Configure<CloudDinarySetting>(
            builder.Configuration.GetSection("CloudDinarySetting"));

            builder.Services.AddSingleton(provider =>
            {
                var config = builder.Configuration.GetSection("CloudinarySettings").Get<CloudDinarySetting>();
                var account = new Account(config.CloudName, config.ApiKey, config.ApiSecret);
                return new Cloudinary(account);
            });

            builder.Services.Configure<EmailSetting>(
            builder.Configuration.GetSection("EmailSettings"));
            builder.Services.AddSingleton(resolver =>
            resolver.GetRequiredService<IOptions<EmailSetting>>().Value);


            builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository.Repositories.UserRepository>();
            builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
            builder.Services.AddScoped<IUserService, UserService.Services.UserService>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IRoleService, RoleService>();
            builder.Services.AddScoped<IUploadPhotoService,UpLoadPhotoService>();
            builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
            builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();


            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "User API",
                    Version = "v1",
                    Description = "API for User Application"
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
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            //add grpc
            builder.Services.AddGrpc();

            var app = builder.Build();
            app.MapGrpcService<UserGrpcServiceImpl>();
            app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");
            app.UseCors("AllowAll");


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
                            var dbContext = services.GetRequiredService<UserDbContext>();
                            var logger = services.GetRequiredService<ILogger<Program>>();

                            // Bước 1: Tự tạo DB nếu chưa có
                            var defaultConnStr = builder.Configuration.GetConnectionString("UserDbConnection");
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
                                logger.LogInformation("✅ Step 1/3: Database '{DbName}' created or already exists.", dbName);
                            }

                            // Bước 2: Tạo schema (các bảng)
                            dbContext.Database.EnsureCreated();
                            logger.LogInformation("✅ Step 2/3: Schema has been created successfully.");

                            // Bước 3: Seed admin account (và role)
                            var adminSettings = services.GetRequiredService<IOptions<AdminAccountSettings>>().Value;

                            var adminRole = dbContext.Roles.FirstOrDefault(r => r.RoleName == "Admin");
                            if (adminRole == null)
                            {
                                adminRole = new UserRepository.Model.Roles { RoleName = "Admin" };
                                dbContext.Roles.Add(adminRole);
                                dbContext.SaveChanges();
                            }

                            if (!dbContext.Users.Any(u => u.Email == adminSettings.Email))
                            {
                                var hashedPassword = BCrypt.Net.BCrypt.HashPassword(adminSettings.Password);
                                var adminUser = new UserRepository.Model.Users
                                {
                                    Email = adminSettings.Email,
                                    PasswordHash = hashedPassword,
                                    UserName = adminSettings.UserName,
                                    Status = "Active",
                                    RoleId = adminRole.Id
                                };
                                dbContext.Users.Add(adminUser);
                                dbContext.SaveChanges();
                                logger.LogInformation("✅ Step 3/3: Admin account has been seeded successfully.");
                            }

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
