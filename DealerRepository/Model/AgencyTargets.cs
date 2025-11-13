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

        // Đại lý
        public int AgencyId { get; set; }

        // Xe (nếu target theo từng model)
        public int VehicleId { get; set; }

        // Thời gian đặt mục tiêu
        public int TargetYear { get; set; }
        public int TargetMonth { get; set; }

        // 🎯 Số XE kỳ vọng bán được (thay cho target doanh thu)
        public int TargetUnits { get; set; }

        public int AchievedUnits { get; set; } = 0;


        // For BI / auditing
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Relation
        public Agency Agency { get; set; }
    }
}
