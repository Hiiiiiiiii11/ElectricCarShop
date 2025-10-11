using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Model
{
    public class VehiclePrices
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public int? AgencyId { get; set; }
        public decimal PriceType { get; set; }
        public decimal PriceAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public Vehicles Vehicle { get; set; }
    }
}
