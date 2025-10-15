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
        private readonly IVehicleGrpcServiceClient _vehicleGrpcServiceClient;
        private readonly ICustomerGrpcServiceClient _customerGrpcServiceClient;

        public TestDriveService(
            ITestDriveRepository testDriveRepository,
            IVehicleGrpcServiceClient vehicleGrpcServiceClient,
            ICustomerGrpcServiceClient customerGrpcServiceClient)
        {
            _testDriveRepository = testDriveRepository;
            _vehicleGrpcServiceClient = vehicleGrpcServiceClient;
            _customerGrpcServiceClient = customerGrpcServiceClient;
        }

        // ===== CREATE =====
        public async Task<TestDriveResponse> CreateTestDriveAsync(CreateTestDriveRequest request)
        {
            var testDrive = new TestDrive
            {
                AgencyId = request.AgencyId,
                VehicleId = request.VehicleId,
                CustomerId = request.CustomerId,
                AppointmentDate = request.AppointmentDate,
                Notes = request.Notes,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Scheduled" : request.Status,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            await _testDriveRepository.AddAsync(testDrive);
            await _testDriveRepository.SaveChangesAsync();

            // Lấy thông tin vehicle & customer từ gRPC
            var vehicle = await _vehicleGrpcServiceClient.GetVehicleByIdAsync(testDrive.VehicleId);
            var customer = await _customerGrpcServiceClient.GetCustomerByIdAsync(testDrive.CustomerId);

            var response = MapToResponse(testDrive);
            response.Vehicle = vehicle;
            response.Customer = customer;

            return response;
        }

        // ===== UPDATE =====
        public async Task<TestDriveResponse> UpdateTestDriveAsync(int id, UpdateTestDriveRequest request)
        {
            var testDrive = await _testDriveRepository.GetByIdAsync(id);
            if (testDrive == null)
                throw new KeyNotFoundException($"Test drive with ID {id} not found.");

            if (request.AppointmentDate.HasValue)
                testDrive.AppointmentDate = request.AppointmentDate.Value;
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
            var vehicle = await _vehicleGrpcServiceClient.GetVehicleByIdAsync(testDrive.VehicleId);
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
                var vehicle = await _vehicleGrpcServiceClient.GetVehicleByIdAsync(td.VehicleId);
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

            var vehicle = await _vehicleGrpcServiceClient.GetVehicleByIdAsync(testDrive.VehicleId);
            var customer = await _customerGrpcServiceClient.GetCustomerByIdAsync(testDrive.CustomerId);

            var response = MapToResponse(testDrive);
            response.Vehicle = vehicle;
            response.Customer = customer;

            return response;
        }

        // ===== MAP =====
        public TestDriveResponse MapToResponse(TestDrive td)
        {
            return new TestDriveResponse
            {
                Id = td.Id,
                AgencyId = td.AgencyId,
                AgencyName = td.Agency?.AgencyName,
                VehicleId = td.VehicleId,
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
