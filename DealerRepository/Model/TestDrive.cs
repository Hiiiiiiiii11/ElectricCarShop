using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Model
{
    public class TestDrive
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public int VehicleId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } // e.g., Scheduled, Completed, Canceled
        public string Notes { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }

        public Dealers Dealer { get; set; }
    }
}
