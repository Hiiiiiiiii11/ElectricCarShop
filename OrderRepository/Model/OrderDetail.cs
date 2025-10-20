using AllocationRepository.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Model
{
    public class OrderDetail
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int QuotationId { get; set; }
        public decimal UnitPrice { get; set; }

        public Quotations Quotation { get; set; }
        public Orders Orders { get; set; }
    }
}
