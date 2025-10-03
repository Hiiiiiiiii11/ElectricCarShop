using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Model
{
    public class AgencyInventory
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public int VehicleId { get; set; }
        public int Quantity { get; set; }
        // Navigation
        public Agency Agency { get; set; }
    }
}
