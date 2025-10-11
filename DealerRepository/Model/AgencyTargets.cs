using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Model
{
    public class AgencyTargets
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public int TargetYear { get; set; }
        public int TargetMonth { get; set; }
        public int TargetSales { get; set; }
        public int AchievedSales{ get; set; }
        public Agency Agency { get; set; }
    }
}
