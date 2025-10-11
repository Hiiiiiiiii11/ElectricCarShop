using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Model
{
    public class Contracts
    {
        public int Id { get; set; }
        public int QuotationId { get; set; }
        public string ContractName { get; set; }
        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }
        public string SignedBy { get; set; }
        public string Terms { get; set; }

    }
}
