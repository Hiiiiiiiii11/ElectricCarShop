using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Model
{
    public class Quotations
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public int CustomerId { get; set; }
        public int VehicleId { get; set; }
        public string QuotationName { get; set; }
        public decimal QuotedPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } 
        public string Status { get; set; }


        public Vehicles Vehicle { get; set; }
    }
}
