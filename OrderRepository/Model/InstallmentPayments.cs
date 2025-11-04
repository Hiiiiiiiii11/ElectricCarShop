using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Model
{
    public class InstallmentPayments
    {
        public int Id { get; set; }
        public int InstallmentPlanId { get; set; }
        public int? InstallmentItemId { get; set; } // Nếu null → thanh toán tổng hợp
        public decimal AmountPaid { get; set; }
        public DateTime PaidDate { get; set; }
        public string PaymentMethod { get; set; } = "BankTransfer"; // hoặc Cash, Card,...
        public string Status { get; set; } = "Completed";
        public string? Note { get; set; }

        // 🔗 Quan hệ
        public InstallmentPlans InstallmentPlan { get; set; }
        public InstallmentItems? InstallmentItem { get; set; }
    }

}
