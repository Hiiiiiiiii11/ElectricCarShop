using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Model
{
    public class Dealers
    {
        public int Id { get; set; }
        public string DealerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }

        public ICollection<DealerContracts> Contracts { get; set; } = new List<DealerContracts>();
        public ICollection<DealerDebts> Debts { get; set; } = new List<DealerDebts>();
        public ICollection<DealerTargets> Targets { get; set; } = new List<DealerTargets>();
        public ICollection<DealerInventory> Inventories { get; set; } = new List<DealerInventory>();
        public ICollection<DealerUser> DealerUsers { get; set; } = new List<DealerUser>();
        public ICollection<TestDrive> TestDrives { get; set; }= new List<TestDrive>();

    }
}
