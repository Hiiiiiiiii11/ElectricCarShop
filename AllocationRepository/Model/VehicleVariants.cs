using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Model
{
    public class VehicleVariants
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string VariantName { get; set; }
        public string Color { get; set; }
        public string BatteryCapacity { get; set; }
        public int RangeKM { get; set; }
        public string Features { get; set; }

    }
}
