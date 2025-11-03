using AnalyticRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticRepository.Repositories
{
    public interface IAnalyticRepository : IGenericRepository<Monthly_Demand_Features>
    {
        Task<List<Monthly_Demand_Features>> GetFeaturesByDateRange(int startYear, int startMonth, int endYear, int endMonth);

        // Cập nhật hoặc Thêm mới (Upsert) một lô dữ liệu features cho 1 tháng
        Task UpsertFeaturesForMonthAsync(int year, int month, List<Monthly_Demand_Features> features);

        // Lấy dữ liệu lịch sử để tính toán (ví dụ: 12 tháng gần nhất)
        Task<List<Monthly_Demand_Features>> GetHistoricalFeaturesForLagging(DateTime until);

        // Lấy thông tin ETL
        Task<EtlLogs> GetEtlStatusAsync();

        // Cập nhật trạng thái ETL
        Task SetEtlStatusAsync(bool isRunning, string message, DateTime? lastRunTime = null);
    }
}
