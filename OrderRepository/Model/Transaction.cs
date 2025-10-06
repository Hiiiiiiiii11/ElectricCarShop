using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Model
{
    public class Transaction
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }  
        public string TransactionCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public Payments Payment { get; set; }
    }
}
