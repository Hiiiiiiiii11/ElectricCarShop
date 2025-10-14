using OrderRepository.Model;
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
        public Customers Customer { get; set; }
        public int VehicleId { get; set; }
        public string QuotationName { get; set; }
        public decimal QuotedPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; } 
        public string Status { get; set; }


        // 🔗 Một báo giá có thể trở thành nhiều đơn hàng
        public ICollection<Orders> Orders { get; set; } = new List<Orders>();
        public ICollection<Contracts> Contracts { get; set; } = new List<Contracts>();

    }
}
