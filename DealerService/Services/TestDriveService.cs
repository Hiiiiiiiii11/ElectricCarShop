
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
        private readonly IVehicleGrpcServiceClient _vehicleGrpcClient;
        private readonly ICustomerGrpcServiceClient _customerGrpcClient; // Thêm client mới

        public TestDriveService(
            ITestDriveRepository testDriveRepository,
            IVehicleGrpcServiceClient vehicleGrpcClient,
            ICustomerGrpcServiceClient customerGrpcClient) // Inject client mới
        {
            _testDriveRepository = testDriveRepository;
            _vehicleGrpcClient = vehicleGrpcClient;
            _customerGrpcClient = customerGrpcClient;
        }

        // ... Các phương thức Create, Update, Delete giữ nguyên ...
        public async Task<TestDriveResponse> CreateTestDriveAsync(CreateTestDriveRequest request)
        {
            var newTestDrive = new TestDrive
            {
                AgencyId = request.AgencyId,
                VehicleId = request.VehicleId,
                CustomerId = request.CustomerId, // Thêm CustomerId
                AppointmentDate = request.AppointmentDate,
                Notes = request.Notes,
                Status = "Scheduled", // Default status
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            await _testDriveRepository.AddAsync(newTestDrive);
            await _testDriveRepository.SaveChangesAsync();

            var createdTestDrive = await _testDriveRepository.GetDetailByIdAsync(newTestDrive.Id);
            return await MapToResponse(createdTestDrive);
        }

        public async Task<TestDriveResponse> UpdateTestDriveAsync(int id, UpdateTestDriveRequest request)
        {
            var testDrive = await _testDriveRepository.GetByIdAsync(id);
            if (testDrive == null)
            {
                throw new KeyNotFoundException($"Test drive with ID {id} not found.");
            }

            testDrive.AppointmentDate = request.AppointmentDate ?? testDrive.AppointmentDate;
            testDrive.Status = request.Status ?? testDrive.Status;
            testDrive.Notes = request.Notes ?? testDrive.Notes;
            testDrive.Feedback = request.Feedback ?? testDrive.Feedback;
            testDrive.UpdateAt = DateTime.UtcNow;

            _testDriveRepository.Update(testDrive);
            await _testDriveRepository.SaveChangesAsync();

            var updatedTestDrive = await _testDriveRepository.GetDetailByIdAsync(id);
            return await MapToResponse(updatedTestDrive);
        }

        public async Task<bool> DeleteTestDriveAsync(int id)
        {
            var testDrive = await _testDriveRepository.GetByIdAsync(id);
            if (testDrive == null)
            {
                throw new KeyNotFoundException($"Test drive with ID {id} not found.");
            }
            _testDriveRepository.Remove(testDrive);
            await _testDriveRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<TestDriveResponse>> GetAllTestDrivesAsync()
        {
            var testDrives = await _testDriveRepository.GetAllAsync();
            var responseTasks = testDrives.Select(td => MapToResponse(td));
            return await Task.WhenAll(responseTasks);
        }

        public async Task<TestDriveResponse?> GetTestDriveByIdAsync(int id)
        {
            var testDrive = await _testDriveRepository.GetDetailByIdAsync(id);
            if (testDrive == null)
            {
                return null;
            }
            return await MapToResponse(testDrive);
        }

        // Private mapping method
        private async Task<TestDriveResponse> MapToResponse(TestDrive td)
        {
            if (td == null) return null;

            // Gọi song song 2 gRPC service để tăng hiệu năng
            var vehicleTask = _vehicleGrpcClient.GetVehicleByIdAsync(td.VehicleId);
            var customerTask = _customerGrpcClient.GetCustomerByIdAsync(td.CustomerId);

            await Task.WhenAll(vehicleTask, customerTask);

            var vehicleInfo = vehicleTask.Result;
            var customerInfo = customerTask.Result;

            return new TestDriveResponse
            {
                Id = td.Id,
                AgencyId = td.AgencyId,
                AgencyName = td.Agency?.AgencyName,
                VehicleId = td.VehicleId,
                Vehicle = vehicleInfo,
                CustomerId = td.CustomerId,
                Customer = customerInfo,
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

