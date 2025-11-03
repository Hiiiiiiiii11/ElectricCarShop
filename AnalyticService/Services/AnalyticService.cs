
using AnalyticRepository.Model.DTO;
using AnalyticRepository.Repositories;
using Grpc.Core;
using GrpcService;
using Microsoft.Extensions.Logging;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnalyticService.Services
{
    public class AnalyticsService : IAnalyticService
    {
        private readonly IAnalyticRepository _analyticRepo;
        private readonly ILogger<AnalyticsService> _logger;
        private readonly IAgencyGrpcServiceClient _agencyGrpcClient;
        private readonly IVehicleInstanceGrpcServiceClient _vehicleInstanceGrpcServiceClient;
        private readonly IOrderGrpcServiceClient _orderGrpcServiceClient;
        // Biến static (hoặc dùng Caching/Singleton) để ra lệnh cho worker
        private static bool _manualTrigger = false;

        public AnalyticsService(IAnalyticRepository analyticRepo, ILogger<AnalyticsService> logger,
            IAgencyGrpcServiceClient agencyGrpcClient,
            IVehicleInstanceGrpcServiceClient vehicleInstanceGrpcServiceClient,
            IOrderGrpcServiceClient orderGrpcServiceClient
            )
        {
            _analyticRepo = analyticRepo;
            _logger = logger;
            _agencyGrpcClient = agencyGrpcClient;
            _vehicleInstanceGrpcServiceClient = vehicleInstanceGrpcServiceClient;
            _orderGrpcServiceClient = orderGrpcServiceClient;

        }

        public async Task<ETLStatusResponse> GetEtlStatusAsync()
        {
            _logger.LogInformation("Getting ETL status...");
            var log = await _analyticRepo.GetEtlStatusAsync();

            return new ETLStatusResponse
            {
                Status = log.IsRunning ? "Running" : "Idle",
                LastSuccessfulRun = log.LastSuccessfulRun,
                Message = log.LastStatusMessage
            };
        }

        // API chỉ bật cờ, Worker sẽ kiểm tra cờ này
        public void TriggerEtlManually()
        {
            _logger.LogWarning("Manual ETL trigger requested via API...");
            _manualTrigger = true;
        }

        // Hàm này được worker gọi để reset cờ
        public static bool CheckAndResetManualTrigger()
        {
            if (_manualTrigger)
            {
                _manualTrigger = false;
                return true; // Có trigger
            }
            return false; // Không có trigger
        }

        public async Task<IEnumerable<DemandFeatureResponse>> GetDemandFeaturesAsync(int startYear, int startMonth, int endYear, int endMonth)
        {
            var features = await _analyticRepo.GetFeaturesByDateRange(startYear, startMonth, endYear, endMonth);

            // Map từ Model (Database) sang DTO (Response)
            return features.Select(f => new DemandFeatureResponse
            {
                Year = f.Year,
                Month = f.Month,
                VehicleId = f.VehicleId,
                AgencyId = f.AgencyId,
                UnitsSold = f.UnitsSold,
                AvgPrice = f.AvgPrice,
                TestDrivesCount = f.TestDrivesCount,
                WasOnPromotion = f.WasOnPromotion,
                UnitsSoldLastMonth = f.UnitsSoldLastMonth,
                UnitsSoldSameMonthLastYear = f.UnitsSoldSameMonthLastYear,
                RollingAvgSales3Months = f.RollingAvgSales3Months,
                RollingAvgSales6Months = f.RollingAvgSales6Months,
                PromotionDiscountAmount = f.PromotionDiscountAmount,
                QuotationsAcceptedCount = f.QuotationsAcceptedCount,
                AgencyOrdersQuantity = f.AgencyOrdersQuantity,
                AgencyTarget = f.AgencyTarget,
                VehicleBatteryCapacity = f.VehicleBatteryCapacity,
                VehicleRangeKM = f.VehicleRangeKM,
                AgencyRegion = f.AgencyRegion,
                LastUpdatedAt = f.LastUpdatedAt
                
            });
        }

        public async Task<IEnumerable<OrderReply>> GetAllOrdersAsync()
        {
            var results = new List<OrderReply>();
            // Lấy stream
            var call = _orderGrpcServiceClient.GetAllOrders(new GrpcService.GetAllOrderRequest());

            // Đọc stream bằng await foreach
            await foreach (var order in call.ResponseStream.ReadAllAsync())
            {
                results.Add(order);
            }
            return results;
        }

        public async Task<IEnumerable<AgencyReply>> GetAllAgencysAsync()
        {
            var results = new List<AgencyReply>();
            var call = _agencyGrpcClient.GetAllAgencies(new GrpcService.GetAllAgencyRequest());

            await foreach (var agency in call.ResponseStream.ReadAllAsync())
            {
                results.Add(agency);
            }
            return results;
        }

        public async Task<IEnumerable<VehicleReply>> GetAllVehiclesAsync()
        {
            var results = new List<VehicleReply>();
            var call = _vehicleInstanceGrpcServiceClient.GetAllVehicles(new GrpcService.GetAllVehicleRequest());

            await foreach (var item in call.ResponseStream.ReadAllAsync())
            {
                results.Add(item);
            }
            return results;
        }
        public async Task<IEnumerable<VehicleInstanceReply>> GetAllVehicleInstancesAsync()
        {
            var results = new List<VehicleInstanceReply>();
            var call = _vehicleInstanceGrpcServiceClient.GetAllVehicleInstances(new GrpcService.GetAllVehicleInstanceRequest());

            await foreach (var item in call.ResponseStream.ReadAllAsync())
            {
                results.Add(item);
            }
            return results;
        }

        public async Task<IEnumerable<QuotationReply>> GetAllQuotationsAsync()
        {
            var results = new List<QuotationReply>();
            var call = _orderGrpcServiceClient.GetAllQuotations(new GrpcService.GetAllQuotationRequest());

            await foreach (var item in call.ResponseStream.ReadAllAsync())
            {
                results.Add(item);
            }
            return results;
        }

        public async Task<IEnumerable<VehiclePriceReply>> GetAllVehiclePricesAsync()
        {
            var results = new List<VehiclePriceReply>();
            var call = _vehicleInstanceGrpcServiceClient.GetAllVehiclePrices(new GrpcService.GetAllVehiclePriceRequest());

            await foreach (var item in call.ResponseStream.ReadAllAsync())
            {
                results.Add(item);
            }
            return results;
        }

        public async Task<IEnumerable<VehiclePromotionReply>> GetAllPromotionsAsync()
        {
            var results = new List<VehiclePromotionReply>();
            var call = _vehicleInstanceGrpcServiceClient.GetAllVehiclePromotions(new GrpcService.GetAllVehiclePromotionRequest());

            await foreach (var item in call.ResponseStream.ReadAllAsync())
            {
                results.Add(item);
            }
            return results;
        }

        public async Task<IEnumerable<TestDriveReply>> GetAllTestDrivesAsync()
        {
            var results = new List<TestDriveReply>();
            var call = _agencyGrpcClient.GetAllTestDrives(new GrpcService.GetAllTestDriveRequest());

            await foreach (var item in call.ResponseStream.ReadAllAsync())
            {
                results.Add(item);
            }
            return results;
        }

        public async Task<IEnumerable<AgencyTargetReply>> GetAllAgencyTargetsAsync()
        {
            var results = new List<AgencyTargetReply>();
            var call = _agencyGrpcClient.GetAllAgencyTargets(new GrpcService.GetAllAgencyTargetRequest());

            await foreach (var item in call.ResponseStream.ReadAllAsync())
            {
                results.Add(item);
            }
            return results;
        }
    }
}