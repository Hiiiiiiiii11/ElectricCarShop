
using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using GrpcService; // Make sure this is the namespace for your gRPC client
using Share.ShareServices; // Assuming the client wrapper is here
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public class TestDriveService : ITestDriveService
    {
        private readonly ITestDriveRepository _testDriveRepository;
        private readonly IVehicleGrpcServiceClient _vehicleGrpcClient; // gRPC client

        public TestDriveService(ITestDriveRepository testDriveRepository, IVehicleGrpcServiceClient vehicleGrpcClient)
        {
            _testDriveRepository = testDriveRepository;
            _vehicleGrpcClient = vehicleGrpcClient;
        }

        public async Task<TestDriveResponse> CreateTestDriveAsync(CreateTestDriveRequest request)
        {
            var newTestDrive = new TestDrive
            {
                AgencyId = request.AgencyId,
                VehicleId = request.VehicleId,
                AppointmentDate = request.AppointmentDate,
                Notes = request.Notes,
                Status = "Scheduled", // Default status
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };

            await _testDriveRepository.AddAsync(newTestDrive);
            await _testDriveRepository.SaveChangesAsync();

            // Get full details to return
            var createdTestDrive = await _testDriveRepository.GetDetailByIdAsync(newTestDrive.Id);
            return await MapToResponse(createdTestDrive);
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

        public async Task<TestDriveResponse> UpdateTestDriveAsync(int id, UpdateTestDriveRequest request)
        {
            var testDrive = await _testDriveRepository.GetByIdAsync(id);
            if (testDrive == null)
            {
                throw new KeyNotFoundException($"Test drive with ID {id} not found.");
            }

            // Update properties
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

        // Private mapping method
        private async Task<TestDriveResponse> MapToResponse(TestDrive td)
        {
            if (td == null) return null;

            // Call gRPC service to get vehicle information
            VehicleReply vehicleInfo = null;
            try
            {
                vehicleInfo = await _vehicleGrpcClient.GetVehicleByIdAsync(td.VehicleId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching vehicle info for ID {td.VehicleId}: {ex.Message}");
            }

            return new TestDriveResponse
            {
                Id = td.Id,
                AgencyId = td.AgencyId,
                AgencyName = td.Agency?.AgencyName, // Relational data
                VehicleId = td.VehicleId,
                Vehicle = vehicleInfo, // gRPC data
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

