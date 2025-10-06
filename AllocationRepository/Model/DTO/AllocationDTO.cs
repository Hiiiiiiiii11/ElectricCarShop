using System;
using System.Collections.Generic;
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
        public string Color { get; set; }
        public string BatteryCapacity { get; set; }
        public int RangeKM { get; set; }
        public string Features { get; set; }
        public string Status { get; set; }

        public VehicleOptionResponse? Option { get; set; }
        public List<AllocationResponse> Allocations { get; set; } = new();
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
    // request model for allocation
    public class AllocationRequest
    {
        public int EvInventoryId { get; set; }
        public int DealerId { get; set; }
        public int VehicleId { get; set; }
        public int AllocationQuantity { get; set; }
    }
    //response model for allocation
    public class AllocationResponse
    {
        public int Id { get; set; }
        public int EvInventoryId { get; set; }
        public int DealerId { get; set; }
        public int VehicleId { get; set; }
        public int AllocationQuantity { get; set; }
        public DateTime AllocationDate { get; set; }
    }
}
