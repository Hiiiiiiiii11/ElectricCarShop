using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Model
{
    public class DealerDebts
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingDebt{ get; set; }
        public DateTime LastUpdate { get; set; }

        // Navigation
        public Dealers Dealer { get; set; }
    }
}
