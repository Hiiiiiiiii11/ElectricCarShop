// Trong file AgencyService/Hosted/TestDriveReminderBackgroundService.cs

using AgencyService.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AgencyService.Hosted
{
    public class TestDriveReminderBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        // Thêm ILogger
        private readonly ILogger<TestDriveReminderBackgroundService> _logger;

        public TestDriveReminderBackgroundService(IServiceProvider serviceProvider,
                                               ILogger<TestDriveReminderBackgroundService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("[TestDriveReminder] Service is starting.");

            // Chờ 10 giây để các service khác khởi động
            await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("[TestDriveReminder] Service is running a check...");
                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var reminderService = scope.ServiceProvider.GetRequiredService<ITestDriveReminderService>();
                        await reminderService.SendRemindersAsync();
                    }
                }
                catch (Exception ex)
                {
                    // Lỗi ở đây thường là lỗi không thể tạo scope,
                    // các lỗi logic đã được xử lý bên trong SendRemindersAsync
                    _logger.LogCritical(ex, "[TestDriveReminder] Unhandled error in ExecuteAsync.");
                }

                _logger.LogInformation("[TestDriveReminder] Check finished. Waiting for next cycle.");

                // Sửa lại: Chờ 1 tiếng (hoặc 10 phút/30 phút tùy bạn)
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
            _logger.LogInformation("[TestDriveReminder] Service is stopping.");
        }
    }
}