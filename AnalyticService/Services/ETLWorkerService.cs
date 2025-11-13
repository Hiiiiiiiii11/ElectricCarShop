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

namespace AnalyticService.Services
{
    public class ETLWorkerService : BackgroundService
    {
        private readonly ILogger<ETLWorkerService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Random _random = new Random(); // Dùng để tạo dữ liệu giả

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
                if (runManually) _logger.LogInformation("Manual ETL trigger detected.");

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

                        // Kiểm tra xem có cần chạy theo lịch không (23 tiếng)
                        bool runScheduled = !status.LastSuccessfulRun.HasValue ||
                                            status.LastSuccessfulRun.Value.AddHours(23) < DateTime.UtcNow;

                        // KIỂM TRA BACKFILL (CHỈ CHẠY 1 LẦN)
                        // Nếu chưa chạy lần nào (LastSuccessfulRun == null), thì Backfill
                        bool needsBackfill = !status.LastSuccessfulRun.HasValue;

                        if (runManually || runScheduled || needsBackfill)
                        {
                            _logger.LogInformation("ETL process starting at: {time}", DateTimeOffset.Now);
                            await analyticRepo.SetEtlStatusAsync(true, "ETL process started.");

                            // === BƯỚC 1: LẤP ĐẦY DỮ LIỆU LỊCH SỬ (NẾU CẦN) ===
                            if (needsBackfill)
                            {
                                // Sửa: Chạy từ T1/2025 đến T9/2025 theo yêu cầu
                                await RunBackfillProcess(scope, stoppingToken);
                            }

                            // === BƯỚC 2: CHẠY ETL CHO THÁNG TRƯỚC ===
                            // (Hôm nay T11/2025, chạy cho T10/2025)
                            var processFullDate = DateTime.UtcNow.AddMonths(-1);
                            await RunEtlProcess(scope, stoppingToken, processFullDate);

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

                // Chờ 10 phút rồi kiểm tra lại
                _logger.LogInformation("Next ETL check in 10 minutes.");
                await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
            }
        }

        // === HÀM MỚI: TẠO DỮ LIỆU GIẢ (T1/2025 - T9/2025) ===
        private async Task RunBackfillProcess(IServiceScope scope, CancellationToken stoppingToken)
        {
            var analyticRepo = scope.ServiceProvider.GetRequiredService<IAnalyticRepository>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ETLWorkerService>>();

            logger.LogWarning("ETL Backfill: Generating mock data from Jan 2025 to Sep 2025...");

            var startDate = new DateTime(2025, 1, 1);
            var endDate = new DateTime(2025, 9, 1);

            // Dữ liệu giả (Hardcoded)
            // (Sử dụng ID thật từ dữ liệu JSON bạn đã cung cấp)
            var vehicleModels = new List<dynamic> {
                new { Id = 1, Battery = "100kWh", Range = 610, Price = 1250000000 },
                new { Id = 2, Battery = "100kWh", Range = 610, Price = 1250000000 },
                new { Id = 3, Battery = "82kWh", Range = 510, Price = 1050000000 },
                new { Id = 4, Battery = "110kWh", Range = 550, Price = 1600000000 },
                new { Id = 5, Battery = "110kWh", Range = 550, Price = 1600000000 },
                new { Id = 6, Battery = "90kWh", Range = 480, Price = 1350000000 },
                new { Id = 7, Battery = "50kWh", Range = 350, Price = 550000000 },
                new { Id = 8, Battery = "50kWh", Range = 350, Price = 550000000 },
                new { Id = 9, Battery = "130kWh", Range = 500, Price = 1800000000 },
                new { Id = 10, Battery = "110kWh", Range = 420, Price = 1500000000 }
            };
            var agencyModels = new List<dynamic> {
                new { Id = 1, Location = "21.0175, 105.8222" }, // Hà Nội
                new { Id = 2, Location = "10.7850, 106.6890" }, // Sài Gòn
                new { Id = 3, Location = "16.0618, 108.2220" }  // Đà Nẵng
            };

            var allHistoricalData = new List<Monthly_Demand_Features>();

            for (var date = startDate; date <= endDate; date = date.AddMonths(1))
            {
                if (stoppingToken.IsCancellationRequested) return;

                logger.LogInformation("Backfilling mock data for {Year}-{Month}...", date.Year, date.Month);
                var featuresForThisMonth = new List<Monthly_Demand_Features>();

                foreach (var v in vehicleModels)
                {
                    foreach (var a in agencyModels)
                    {
                        // Tạo dữ liệu ngẫu nhiên
                        int unitsSold = _random.Next(5, 40) + (date.Month % 3 == 0 ? 15 : 0); // Tăng đột biến theo quý
                        int testDrives = unitsSold * 2 + _random.Next(-10, 10);

                        // Tạo feature và thêm vào cả 2 danh sách
                        var feature = CreateMockFeature(date, v.Id, a.Id, v.Battery, v.Range, a.Location, unitsSold, v.Price, testDrives, allHistoricalData);
                        featuresForThisMonth.Add(feature);
                        allHistoricalData.Add(feature); // Thêm vào lịch sử tổng
                    }
                }
                // Ghi đè dữ liệu của tháng này
                await analyticRepo.UpsertFeaturesForMonthAsync(date.Year, date.Month, featuresForThisMonth);
            }
        }

        // Helper tạo dữ liệu giả
        private Monthly_Demand_Features CreateMockFeature(DateTime date, int vId, int aId, string battery, int range, string region, int unitsSold, decimal price, int testDrives, List<Monthly_Demand_Features> historicalData)
        {
            var lastMonth = date.AddMonths(-1);
            var lastYear = date.AddYears(-1);

            // Tìm trong danh sách lịch sử (đã bao gồm các tháng backfill trước đó)
            var lastMonthData = historicalData.FirstOrDefault(h => h.VehicleId == vId && h.AgencyId == aId && h.Year == lastMonth.Year && h.Month == lastMonth.Month);
            var lastYearData = historicalData.FirstOrDefault(h => h.VehicleId == vId && h.AgencyId == aId && h.Year == lastYear.Year && h.Month == lastYear.Month);

            var rollingData = historicalData
                .Where(h => h.VehicleId == vId && h.AgencyId == aId && (new DateTime(h.Year, h.Month, 1) < date))
                .OrderByDescending(h => h.Year).ThenByDescending(h => h.Month)
                .Select(h => h.UnitsSold)
                .ToList();

            return new Monthly_Demand_Features
            {
                Year = date.Year,
                Month = date.Month,
                VehicleId = vId,
                AgencyId = aId,
                UnitsSold = unitsSold,
                UnitsSoldLastMonth = lastMonthData?.UnitsSold ?? 0,
                UnitsSoldSameMonthLastYear = lastYearData?.UnitsSold ?? 0,
                RollingAvgSales3Months = rollingData.Take(3).DefaultIfEmpty(0).Average(),
                RollingAvgSales6Months = rollingData.Take(6).DefaultIfEmpty(0).Average(),
                AvgPrice = price,
                WasOnPromotion = (date.Month % 3 == 0),
                PromotionDiscountAmount = (date.Month % 3 == 0) ? 20000000 : 0,
                TestDrivesCount = testDrives,
                QuotationsAcceptedCount = (int)(testDrives * 0.7),
                AgencyOrdersQuantity = unitsSold, // Dùng proxy
                AgencyTarget = unitsSold + 5,
                VehicleBatteryCapacity = battery,
                VehicleRangeKM = range,
                AgencyRegion = region,
                LastUpdatedAt = DateTime.UtcNow
            };
        }


        // === HÀM LOGIC ETL CHÍNH (Xử lý data thật) ===
        private async Task RunEtlProcess(IServiceScope scope, CancellationToken stoppingToken, DateTime processFullDate)
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

                // Chạy song song các lệnh gọi gRPC
                var vehiclesTask = GrpcStreamHelper.ReadAllAsync(vehicleClient.GetAllVehicleInstances(new GetAllVehicleInstanceRequest()), stoppingToken);
                var agenciesTask = GrpcStreamHelper.ReadAllAsync(agencyClient.GetAllAgencies(new GetAllAgencyRequest()), stoppingToken);
                var pricesTask = GrpcStreamHelper.ReadAllAsync(vehicleClient.GetAllVehiclePrices(new GetAllVehiclePriceRequest()), stoppingToken);
                var promotionsTask = GrpcStreamHelper.ReadAllAsync(vehicleClient.GetAllVehiclePromotions(new GetAllVehiclePromotionRequest()), stoppingToken);
                var targetsTask = GrpcStreamHelper.ReadAllAsync(agencyClient.GetAllAgencyTargets(new GetAllAgencyTargetRequest()), stoppingToken);
                var testDrivesTask = GrpcStreamHelper.ReadAllAsync(agencyClient.GetAllTestDrives(new GetAllTestDriveRequest()), stoppingToken);
                var ordersTask = GrpcStreamHelper.ReadAllAsync(orderClient.GetAllOrders(new GetAllOrderRequest()), stoppingToken);
                var quotationsTask = GrpcStreamHelper.ReadAllAsync(orderClient.GetAllQuotations(new GetAllQuotationRequest()), stoppingToken);
                var agencyOrdersTask = GrpcStreamHelper.ReadAllAsync(agencyClient.GetAllAgencyOrders(new GetAllAgencyOrderRequest()), stoppingToken);

                await Task.WhenAll(vehiclesTask, agenciesTask, pricesTask, promotionsTask, targetsTask, testDrivesTask, ordersTask, quotationsTask, agencyOrdersTask);

                var vehicles = await vehiclesTask;
                var agencies = await agenciesTask;
                var prices = await pricesTask;
                var promotions = await promotionsTask;
                var targets = await targetsTask;
                var testDrives = await testDrivesTask;
                var orders = await ordersTask;
                var quotations = await quotationsTask;
                var agencyOrders = await agencyOrdersTask; // (Dù không dùng nhưng vẫn lấy)

                logger.LogInformation($"Extracted: {agencies.Count} agencies, {vehicles.Count} vehicle instances, {orders.Count} customer orders.");

                // === 2. TRANSFORM (Chuyển đổi) ===
                logger.LogInformation("ETL Step 2: Transforming data for {Year}-{Month}...", processFullDate.Year, processFullDate.Month);

                var processYear = processFullDate.Year;
                var processMonth = processFullDate.Month;

                // Lấy TẤT CẢ Lịch sử (bao gồm cả data giả vừa tạo)
                var historicalData = (await analyticRepo.GetHistoricalFeaturesForLagging(DateTime.UtcNow)).ToList();

                // --- CHUẨN BỊ LOOKUP TABLES ---
                var instanceToModelLookup = vehicles.ToDictionary(v => v.Id, v => v.VehicleId);
                var vehicleModels = vehicles.Where(v => v.VehicleId > 0).Select(v => new { v.VehicleId, v.BatteryCapacity, v.RangeKM }).Distinct().ToList();
                var agencyModels = agencies.Select(a => new { a.Id, a.Location }).Distinct().ToList();
                var featuresToLoad = new List<Monthly_Demand_Features>();

                foreach (var vehicleModel in vehicleModels)
                {
                    var vehicleId = vehicleModel.VehicleId;
                    // Lấy các xe (instances) thuộc model (loại xe) này
                    var instancesForThisModel = vehicles.Where(v => v.VehicleId == vehicleId).Select(v => v.Id).ToHashSet();
                    if (!instancesForThisModel.Any()) continue; // Bỏ qua nếu model này không có xe instance

                    foreach (var agency in agencyModels)
                    {
                        var agencyId = agency.Id;

                        // --- BẮT ĐẦU TÍNH TOÁN FEATURES ---

                        // 1. Quotations & UnitsSold
                        var acceptedQuotes = quotations.Where(q =>
                           q.AgencyId == agencyId &&
                           instancesForThisModel.Contains(q.VehicleInstanceId) &&
                           q.Status == "Accepted" &&
                           IsDateInProcessMonth(q.CreatedDate, processYear, processMonth)
                        ).ToList();
                        int quotationsAcceptedCount = acceptedQuotes.Count;
                        var customerIdsFromQuotes = acceptedQuotes.Select(q => q.CustomerId).ToHashSet();
                        int unitsSold = orders.Count(o =>
                            customerIdsFromQuotes.Contains(o.CustomerId) &&
                            o.Status == "Completed" &&
                            IsDateInProcessMonth(o.OrderDate, processYear, processMonth)
                        );

                        // 2. TestDrives
                        int testDrivesCount = testDrives.Count(t =>
                            t.AgencyId == agencyId &&
                            instancesForThisModel.Contains(t.VehicleInstanceId) &&
                            t.Status == "Completed" &&
                            IsDateInProcessMonth(t.AppointmentDate, processYear, processMonth)
                        );

                        // 3. Price
                        var price = prices.FirstOrDefault(p => p.VehicleId == vehicleId && p.OptionalAgencyIdCase == VehiclePriceReply.OptionalAgencyIdOneofCase.AgencyId && p.AgencyId == agencyId && IsDateInRange(p.StartDate, p.EndDate, processFullDate))
                                 ?? prices.FirstOrDefault(p => p.VehicleId == vehicleId && p.OptionalAgencyIdCase == VehiclePriceReply.OptionalAgencyIdOneofCase.None && IsDateInRange(p.StartDate, p.EndDate, processFullDate));
                        decimal avgPrice = (decimal)(price?.PriceAmount ?? 0);

                        // 4. Promotion
                        var promo = promotions.FirstOrDefault(p => p.VehicleId == vehicleId && IsDateInRange(p.StartDate, p.EndDate, processFullDate));
                        bool wasOnPromo = promo != null;
                        decimal promotionDiscountAmount = (decimal)(promo?.DiscountAmount ?? 0);

                        // 6. AgencyTarget
                        int agencyTarget = targets.FirstOrDefault(t => t.AgencyId == agencyId && t.TargetYear == processYear && t.TargetMonth == processMonth)?.TargetSales ?? 0;

                        // 7. AgencyOrdersQuantity (Proxy)
                        int agencyOrdersQuantity = agencyTarget; // Dùng proxy

                        // 8. Lag
                        var lastMonth = processFullDate.AddMonths(-1);
                        var lastYear = processFullDate.AddYears(-1);
                        var lastMonthData = historicalData.FirstOrDefault(h => h.VehicleId == vehicleId && h.AgencyId == agencyId && h.Year == lastMonth.Year && h.Month == lastMonth.Month);
                        var lastYearData = historicalData.FirstOrDefault(h => h.VehicleId == vehicleId && h.AgencyId == agencyId && h.Year == lastYear.Year && h.Month == lastYear.Month);
                        int unitsSoldLastMonth = lastMonthData?.UnitsSold ?? 0;
                        int unitsSoldSameMonthLastYear = lastYearData?.UnitsSold ?? 0;

                        // 9. Rolling
                        double rollingAvg3Months = GetRollingAverage(historicalData, vehicleId, agencyId, processFullDate, 3);
                        double rollingAvg6Months = GetRollingAverage(historicalData, vehicleId, agencyId, processFullDate, 6);

                        featuresToLoad.Add(new Monthly_Demand_Features
                        {
                            Year = processYear,
                            Month = processMonth,
                            VehicleId = vehicleId,
                            AgencyId = agencyId,
                            UnitsSold = unitsSold,
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
                            AgencyRegion = agency.Location,
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
                throw;
            }
        }

        // (Các hàm Helper: IsDateInRange, IsDateInProcessMonth, GetRollingAverage)
        private bool IsDateInRange(string start, string end, DateTime processDate)
        {
            try
            {
                var startDate = DateTime.Parse(start).Date; var endDate = DateTime.Parse(end).Date;
                var monthStart = new DateTime(processDate.Year, processDate.Month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);
                return startDate <= monthEnd && endDate >= monthStart;
            }
            catch { return false; }
        }
        private bool IsDateInProcessMonth(string dateString, int processYear, int processMonth)
        {
            try
            {
                var date = DateTime.Parse(dateString);
                return date.Year == processYear && date.Month == processMonth;
            }
            catch { return false; }
        }
        private double GetRollingAverage(List<Monthly_Demand_Features> historicalData, int vehicleId, int agencyId, DateTime currentDate, int months)
        {
            double totalSales = 0; int monthsFound = 0;
            for (int i = 1; i <= months; i++)
            {
                var dateToFind = currentDate.AddMonths(-i);
                var historicalEntry = historicalData.FirstOrDefault(h => h.VehicleId == vehicleId && h.AgencyId == agencyId && h.Year == dateToFind.Year && h.Month == dateToFind.Month);
                if (historicalEntry != null) { totalSales += historicalEntry.UnitsSold; monthsFound++; }
            }
            return (monthsFound > 0) ? (totalSales / monthsFound) : 0;
        }
    }

    // Class helper để đọc gRPC stream
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
            catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled || ex.StatusCode == StatusCode.Unavailable) { }
            catch (OperationCanceledException) { }
            return list;
        }
    }
}