using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Model
{
    public class AgencyContracts
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }
        public DateTime ContractEndDate { get; set; }
        public string Terms { get; set; }
        public string Status { get; set; }

        // Navigation
        public Agency Agency { get; set; }
        public ICollection<AgencyDebts> Debts { get; set; } = new List<AgencyDebts>();
    }
}
