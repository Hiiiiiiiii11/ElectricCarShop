using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Model
{
    public class InstallmentItems
    {
        public int Id { get; set; }
        public int InstallmentPlanId { get; set; }
        public int InstallmentNo { get; set; }        // Kỳ thứ mấy
        public DateTime DueDate { get; set; }         // Ngày đến hạn

        public decimal Percentage { get; set; }       // % tổng tiền trong kỳ này
        public decimal AmountDue { get; set; }        // Tổng phải trả
        public decimal PrincipalComponent { get; set; }
        public decimal InterestComponent { get; set; }
        public decimal FeeComponent { get; set; }

        public string Status { get; set; } = "Pending"; // Pending / Paid / Overdue
        public string? Notes { get; set; }

        public InstallmentPlans InstallmentPlans { get; set; }
        public ICollection<InstallmentPayments> Payments { get; set; } = new List<InstallmentPayments>();
    }

}
