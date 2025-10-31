using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Model
{
    public class VehicleInstance
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string Vin { get; set; }
        public string EngineNumber { get; set; }
        public string Status { get; set; }
        public Vehicles Vehicle { get; set; }

        public ICollection<EVInventory> EVInventories { get; set; } = new List<EVInventory>();

        public ICollection<Allocations> Allocations { get; set; } = new List<Allocations>();
    }
}
