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
        public int TargetYear { get; set; }
        public int TargetMonth { get; set; }
        public int TargetSales { get; set; }
        public int AchievedSales{ get; set; }
        public Dealers Dealer { get; set; }
    }
}
