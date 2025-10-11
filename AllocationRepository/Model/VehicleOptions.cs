using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Model
{
    public class VehicleOptions
    {
        public int Id { get; set; }
        public string ModelName { get; set; }
        public string Description { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public ICollection<Vehicles> Vehicles { get; set; } = new List<Vehicles>();
    }
}
