using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Model
{
    public class DealerInventory
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public int VariantId { get; set; }
        public int Quantity { get; set; }
    }
}
