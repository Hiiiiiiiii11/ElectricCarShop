using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Model
{
    public class AgencyDebts
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public int AgencyContractId { get; set; }
        public decimal DebtAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime DueDate { get; set; }
        public string Status {  get; set; }
        public string Notes { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }

        // Navigation
        public Agency Agency { get; set; }
        public AgencyContracts AgencyContract { get; set; }
    }
}
