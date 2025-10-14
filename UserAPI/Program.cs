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

            // --- Đăng ký các services ---
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
            builder.Services.AddScoped<IUploadPhotoService, UpLoadPhotoService>();
            builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
            builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "User API", Version = "v1" });
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

            var app = builder.Build();

            // =================================================================
            // === LOGIC TỰ ĐỘNG TẠO DATABASE KHI DEPLOY (ÁP DỤNG TỪ DỰ ÁN CŨ) ===
            // =================================================================
            if (app.Environment.IsEnvironment("Docker"))
            {
                // Thêm một độ trễ nhỏ để đảm bảo SQL Server có đủ thời gian khởi động hoàn toàn
                // ngay cả sau khi health check đã pass.
                Thread.Sleep(TimeSpan.FromSeconds(10));

                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    try
                    {
                        var dbContext = services.GetRequiredService<UserDbContext>();

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
                            adminRole = new Roles { RoleName = "Admin" };
                            dbContext.Roles.Add(adminRole);
                            dbContext.SaveChanges();
                        }

                        if (!dbContext.Users.Any(u => u.Email == adminSettings.Email))
                        {
                            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(adminSettings.Password);
                            var adminUser = new Users
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
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "❌ An error occurred during database setup.");
                    }
                }
            }

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Docker"))
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.MapGrpcService<UserGrpcServiceImpl>();

            app.Run();
        }
    }
}

