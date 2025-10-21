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
        public int AgencyContractId { get; set; }
        public int VehicleInstanceId { get; set; }
        public DateTime AllocationDate { get; set; }
        public VehicleInstance VehicleInstance { get; set; }
    }
}
