using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Model
{
    public class TestDrive
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public int VehicleId { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public string? Status { get; set; } // e.g., Scheduled, Completed, Canceled
        public string? Notes { get; set; }
        public string? Feedback { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public Agency Agency { get; set; }
    }
}
