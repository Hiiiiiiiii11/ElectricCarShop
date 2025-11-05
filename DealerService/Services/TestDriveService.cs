using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using GrpcService;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public TestDriveService(
            ITestDriveRepository testDriveRepository,
            IVehicleInstanceGrpcServiceClient vehicleGrpcServiceClient,
            ICustomerGrpcServiceClient customerGrpcServiceClient,
            IAgencyRepository agencyRepository,
            IOrderGrpcServiceClient orderGrpcServiceClient
            )
        {
            _testDriveRepository = testDriveRepository;
            _vehicleGrpcServiceClient = vehicleGrpcServiceClient;
            _customerGrpcServiceClient = customerGrpcServiceClient;
            _agencyRepository = agencyRepository;
            _orderGrpcServiceClient = orderGrpcServiceClient;
        }

        // ===== CREATE =====
        public async Task<TestDriveResponse> CreateTestDriveAsync(CreateTestDriveRequest request)
        {
            // === BƯỚC 2: KIỂM TRA BÁO GIÁ TỒN TẠI (gRPC) ===
            var quotationCheck = await _orderGrpcServiceClient.CheckQuotationExistsForVehicleAsync(request.VehicleInstanceId);
            if (quotationCheck.Exists)
            {
                throw new InvalidOperationException($"Không thể đặt lịch. Xe (ID: {request.VehicleInstanceId}) đã nằm trong một Báo giá (Pending hoặc Accepted).");
            }
            // === KẾT THÚC KIỂM TRA BÁO GIÁ ===

            // --- BƯỚC 1: KIỂM TRA BOOKING TRÙNG LẶP (USER + VEHICLE) (Logic cũ) ---
            var existingUserBookingForVehicle = await _testDriveRepository.FindAsync(td =>
                td.CustomerId == request.CustomerId &&
                td.VehicleInstanceId == request.VehicleInstanceId &&
                (td.Status == "Scheduled" || td.Status == "Completed")
            );

            if (existingUserBookingForVehicle.Any())
            {
                throw new InvalidOperationException($"Khách hàng (ID: {request.CustomerId}) đã có lịch hẹn (Scheduled/Completed) cho xe này (ID: {request.VehicleInstanceId}).");
            }

            // --- BƯỚC 2: KIỂM TRA TRÙNG NGÀY (Logic cũ) ---
            if (!request.AppointmentDate.HasValue)
            {
                throw new InvalidOperationException("Ngày hẹn (AppointmentDate) là bắt buộc.");
            }

            var requestedDate = request.AppointmentDate.Value.Date;

            var existingVehicleBookingOnDate = await _testDriveRepository.FindAsync(td =>
                td.VehicleInstanceId == request.VehicleInstanceId &&
                td.Status == "Scheduled" &&
                td.AppointmentDate.HasValue &&
                td.AppointmentDate.Value.Date == requestedDate
            );

            if (existingVehicleBookingOnDate.Any())
            {
                throw new InvalidOperationException($"Lịch trùng! Xe (ID: {request.VehicleInstanceId}) đã có lịch hẹn lái thử vào ngày {requestedDate:yyyy-MM-dd}.");
            }

            // --- BƯỚC 3: TẠO MỚI (Code cũ) ---
            var testDrive = new TestDrive
            {
                AgencyId = request.AgencyId,
                VehicleInstanceId = request.VehicleInstanceId,
                CustomerId = request.CustomerId,
                AppointmentDate = request.AppointmentDate,
                Notes = request.Notes,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Scheduled" : request.Status,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow,
                IsOneDayReminderSent = false,
                IsThreeDayReminderSent = false
            };

            await _testDriveRepository.AddAsync(testDrive);
            await _testDriveRepository.SaveChangesAsync();

            // --- BƯỚC 4: LẤY DỮ LIỆU GRPC (Code cũ) ---
            var vehicle = await _vehicleGrpcServiceClient.GetVehicleInstanceByIdAsync(testDrive.VehicleInstanceId);
            var customer = await _customerGrpcServiceClient.GetCustomerByIdAsync(testDrive.CustomerId);

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

            // --- KIỂM TRA TRÙNG NGÀY (MỚI) ---
            // Chỉ kiểm tra nếu người dùng *thay đổi* ngày hẹn sang một ngày khác
            if (request.AppointmentDate.HasValue && request.AppointmentDate.Value.Date != testDrive.AppointmentDate?.Date)
            {
                var requestedDate = request.AppointmentDate.Value.Date;

                var existingBookingOnDate = await _testDriveRepository.FindAsync(td =>
                    td.VehicleInstanceId == testDrive.VehicleInstanceId && // Cùng xe
                    td.Id != id && // Loại trừ chính lịch hẹn đang update
                    td.Status == "Scheduled" &&
                    td.AppointmentDate.HasValue &&
                    td.AppointmentDate.Value.Date == requestedDate
                );

                if (existingBookingOnDate.Any())
                {
                    // Nếu tìm thấy bất kỳ lịch nào trong ngày đó, ném lỗi
                    throw new InvalidOperationException($"Lịch trùng! Xe (ID: {testDrive.VehicleInstanceId}) đã có lịch hẹn lái thử vào ngày {requestedDate:yyyy-MM-dd}.");
                }

                // Nếu không trùng, gán ngày mới
                testDrive.AppointmentDate = request.AppointmentDate.Value;
            }
            // --- KẾT THÚC KIỂM TRA ---

            // Cập nhật các trường còn lại
            if (!string.IsNullOrWhiteSpace(request.Status))
                testDrive.Status = request.Status;
            if (!string.IsNullOrWhiteSpace(request.Notes))
                testDrive.Notes = request.Notes;
            if (!string.IsNullOrWhiteSpace(request.Feedback))
                testDrive.Feedback = request.Feedback;

            testDrive.UpdateAt = DateTime.UtcNow;

            _testDriveRepository.Update(testDrive);
            await _testDriveRepository.SaveChangesAsync();

            // Lấy dữ liệu gRPC
            var vehicle = await _vehicleGrpcServiceClient.GetVehicleInstanceByIdAsync(testDrive.VehicleInstanceId);
            var customer = await _customerGrpcServiceClient.GetCustomerByIdAsync(testDrive.CustomerId);

            var response = MapToResponse(testDrive);
            response.Vehicle = vehicle;
            response.Customer = customer;

            return response;
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
