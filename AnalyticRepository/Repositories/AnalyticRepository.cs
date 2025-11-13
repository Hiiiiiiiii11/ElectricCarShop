using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnalyticRepository.Data;
using AnalyticRepository.Model;
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;

namespace AnalyticRepository.Repositories
{
    public class AnalyticRepository : GenericRepository<Monthly_Demand_Features>, IAnalyticRepository
    {
        private readonly AnalyticDbContext _context;
        public AnalyticRepository(AnalyticDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<List<Monthly_Demand_Features>> GetFeaturesByDateRange(
    int startYear,
    int startMonth,
    int endYear,
    int endMonth,
    int? vehicleId,
    int? agencyId)
        {
            var query = _context.MonthlyDemandFeatures.AsQueryable();

            // Lọc theo thời gian
            query = query.Where(f =>
                (f.Year > startYear || (f.Year == startYear && f.Month >= startMonth)) &&
                (f.Year < endYear || (f.Year == endYear && f.Month <= endMonth))
            );

            // Lọc theo vehicleId (optional)
            if (vehicleId.HasValue)
            {
                query = query.Where(f => f.VehicleId == vehicleId.Value);
            }

            // Lọc theo agencyId (optional)
            if (agencyId.HasValue)
            {
                query = query.Where(f => f.AgencyId == agencyId.Value);
            }

            return await query.AsNoTracking().ToListAsync();
        }



        public async Task<List<Monthly_Demand_Features>> GetHistoricalFeaturesForLagging(DateTime until)
        {
            // Lấy 13 tháng gần nhất để tính lag 1, 3, 12
            var fromDate = until.AddMonths(-13);
            return await _context.MonthlyDemandFeatures
                .Where(f => f.Year > fromDate.Year || (f.Year == fromDate.Year && f.Month >= fromDate.Month))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<EtlLogs> GetEtlStatusAsync()
        {

           var log = await _context.EtlLogs.FirstOrDefaultAsync(l => l.Id == 1);
if (log == null)
{
    log = new EtlLogs
    {
        IsRunning = false,
        LastStatusMessage = "Initialized",
        LastSuccessfulRun = DateTime.UtcNow
    };
    _context.EtlLogs.Add(log);
    await _context.SaveChangesAsync();
}
return log;
        }

        public async Task SetEtlStatusAsync(bool isRunning, string message, DateTime? lastRunTime = null)
        {
            var log = await _context.EtlLogs.FirstAsync(l => l.Id == 1);

            log.IsRunning = isRunning;
            log.LastStatusMessage = message;
            if (lastRunTime.HasValue)
            {
                log.LastSuccessfulRun = lastRunTime.Value;
            }

            _context.EtlLogs.Update(log);
            await _context.SaveChangesAsync();
        }

        // =================================================================
        // HÀM ĐÃ ĐƯỢC SỬA LỖI
        // =================================================================
        public async Task UpsertFeaturesForMonthAsync(int year, int month, List<Monthly_Demand_Features> features)
        {
            // 1. Tạo một "chiến lược" (strategy) từ DbContext
            var strategy = _context.Database.CreateExecutionStrategy();

            // 2. Yêu cầu chiến lược đó thực thi (và tự động thử lại nếu cần)
            await strategy.ExecuteAsync(async () =>
            {
                // 3. Đặt Transaction thủ công BÊN TRONG chiến lược
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        // 1. Xóa tất cả dữ liệu của tháng đó
                        await _context.MonthlyDemandFeatures
                            .Where(f => f.Year == year && f.Month == month)
                            .ExecuteDeleteAsync();

                        // 2. Thêm dữ liệu mới
                        await _context.MonthlyDemandFeatures.AddRangeAsync(features);

                        // 3. Lưu thay đổi
                        await _context.SaveChangesAsync();

                        // 4. Commit
                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        // 5. Rollback nếu có lỗi
                        await transaction.RollbackAsync();
                        throw; // Ném lỗi ra ngoài để strategy biết là đã thất bại
                    }
                }
            });
        }

    }
}