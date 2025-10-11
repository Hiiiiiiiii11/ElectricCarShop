using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Model
{
    public class Agency
    {
        public int Id { get; set; }
        public string AgencyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }

        public ICollection<AgencyContracts> Contracts { get; set; } = new List<AgencyContracts>();
        public ICollection<AgencyDebts> Debts { get; set; } = new List<AgencyDebts>();
        public ICollection<AgencyTargets> Targets { get; set; } = new List<AgencyTargets>();
        public ICollection<AgencyInventory> Inventories { get; set; } = new List<AgencyInventory>();
        public ICollection<TestDrive> TestDrives { get; set; }= new List<TestDrive>();

    }
}
