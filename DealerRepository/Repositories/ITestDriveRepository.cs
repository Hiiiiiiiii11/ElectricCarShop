using AgencyRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Repositories
{
    public interface ITestDriveRepository : IGenericRepository<TestDrive>
    {
        Task<IEnumerable<TestDrive>> GetTestDrivesByAgencyIdAsync(int AgencyId);

        // Lấy tất cả lịch lái thử theo Vehicle
        Task<IEnumerable<TestDrive>> GetTestDrivesByVehicleIdAsync(int vehicleId);

        // Lấy lịch lái thử theo ngày (vd: tất cả lịch trong ngày cụ thể)
        Task<IEnumerable<TestDrive>> GetTestDrivesByDateAsync(DateTime date);

        // Lấy lịch lái thử theo khoảng thời gian
        Task<IEnumerable<TestDrive>> GetTestDrivesByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Tìm kiếm theo trạng thái (Scheduled, Completed, Canceled)
        Task<IEnumerable<TestDrive>> GetTestDrivesByStatusAsync(string status);

        // Lấy chi tiết 1 lịch lái thử cụ thể kèm Agency, vehicle
        Task<TestDrive?> GetDetailByIdAsync(int id);

        // Kiểm tra trùng lịch (cùng vehicle cùng giờ)
        Task<bool> IsScheduleConflictAsync(int vehicleId, DateTime appointmentDate);
    }
}
