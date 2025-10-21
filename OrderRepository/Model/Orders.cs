using AllocationRepository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Model
{
    public class Orders
    {
        public int Id { get; set; }

        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int CreateBy { get; set; }
        public Customers Customer { get; set; }
        public ICollection<OrderDetail> Details { get; set; }   = new List<OrderDetail>();
    }
}
