using AnalyticRepository.Model.DTO;
using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticService.Services
{
    public interface IAnalyticService
    {
        Task<ETLStatusResponse> GetEtlStatusAsync();
        // Kích hoạt ETL (Không chờ)
        void TriggerEtlManually();
        Task<IEnumerable<DemandFeatureResponse>> GetDemandFeaturesAsync(int startYear, int startMonth, int endYear, int endMonth, int? vehicleId, int? agencyId );
        Task<IEnumerable<OrderReply>> GetAllOrdersAsync();
        Task<IEnumerable<AgencyReply>> GetAllAgencysAsync();
        Task<IEnumerable<VehicleReply>> GetAllVehiclesAsync();
        Task<IEnumerable<VehicleInstanceReply>> GetAllVehicleInstancesAsync();
        Task<IEnumerable<QuotationReply>> GetAllQuotationsAsync();
        Task<IEnumerable<VehiclePriceReply>> GetAllVehiclePricesAsync();
        Task<IEnumerable<VehiclePromotionReply>> GetAllPromotionsAsync();
        Task<IEnumerable<TestDriveReply>> GetAllTestDrivesAsync();
        Task<IEnumerable<AgencyTargetReply>> GetAllAgencyTargetsAsync();
        Task<IEnumerable<AgencyOrderReply>> GetAllAgencyOrdersAsync();


    }
}
