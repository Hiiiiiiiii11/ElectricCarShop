using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Model
{
    public class AgencyOrder
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public int AgencyContractId { get; set; }
        public int VehicleId { get; set; }
        public int Quantity { get; set; }
        public string Status { get; set; }
        public DateTime OrderDate { get; set; }

        public Agency Agency { get; set; }
        public AgencyContracts AgencyContracts { get; set; }
    }
}
