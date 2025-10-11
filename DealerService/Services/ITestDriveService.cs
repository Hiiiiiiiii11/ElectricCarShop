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
        Task<TestDriveResponse> GetTestDriveScheduleByVehilceId(int vehicleId);
        //Task<TestDriveResponse> GetTestDriveScheduleByAgencyId();

    }
}
