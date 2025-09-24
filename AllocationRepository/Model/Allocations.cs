using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Model
{
    public class Allocations
    {
        public int Id { get; set; }
        public int EvInventoryId { get; set; }
        public int DealerId { get; set; }
        public int VehicleId { get; set; }
        public int AllocationQuantity { get; set; }
        public DateTime AllocationDate { get; set; }

        public EVInventory EVInventory { get; set; }
        public Vehicles Vehicle { get; set; }
    }
}
