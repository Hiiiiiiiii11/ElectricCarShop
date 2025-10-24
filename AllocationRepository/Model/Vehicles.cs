using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Model
{
    public class Vehicles

    {
        public int Id { get; set; }
        public int VehicleOptionId { get; set; }
        public VehicleOptions VehicleOption { get; set; }
        public string VariantName { get; set; }
        public string VehicleImage { get; set; }
        public string Color { get; set; }
        public string BatteryCapacity { get; set; }
        public int RangeKM { get; set; }
        public string Features { get; set; }
        public string Status { get; set; }

        public ICollection<VehiclePromotions> VehiclePromotions { get; set; } = new List<VehiclePromotions>();
        public ICollection<VehiclePrices> VehiclePrices { get; set; } = new List<VehiclePrices>();
        public ICollection<VehicleInstance> VehicleInstances { get; set; } = new List<VehicleInstance>();


    }
}
