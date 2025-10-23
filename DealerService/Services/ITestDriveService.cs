using AgencyRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public interface ITestDriveService
    {
        Task<TestDriveResponse?> GetTestDriveByIdAsync(int id);
        Task<IEnumerable<TestDriveResponse>> GetAllTestDrivesAsync();
        Task<IEnumerable<TestDriveResponse>> GetTestDrivesByAgencyIdAsync(int agencyId);
        Task<TestDriveResponse> CreateTestDriveAsync(CreateTestDriveRequest request);
        Task<TestDriveResponse> UpdateTestDriveAsync(int id, UpdateTestDriveRequest request);
        Task<bool> DeleteTestDriveAsync(int id);

    }
}
