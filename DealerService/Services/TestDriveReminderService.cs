// Trong file AgencyService/Implement/TestDriveReminderService.cs

using AgencyRepository.Repositories;
using GrpcService;
using Share.Setting;
using System;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
// Thêm 2 using này
using AgencyRepository.Model;
using System.Collections.Generic;
using AgencyService.Services;
using Microsoft.Extensions.Logging;
using Share.ShareServices;

namespace AgencyService.Implement
{
    public class TestDriveReminderService : ITestDriveReminderService
    {
        private readonly ITestDriveRepository _testDriveRepository;
        private readonly ICustomerGrpcServiceClient _customerGrpcClient;
        private readonly EmailSetting _emailSetting;
        // Thêm ILogger để ghi log
        private readonly ILogger<TestDriveReminderService> _logger;

        public TestDriveReminderService(
            ITestDriveRepository testDriveRepository,
            ICustomerGrpcServiceClient customerGrpcClient,
            EmailSetting emailSetting,
            ILogger<TestDriveReminderService> logger) // Inject ILogger
        {
            _testDriveRepository = testDriveRepository;
            _customerGrpcClient = customerGrpcClient;
            _emailSetting = emailSetting;
            _logger = logger;
        }

        /// <summary>
        /// Logic mới: Hiệu quả và không spam
        /// </summary>
        public async Task SendRemindersAsync()
        {
            var now = DateTime.UtcNow;
            var oneDayTarget = now.AddDays(1);
            var threeDayTarget = now.AddDays(3);

            // 1. Chỉ lấy các lịch hẹn "Scheduled" cần được nhắc
            var testDrivesToProcess = await _testDriveRepository.FindAsync(td =>
                td.AppointmentDate.HasValue &&
                td.Status == "Scheduled" && // Chỉ nhắc các lịch "Scheduled"
                (
                    (!td.IsThreeDayReminderSent && td.AppointmentDate.Value <= threeDayTarget) ||
                    (!td.IsOneDayReminderSent && td.AppointmentDate.Value <= oneDayTarget)
                )
            );

            if (!testDrivesToProcess.Any())
            {
                _logger.LogInformation("[TestDriveReminder] Không có lịch hẹn nào cần nhắc.");
                return;
            }

            _logger.LogInformation($"[TestDriveReminder] Tìm thấy {testDrivesToProcess.Count()} lịch hẹn cần xử lý.");

            var tasksToUpdateInDb = new List<TestDrive>();

            foreach (var td in testDrivesToProcess)
            {
                string reminderType = "";
                bool shouldProcess = false;

                // ----- XÁC ĐỊNH LOẠI NHẮC NHỞ -----
                // Ưu tiên 1 ngày (nếu lỡ cả 2)
                if (!td.IsOneDayReminderSent && td.AppointmentDate.Value <= oneDayTarget)
                {
                    reminderType = "1-day";
                    shouldProcess = true;
                    // ***** LOGIC MỚI *****
                    // Đánh dấu đã xử lý (kể cả thất bại) để không gửi lại
                    td.IsOneDayReminderSent = true;
                }
                else if (!td.IsThreeDayReminderSent && td.AppointmentDate.Value <= threeDayTarget)
                {
                    reminderType = "3-day";
                    shouldProcess = true;
                    // ***** LOGIC MỚI *****
                    // Đánh dấu đã xử lý (kể cả thất bại) để không gửi lại
                    td.IsThreeDayReminderSent = true;
                }

                if (shouldProcess)
                {
                    // ----- THỰC HIỆN GỬI -----
                    // Hàm này sẽ trả về true (thành công) hoặc false (thất bại)
                    bool sentSuccessfully = await TryProcessReminderAsync(td, reminderType);

                    // ***** LOGIC MỚI: XỬ LÝ KHI THẤT BẠI *****
                    if (!sentSuccessfully)
                    {
                        // Nếu gửi thất bại (gRPC lỗi, SMTP lỗi...)
                        // Chuyển status về "Pending" theo yêu cầu
                        _logger.LogWarning($"[TestDriveReminder] Gửi mail thất bại cho TD ID {td.Id}. Chuyển status về 'Pending'.");
                        td.Status = "Pending";
                    }

                    // Luôn thêm vào danh sách update (vì cờ đã thay đổi, hoặc status đã thay đổi)
                    tasksToUpdateInDb.Add(td);
                }
            }

            // ----- 3. CẬP NHẬT DATABASE -----
            if (tasksToUpdateInDb.Any())
            {
                // Dùng hàm UpdateRange bạn vừa thêm ở Bước 1
                _testDriveRepository.UpdateRange(tasksToUpdateInDb);
                await _testDriveRepository.SaveChangesAsync();
                _logger.LogInformation($"[TestDriveReminder] Đã xử lý và cập nhật {tasksToUpdateInDb.Count} bản ghi.");
            }
        }

        // Sửa lại hàm private: trả về bool (thành công/thất bại)
        private async Task<bool> TryProcessReminderAsync(TestDrive td, string reminderType)
        {
            CustomerReply customerReply;
            try
            {
                // 1. Lấy thông tin khách hàng qua gRPC
                customerReply = await _customerGrpcClient.GetCustomerByIdAsync(td.CustomerId);
                if (string.IsNullOrWhiteSpace(customerReply?.Email))
                {
                    // Nếu khách không có email, ta coi như "gửi thất bại" (theo logic mới)
                    _logger.LogWarning($"[TestDriveReminder] Lỗi: CustomerId {td.CustomerId} (TD ID {td.Id}) không có email.");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TestDriveReminder] Lỗi gRPC khi lấy CustomerId {td.CustomerId} (TD ID {td.Id}).");
                return false; // Thất bại
            }

            try
            {
                // 2. Gửi email
                string subject = $"🚗 Reminder: Your test drive is {reminderType} away!";
                string body = $@"
                    <p>Dear {customerReply.FullName},</p>
                    <p>This is a friendly reminder for your <strong>test drive</strong> appointment scheduled on:</p>
                    <p><b>{td.AppointmentDate:dddd, MMMM dd yyyy HH:mm} (UTC)</b></p>
                    <p>We look forward to seeing you soon!</p>
                    <p>— Electric Vehicle Agency</p>";

                await SendEmailAsync(customerReply.Email, subject, body);

                _logger.LogInformation($"[TestDriveReminder] Đã gửi email {reminderType} thành công cho TD ID {td.Id}.");
                return true; // Thành công
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TestDriveReminder] Lỗi SMTP khi gửi email đến {customerReply.Email} (TD ID {td.Id}).");
                return false; // Thất bại
            }
        }

        // Hàm SendEmailAsync giữ nguyên (không đổi)
        private Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var client = new SmtpClient(_emailSetting.SmtpServer, _emailSetting.SmtpPort)
            {
                Credentials = new System.Net.NetworkCredential(_emailSetting.SenderEmail, _emailSetting.SenderPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSetting.SenderEmail, _emailSetting.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(toEmail);

            return client.SendMailAsync(mailMessage);
        }
    }
}