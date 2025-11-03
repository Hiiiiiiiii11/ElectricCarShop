using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using AnalyticRepository.Repositories;
using Share.ShareServices;
using AnalyticRepository.Model;
using System.Collections.Generic;
using GrpcService;
using Grpc.Core;
using System.Linq;
using Google.Protobuf.WellKnownTypes; // <-- Thêm using này

namespace AnalyticService.Services
{
    public class ETLWorkerService : BackgroundService
    {
        private readonly ILogger<ETLWorkerService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ETLWorkerService(ILogger<ETLWorkerService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ETL Worker Service is starting.");

            // Chờ 1 phút để các service khác khởi động xong
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                bool runManually = AnalyticsService.CheckAndResetManualTrigger();
                if (runManually)
                {
                    _logger.LogInformation("Manual ETL trigger detected.");
                }

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var analyticRepo = scope.ServiceProvider.GetRequiredService<IAnalyticRepository>();
                        var status = await analyticRepo.GetEtlStatusAsync();

                        if (status.IsRunning)
                        {
                            _logger.LogWarning("ETL process is already running. Skipping this run.");
                            await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                            continue;
                        }

                        bool runScheduled = !status.LastSuccessfulRun.HasValue ||
                                            status.LastSuccessfulRun.Value.AddHours(23) < DateTime.UtcNow;

                        if (runManually || runScheduled)
                        {
                            _logger.LogInformation("ETL process starting at: {time}", DateTimeOffset.Now);
                            await analyticRepo.SetEtlStatusAsync(true, "ETL process started.");

                            // === CHẠY ETL (Bản đầy đủ đã sửa logic) ===
                            await RunEtlProcess(scope, stoppingToken); // Truyền stoppingToken

                            await analyticRepo.SetEtlStatusAsync(false, "ETL process finished successfully.", DateTime.UtcNow);
                            _logger.LogInformation("ETL process finished successfully.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during the ETL process.");
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var analyticRepo = scope.ServiceProvider.GetRequiredService<IAnalyticRepository>();
                        await analyticRepo.SetEtlStatusAsync(false, $"ETL failed: {ex.Message}");
                    }
                }

                _logger.LogInformation("Next ETL check in 10 minutes.");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }

            _logger.LogInformation("ETL Worker Service is stopping.");
        }

        // Hàm logic ETL chính (ĐÃ SỬA LẠI HOÀN CHỈNH)
        private async Task RunEtlProcess(IServiceScope scope, CancellationToken stoppingToken)
        {
            try
            {
                var analyticRepo = scope.ServiceProvider.GetRequiredService<IAnalyticRepository>();
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ETLWorkerService>>();

                // === 1. EXTRACT (Trích xuất) ===
                logger.LogInformation("ETL Step 1: Extracting data via gRPC...");

                var agencyClient = scope.ServiceProvider.GetRequiredService<IAgencyGrpcServiceClient>();
                var vehicleClient = scope.ServiceProvider.GetRequiredService<IVehicleInstanceGrpcServiceClient>();
                var orderClient = scope.ServiceProvider.GetRequiredService<IOrderGrpcServiceClient>();

                // Sửa lỗi: Dùng 'GetAllRequest' chung (hoặc tên đúng trong .proto của bạn)
                var vehicles = await GrpcStreamHelper.ReadAllAsync(vehicleClient.GetAllVehicleInstances(new GetAllVehicleInstanceRequest()), stoppingToken);
                var agencies = await GrpcStreamHelper.ReadAllAsync(agencyClient.GetAllAgencies(new GetAllAgencyRequest()), stoppingToken); // Giả định dùng chung
                var prices = await GrpcStreamHelper.ReadAllAsync(vehicleClient.GetAllVehiclePrices(new GetAllVehiclePriceRequest()), stoppingToken);
                var promotions = await GrpcStreamHelper.ReadAllAsync(vehicleClient.GetAllVehiclePromotions(new GetAllVehiclePromotionRequest()), stoppingToken);
                var targets = await GrpcStreamHelper.ReadAllAsync(agencyClient.GetAllAgencyTargets(new GetAllAgencyTargetRequest()), stoppingToken); // Giả định dùng chung
                var testDrives = await GrpcStreamHelper.ReadAllAsync(agencyClient.GetAllTestDrives(new GetAllTestDriveRequest()), stoppingToken); // Giả định dùng chung
                var orders = await GrpcStreamHelper.ReadAllAsync(orderClient.GetAllOrders(new GetAllOrderRequest()), stoppingToken); // Giả định dùng chung
                var quotations = await GrpcStreamHelper.ReadAllAsync(orderClient.GetAllQuotations(new GetAllQuotationRequest()), stoppingToken); // Giả định dùng chung

                logger.LogInformation($"Extracted: {agencies.Count} agencies, {vehicles.Count} vehicle instances, {orders.Count} orders.");

                // === 2. TRANSFORM (Chuyển đổi) ===
                logger.LogInformation("ETL Step 2: Transforming data...");

                // Xử lý tháng trước
                var processFullDate = DateTime.UtcNow;
                var processYear = processFullDate.Year;
                var processMonth = processFullDate.Month;
                var monthStart = new DateTime(processYear, processMonth, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                var historicalData = (await analyticRepo.GetHistoricalFeaturesForLagging(DateTime.UtcNow)).ToList();

                // --- CHUẨN BỊ LOOKUP TABLES ---

                // Map[InstanceId] -> VehicleId (Model)
                var instanceToModelLookup = vehicles.ToDictionary(v => v.Id, v => v.VehicleId);
                if (instanceToModelLookup.Count == 0)
                {
                    logger.LogWarning("No vehicle instances found. ETL will produce empty features.");
                }

                // Danh sách Model xe (VehicleId) và các đặc trưng tĩnh
                var vehicleModels = vehicles
                    .Where(v => v.VehicleId > 0) // Chỉ lấy instance có map với model
                    .Select(v => new { v.VehicleId, v.BatteryCapacity, v.RangeKM })
                    .Distinct()
                    .ToList();

                var agencyModels = agencies
                    .Select(a => new { a.Id, a.Location })
                    .Distinct()
                    .ToList();

                var featuresToLoad = new List<Monthly_Demand_Features>();

                // Lặp qua từng MODEL XE (VehicleId)
                foreach (var vehicleModel in vehicleModels)
                {
                    var vehicleId = vehicleModel.VehicleId;
                    var instancesForThisModel = vehicles
                        .Where(v => v.VehicleId == vehicleId)
                        .Select(v => v.Id)
                        .ToHashSet();

                    // Lặp qua từng ĐẠI LÝ (AgencyId)
                    foreach (var agency in agencyModels)
                    {
                        var agencyId = agency.Id;

                        // --- BẮT ĐẦU TÍNH TOÁN FEATURES ---

                        // 1. QuotationsAcceptedCount
                        var acceptedQuotes = quotations.Where(q =>
                           q.AgencyId == agencyId &&
                           instancesForThisModel.Contains(q.VehicleInstanceId) &&
                           q.Status == "Accepted" &&
                           IsDateInProcessMonth(q.CreatedDate, processYear, processMonth)
                        ).ToList();
                        int quotationsAcceptedCount = acceptedQuotes.Count;

                        // 2. UnitsSold (LOGIC CẬP NHẬT CỦA BẠN)
                        // Lấy ID khách hàng từ các báo giá đã chấp nhận ở trên
                        var customerIdsFromQuotes = acceptedQuotes.Select(q => q.CustomerId).ToHashSet();

                        // Đếm số Order đã "Completed" trong tháng, khớp với các khách hàng trên
                        int unitsSold = orders.Count(o =>
                            customerIdsFromQuotes.Contains(o.CustomerId) &&
                            o.Status == "Completed" &&
                            IsDateInProcessMonth(o.OrderDate, processYear, processMonth)
                        );

                        // 3. TestDrivesCount
                        int testDrivesCount = testDrives.Count(t =>
                            t.AgencyId == agencyId &&
                            instancesForThisModel.Contains(t.VehicleInstanceId) &&
                            t.Status == "Completed" &&
                            IsDateInProcessMonth(t.AppointmentDate, processYear, processMonth)
                        );

                        // 4. AvgPrice
                        var price = prices.FirstOrDefault(p =>
                            p.VehicleId == vehicleId &&
                            p.OptionalAgencyIdCase == VehiclePriceReply.OptionalAgencyIdOneofCase.AgencyId &&
                            p.AgencyId == agencyId &&
                            IsDateInRange(p.StartDate, p.EndDate, processFullDate)
                        );
                        if (price == null) // Nếu không có giá riêng, lấy giá chung
                        {
                            price = prices.FirstOrDefault(p =>
                                p.VehicleId == vehicleId &&
                                p.OptionalAgencyIdCase == VehiclePriceReply.OptionalAgencyIdOneofCase.None &&
                                IsDateInRange(p.StartDate, p.EndDate, processFullDate)
                            );
                        }
                        decimal avgPrice = (decimal)(price?.PriceAmount ?? 0);

                        // 5. Promotion
                        var promo = promotions.FirstOrDefault(p =>
                            p.VehicleId == vehicleId &&
                            IsDateInRange(p.StartDate, p.EndDate, processFullDate)
                        );
                        bool wasOnPromo = promo != null;
                        decimal promotionDiscountAmount = (decimal)(promo?.DiscountAmount ?? 0);

                        // 6. AgencyTarget
                        int agencyTarget = targets.FirstOrDefault(t =>
                            t.AgencyId == agencyId &&
                            t.TargetYear == processYear &&
                            t.TargetMonth == processMonth)?.TargetSales ?? 0;

                        // 7. AgencyOrdersQuantity
                        // Logic: Dùng chỉ tiêu làm proxy, vì AgencyOrderReply (từ .proto) không có ngày
                        int agencyOrdersQuantity = orders.Count(o =>
                             o.Status == "Completed" &&
                             IsDateInProcessMonth(o.OrderDate, processYear, processMonth)
                         );

                        // 8. Lagged Features (Lấy từ historicalData)
                        var lastMonth = processFullDate.AddMonths(-1);
                        var lastYear = processFullDate.AddYears(-1);

                        var lastMonthData = historicalData
                            .FirstOrDefault(h => h.VehicleId == vehicleId && h.AgencyId == agencyId && h.Year == lastMonth.Year && h.Month == lastMonth.Month);

                        var lastYearData = historicalData
                            .FirstOrDefault(h => h.VehicleId == vehicleId && h.AgencyId == agencyId && h.Year == lastYear.Year && h.Month == lastYear.Month);

                        int unitsSoldLastMonth = lastMonthData?.UnitsSold ?? 0;
                        int unitsSoldSameMonthLastYear = lastYearData?.UnitsSold ?? 0;

                        // 9. Rolling Averages (Lấy từ historicalData)
                        double rollingAvg3Months = GetRollingAverage(historicalData, vehicleId, agencyId, processFullDate, 3);
                        double rollingAvg6Months = GetRollingAverage(historicalData, vehicleId, agencyId, processFullDate, 6);

                        // === TẠO OBJECT FEATURE ===
                        featuresToLoad.Add(new Monthly_Demand_Features
                        {
                            Year = processYear,
                            Month = processMonth,
                            VehicleId = vehicleId,
                            AgencyId = agencyId,
                            UnitsSold = unitsSold, // Target
                            UnitsSoldLastMonth = unitsSoldLastMonth,
                            UnitsSoldSameMonthLastYear = unitsSoldSameMonthLastYear,
                            RollingAvgSales3Months = rollingAvg3Months,
                            RollingAvgSales6Months = rollingAvg6Months,
                            AvgPrice = avgPrice,
                            WasOnPromotion = wasOnPromo,
                            PromotionDiscountAmount = promotionDiscountAmount,
                            TestDrivesCount = testDrivesCount,
                            QuotationsAcceptedCount = quotationsAcceptedCount,
                            AgencyOrdersQuantity = agencyOrdersQuantity,
                            AgencyTarget = agencyTarget,
                            VehicleBatteryCapacity = vehicleModel.BatteryCapacity,
                            VehicleRangeKM = vehicleModel.RangeKM,
                            AgencyRegion = agency.Location, // Giả sử location là region
                            LastUpdatedAt = DateTime.UtcNow
                        });
                    }
                }

                // === 3. LOAD (Tải) ===
                logger.LogInformation($"ETL Step 3: Loading {featuresToLoad.Count} feature rows for {processYear}-{processMonth}...");

                if (featuresToLoad.Any())
                {
                    await analyticRepo.UpsertFeaturesForMonthAsync(processYear, processMonth, featuresToLoad);
                }
                else
                {
                    logger.LogWarning("No features were generated to load.");
                }
            }
            catch (Exception ex)
            {
                var logger = scope.ServiceProvider.GetRequiredService<ILogger<ETLWorkerService>>();
                logger.LogError(ex, "ETL process crashed inside RunEtlProcess");
                throw; // Ném lỗi ra ngoài để catch ngoài cùng xử lý
            }
        }

        // Helper kiểm tra ngày
        private bool IsDateInRange(string start, string end, DateTime processDate)
        {
            // Sửa ở đây: Dùng DateTime.Parse
            var startDate = DateTime.Parse(start);
            var endDate = DateTime.Parse(end);

            // Ngày bắt đầu của tháng xử lý
            var monthStart = new DateTime(processDate.Year, processDate.Month, 1);
            // Ngày kết thúc của tháng xử lý
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            // Logic: Khoảng thời gian (start, end) có giao (overlap) với (monthStart, monthEnd) không
            return startDate <= monthEnd && endDate >= monthStart;
        }
        private bool IsDateInProcessMonth(string dateString, int processYear, int processMonth)
        {
            try
            {
                var date = DateTime.Parse(dateString);
                return date.Year == processYear && date.Month == processMonth;
            }
            catch
            {
                return false; // Lỗi parse ngày
            }
        }


        private double GetRollingAverage(List<Monthly_Demand_Features> historicalData, int vehicleId, int agencyId, DateTime currentDate, int months)
        {
            double totalSales = 0;
            int monthsFound = 0;

            // Lấy dữ liệu của 'months' tháng gần nhất (không bao gồm tháng hiện tại)
            for (int i = 1; i <= months; i++)
            {
                var dateToFind = currentDate.AddMonths(-i);
                var historicalEntry = historicalData
                    .FirstOrDefault(h => h.VehicleId == vehicleId &&
                                         h.AgencyId == agencyId &&
                                         h.Year == dateToFind.Year &&
                                         h.Month == dateToFind.Month);

                if (historicalEntry != null)
                {
                    totalSales += historicalEntry.UnitsSold;
                    monthsFound++;
                }
            }

            // Trả về trung bình (nếu không tìm thấy tháng nào thì trả về 0)
            return (monthsFound > 0) ? (totalSales / monthsFound) : 0;
        }

    }
    internal static class GrpcStreamHelper
    {
        public static async Task<List<T>> ReadAllAsync<T>(AsyncServerStreamingCall<T> streamCall, CancellationToken stoppingToken = default)
        {
            var list = new List<T>();
            try
            {
                await foreach (var item in streamCall.ResponseStream.ReadAllAsync(stoppingToken))
                {
                    list.Add(item);
                }
            }
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
            {
                // Bỏ qua lỗi Cancelled khi service dừng
            }
            return list;
        }
    }
}


   