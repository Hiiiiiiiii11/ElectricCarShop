using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public class TestDriveService : ITestDriveService
    {
        private readonly ITestDriveRepository _repository;
        public TestDriveService(ITestDriveRepository repository)
        {
            _repository = repository;
        }
            public Task<TestDriveResponse> GetTestDriveScheduleByVehilceId(int vehicleId)
        {
            throw new NotImplementedException();
        }
    }
}
