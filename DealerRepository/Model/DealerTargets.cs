using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Model
{
    public class DealerTargets
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public decimal TargetYear { get; set; }
        public decimal TargetMonth { get; set; }
        public int TargetSales { get; set; }
        public int AchievedSales{ get; set; }
    }
}
