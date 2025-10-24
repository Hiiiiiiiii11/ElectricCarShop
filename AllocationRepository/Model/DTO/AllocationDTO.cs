using GrpcService;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Model.DTO
{


    // create vehicle request
    public class CreateVehicleRequest
    {
        public int VehicleOptionId { get; set; }
        public string VariantName { get; set; }
        public IFormFile? VehicleImage { get; set; }
        public string Color { get; set; }
        public string BatteryCapacity { get; set; }
        public int RangeKM { get; set; }
        public string Features { get; set; }
        public string Status { get; set; }
    }

    //model for update vehicle 
    public class UpdateVehicleRequest
    {
        public int? VehicleOptionId { get; set; }
        public string? VariantName { get; set; }
        public IFormFile? VehicleImage { get; set; }
        public string? Color { get; set; }
        public string? BatteryCapacity { get; set; }
        public int? RangeKM { get; set; }
        public string? Features { get; set; }
        public string? Status { get; set; }
    }

    //model for vehicle response
    public class VehicleResponse
    {
        public int Id { get; set; }
        public int VehicleOptionId { get; set; }
        public string VariantName { get; set; }
        public string VehicleImage { get; set; }
        public string Color { get; set; }
        public string BatteryCapacity { get; set; }
        public int RangeKM { get; set; }
        public string Features { get; set; }
        public string Status { get; set; }

        public VehicleOptionResponse? Option { get; set; }
        //public List<AllocationResponse> Allocations { get; set; } = new();
    }

    //model for vehicle option response
    public class VehicleOptionResponse
    {
        public int Id { get; set; }
        public string ModelName { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
    }
    public class VehicleOptionRequest
    {
        public string ModelName { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class UpdateVehicleOptionRequest
    {
        public string ModelName { get; set; }
        public string Description { get; set; } = string.Empty;
    }
    public class AllocationRequestModel
    {
        public int AgencyContractId { get; set; }
        public int VehicleInstanceId { get; set; }
    }

    public class AllocationResponse
    {
        public int Id { get; set; }
        public int AgencyContractId { get; set; }
        public int VehicleInstanceId { get; set; }
        public DateTime AllocationDate { get; set; }
        public AgencyContractReply ContractReply { get; set; }
        public VehicleInstanceResponse? VehicleInstance { get; set; }
    }

    //request create evinventory
    public class EVInventoryRequest
    {
        public int VehicleInstanceId { get; set; }
    }

    public class EVInventoryResponse
    {
        public int Id { get; set; }
        public int VehicleInstanceId { get; set; }
        public VehicleInstanceResponse VehicleInstance { get; set; }
    }

    //request create vehicle price
    public class VehiclePriceRequest
    {
        public int VehicleId { get; set; }
        public int? AgencyId { get; set; }
        public string PriceType { get; set; }
        public decimal PriceAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }
    //request update vehicle price
    public class VehiclePriceUpdateRequest
    {
        public int? VehicleId { get; set; }
        public int? AgencyId { get; set; }
        public string? PriceType { get; set; }
        public decimal? PriceAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    //response model for vehicleprice
    public class VehiclePriceResponse
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VehicleName { get; set; }
        public int? AgencyId { get; set; }
        public string PriceType { get; set; }
        public decimal PriceAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    //request create model for vehicle promotion
    public class VehiclePromotionRequest
    {
        public int VehicleId { get; set; }
        public string PromoName { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    //request update model for vehicle promotion
    public class VehiclePromotionUpdateRequest
    {
        public int? VehicleId { get; set; }
        public string? PromoName { get; set; }
        public decimal? DiscountAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    //response model for vehicle promotion
    public class VehiclePromotionResponse
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string PromoName { get; set; }
        public decimal DiscountAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    //request model for vehicle instance
    public class CreateVehicleInstanceRequest
    {
        [Required(ErrorMessage = "VehicleId is required")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "VIN is required")]
        public string Vin { get; set; }

        [Required(ErrorMessage = "EngineNumber is required")]
        public string EngineNumber { get; set; }

    }
    public class UpdateVehicleInstanceRequest
    {
        public int? VehicleId { get; set; }
        public string? Vin { get; set; }
        public string? EngineNumber { get; set; }
    }

    public class VehicleInstanceResponse
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Vin { get; set; }
        public string EngineNumber { get; set; }
        public VehicleResponse Vehicle { get; set; }
    }
}
