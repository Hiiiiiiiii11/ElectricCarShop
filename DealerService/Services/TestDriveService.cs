using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using GrpcService;
using Microsoft.Extensions.Logging;
using Share.Setting;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public class TestDriveService : ITestDriveService
    {
        private readonly ITestDriveRepository _testDriveRepository;
        private readonly IVehicleInstanceGrpcServiceClient _vehicleGrpcServiceClient;
        private readonly ICustomerGrpcServiceClient _customerGrpcServiceClient;
        private readonly IAgencyRepository _agencyRepository;
        private readonly IOrderGrpcServiceClient _orderGrpcServiceClient;
        private readonly EmailSetting _emailSetting;
        private readonly ILogger<TestDriveService> _logger;

        public TestDriveService(
            ITestDriveRepository testDriveRepository,
            IVehicleInstanceGrpcServiceClient vehicleGrpcServiceClient,
            ICustomerGrpcServiceClient customerGrpcServiceClient,
            IAgencyRepository agencyRepository,
            IOrderGrpcServiceClient orderGrpcServiceClient,
            EmailSetting emailSetting,
            ILogger<TestDriveService> logger

            )
        {
            _testDriveRepository = testDriveRepository;
            _vehicleGrpcServiceClient = vehicleGrpcServiceClient;
            _customerGrpcServiceClient = customerGrpcServiceClient;
            _agencyRepository = agencyRepository;
            _orderGrpcServiceClient = orderGrpcServiceClient;
            _emailSetting = emailSetting;
            _logger = logger;
        }

        // ===== CREATE =====
        public async Task<TestDriveResponse> CreateTestDriveAsync(CreateTestDriveRequest request)
        {
            // 1️⃣ KIỂM TRA BÁO GIÁ TỒN TẠI (gRPC)
            var quotationCheck = await _orderGrpcServiceClient
                .CheckQuotationExistsForVehicleAsync(request.VehicleInstanceId);

            if (quotationCheck.Exists)
            {
                throw new InvalidOperationException(
                    $"Không thể đặt lịch. Xe (ID: {request.VehicleInstanceId}) đã nằm trong một Báo giá (Pending hoặc Accepted).");
            }

            // 2️⃣ KIỂM TRA BOOKING TRÙNG (USER + VEHICLE)
            var existingUserBookingForVehicle = await _testDriveRepository.FindAsync(td =>
                td.CustomerId == request.CustomerId &&
                td.VehicleInstanceId == request.VehicleInstanceId &&
                (td.Status == "Scheduled" || td.Status == "Completed")
            );

            if (existingUserBookingForVehicle.Any())
            {
                throw new InvalidOperationException(
                    $"Khách hàng (ID: {request.CustomerId}) đã có lịch hẹn (Scheduled/Completed) cho xe này (ID: {request.VehicleInstanceId}).");
            }

            // 3️⃣ KIỂM TRA NGÀY HẸN
            if (!request.AppointmentDate.HasValue)
            {
                throw new InvalidOperationException("Ngày hẹn (AppointmentDate) là bắt buộc.");
            }

            var requestedDate = request.AppointmentDate.Value.Date;
            if (requestedDate < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("Ngày hẹn không thể trước ngày hiện tại.");
            }

            // ⚠️ CHỈ CHẶN LỊCH ĐÃ ĐƯỢC CHỐT (Scheduled), Pending vẫn cho phép trùng ngày
            var existingVehicleBookingOnDate = await _testDriveRepository.FindAsync(td =>
                td.VehicleInstanceId == request.VehicleInstanceId &&
                td.Status == "Scheduled" &&
                td.AppointmentDate.HasValue &&
                td.AppointmentDate.Value.Date == requestedDate
            );

            if (existingVehicleBookingOnDate.Any())
            {
                throw new InvalidOperationException(
                    $"Lịch trùng! Xe (ID: {request.VehicleInstanceId}) đã có lịch hẹn lái thử vào ngày {requestedDate:yyyy-MM-dd}.");
            }

            // 4️⃣ TẠO MỚI – DEFAULT STATUS = Pending
            var testDrive = new TestDrive
            {
                AgencyId = request.AgencyId,
                VehicleInstanceId = request.VehicleInstanceId,
                CustomerId = request.CustomerId,
                AppointmentDate = request.AppointmentDate,
                Notes = request.Notes,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                IsOneDayReminderSent = false,
                IsThreeDayReminderSent = false
            };

            await _testDriveRepository.AddAsync(testDrive);
            await _testDriveRepository.SaveChangesAsync();

            // 5️⃣ LẤY DỮ LIỆU QUA gRPC
            var vehicle = await _vehicleGrpcServiceClient.GetVehicleInstanceByIdAsync(testDrive.VehicleInstanceId);
            var customer = await _customerGrpcServiceClient.GetCustomerByIdAsync(testDrive.CustomerId);

            // 6️⃣ GỬI EMAIL THÔNG BÁO ĐẶT LỊCH (PENDING)
            try
            {
                if (!string.IsNullOrWhiteSpace(customer.Email))
                {
                    var subject = "🚗 Đặt lịch lái thử thành công (đang chờ xác nhận)";
                    var body = $@"
                        <p>Xin chào {customer.FullName},</p>
                        <p>Chúng tôi đã nhận được yêu cầu <b>đặt lịch lái thử</b> của bạn.</p>
                        <p><b>Thời gian dự kiến:</b> {testDrive.AppointmentDate:dd/MM/yyyy HH:mm}</p>
                        <p><b>Trạng thái hiện tại:</b> <span style='color:orange;'>Pending (Đang chờ xác nhận)</span></p>
                        <p>Chúng tôi sẽ sớm liên hệ để xác nhận lịch hẹn của bạn.</p>
                        <p>Trân trọng,<br/>EVN Auto</p>";

                    await SendEmailAsync(customer.Email, subject, body);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[TestDrive] Lỗi khi gửi email đặt lịch Pending cho TestDriveId={testDrive.Id}");
                // không throw, tránh làm fail API
            }

            var response = MapToResponse(testDrive);
            response.Vehicle = vehicle;
            response.Customer = customer;

            return response;
        }

        // ===== UPDATE (ĐÃ THÊM KIỂM TRA TRÙNG NGÀY) =====
        public async Task<TestDriveResponse> UpdateTestDriveAsync(int id, UpdateTestDriveRequest request)
        {
            var testDrive = await _testDriveRepository.GetByIdAsync(id);
            if (testDrive == null)
                throw new KeyNotFoundException($"Test drive with ID {id} not found.");

            var oldStatus = testDrive.Status;   // 👈 lưu lại status cũ

            // --- KIỂM TRA TRÙNG NGÀY NẾU ĐỔI NGÀY ---
            if (request.AppointmentDate.HasValue && request.AppointmentDate.Value.Date != testDrive.AppointmentDate?.Date)
            {
                var requestedDate = request.AppointmentDate.Value.Date;

                var existingBookingOnDate = await _testDriveRepository.FindAsync(td =>
                    td.VehicleInstanceId == testDrive.VehicleInstanceId &&
                    td.Id != id &&
                    td.Status == "Scheduled" &&
                    td.AppointmentDate.HasValue &&
                    td.AppointmentDate.Value.Date == requestedDate
                );

                if (existingBookingOnDate.Any())
                {
                    throw new InvalidOperationException(
                        $"Lịch trùng! Xe (ID: {testDrive.VehicleInstanceId}) đã có lịch hẹn lái thử vào ngày {requestedDate:yyyy-MM-dd}.");
                }

                if (requestedDate < DateTime.UtcNow.Date)
                    throw new InvalidOperationException("Ngày hẹn không thể trước ngày hiện tại.");

                testDrive.AppointmentDate = request.AppointmentDate.Value;
            }

            // Cập nhật field khác
            if (!string.IsNullOrWhiteSpace(request.Status))
                testDrive.Status = request.Status;

            if (!string.IsNullOrWhiteSpace(request.Notes))
                testDrive.Notes = request.Notes;

            if (!string.IsNullOrWhiteSpace(request.Feedback))
                testDrive.Feedback = request.Feedback;

            testDrive.UpdateAt = DateTime.UtcNow.AddMonths(-1);

            _testDriveRepository.Update(testDrive);
            await _testDriveRepository.SaveChangesAsync();

            // Lấy dữ liệu gRPC
            var vehicle = await _vehicleGrpcServiceClient.GetVehicleInstanceByIdAsync(testDrive.VehicleInstanceId);
            var customer = await _customerGrpcServiceClient.GetCustomerByIdAsync(testDrive.CustomerId);
            var agency = await _agencyRepository.GetByIdAsync(testDrive.AgencyId);

            // ️⃣ GỬI EMAIL KHI CHỐT LỊCH: status CHUYỂN SANG "Scheduled"
            if (!string.Equals(oldStatus, "Scheduled", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(testDrive.Status, "Scheduled", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(customer.Email))
                    {
                        var subject = "✅ Lịch lái thử của bạn đã được xác nhận";
                        var body = $@"
                            <p>Xin chào {customer.FullName},</p>
                            <p>Lịch <b>lái thử xe</b> của bạn đã được <b>xác nhận</b>.</p>
                            <p><b>Thời gian:</b> {testDrive.AppointmentDate:dd/MM/yyyy HH:mm}</p>
                           <p><b>Địa điểm:</b><br/>{(agency != null ? $"{agency.AgencyName}<br/>{agency.Address}" : "Đại lý EVN Auto")}</p>
                            <p>Rất mong được đón tiếp bạn!</p>
                            <p>Trân trọng,<br/>EVN Auto</p>";

                        await SendEmailAsync(customer.Email, subject, body);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"[TestDrive] Lỗi khi gửi email xác nhận Scheduled cho TestDriveId={testDrive.Id}");
                    // không throw
                }
            }

            var response = MapToResponse(testDrive);
            response.Vehicle = vehicle;
            response.Customer = customer;

            return response;
        }

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


        // ===== DELETE =====
        public async Task<bool> DeleteTestDriveAsync(int id)
        {
            var testDrive = await _testDriveRepository.GetByIdAsync(id);
            if (testDrive == null)
                throw new KeyNotFoundException($"Test drive with ID {id} not found.");

            _testDriveRepository.Remove(testDrive);
            await _testDriveRepository.SaveChangesAsync();
            return true;
        }

        // ===== GET ALL =====
        public async Task<IEnumerable<TestDriveResponse>> GetAllTestDrivesAsync()
        {
            var testDrives = await _testDriveRepository.GetAllAsync();
            var result = new List<TestDriveResponse>();

            foreach (var td in testDrives)
            {
                var response = MapToResponse(td);

                // Gọi gRPC
                var vehicle = await _vehicleGrpcServiceClient.GetVehicleInstanceByIdAsync(td.VehicleInstanceId);
                var customer = await _customerGrpcServiceClient.GetCustomerByIdAsync(td.CustomerId);

                response.Vehicle = vehicle;
                response.Customer = customer;

                result.Add(response);
            }

            return result;
        }

        // ===== GET BY ID =====
        public async Task<TestDriveResponse> GetTestDriveByIdAsync(int id)
        {
            var testDrive = await _testDriveRepository.GetByIdAsync(id);
            if (testDrive == null)
                throw new KeyNotFoundException($"Test drive with ID {id} not found.");

            var vehicle = await _vehicleGrpcServiceClient.GetVehicleInstanceByIdAsync(testDrive.VehicleInstanceId);
            var customer = await _customerGrpcServiceClient.GetCustomerByIdAsync(testDrive.CustomerId);

            var response = MapToResponse(testDrive);
            response.Vehicle = vehicle;
            response.Customer = customer;

            return response;
        }
        // ===== GET BY AGENCY ID =====
        public async Task<IEnumerable<TestDriveResponse>> GetTestDrivesByAgencyIdAsync(int agencyId)
        {
            var agency = await _agencyRepository.GetByIdAsync(agencyId);
            if (agency == null)
                throw new KeyNotFoundException($"Agency with ID {agencyId} not found.");
            var testDrives = await _testDriveRepository.GetTestDrivesByAgencyIdAsync(agencyId);
            
            var result = new List<TestDriveResponse>();
            foreach (var td in testDrives)
            {
                var response = MapToResponse(td);
                // Gọi gRPC
                var vehicle = await _vehicleGrpcServiceClient.GetVehicleInstanceByIdAsync(td.VehicleInstanceId);
                var customer = await _customerGrpcServiceClient.GetCustomerByIdAsync(td.CustomerId);
                response.Vehicle = vehicle;
                response.Customer = customer;
                result.Add(response);
            }
            return result;
        }

        // ===== MAP =====
        public TestDriveResponse MapToResponse(TestDrive td)
        {
            return new TestDriveResponse
            {
                Id = td.Id,
                AgencyId = td.AgencyId,
                AgencyName = td.Agency?.AgencyName,
                VehicleInstanceId = td.VehicleInstanceId,
                CustomerId = td.CustomerId,
                AppointmentDate = td.AppointmentDate,
                Status = td.Status,
                Notes = td.Notes,
                Feedback = td.Feedback,
                CreateAt = td.CreateAt,
                UpdateAt = td.UpdateAt
            };
        }
    }
}
