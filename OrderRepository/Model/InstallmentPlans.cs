using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Model
{
    public class InstallmentPlans
    {
        public int Id { get; set; }
        public int? ContractId { get; set; }
        public int? AgencyContractId { get; set; }

        public decimal PrincipalAmount { get; set; } // Tổng số tiền gốc
        public decimal DepositAmount { get; set; }   // Tiền đặt cọc
        public decimal InterestRate { get; set; }    // % lãi suất
        public string InterestMethod { get; set; }   // "flat", "declining", "none"

        // 🧩 Lưu cấu hình chia kỳ (VD: 3 tháng đầu 50%, 9 tháng sau 50%)
        // Dạng JSON: [{"months":3,"percentage":0.5},{"months":9,"percentage":0.5}]
        public string? RuleJson { get; set; }

        public string Status { get; set; } = "Active";
        [NotMapped]  
        public decimal TotalPaid { get; set; }
        public string? Note { get; set; }
        public DateTime CreateAt { get; set; } = DateTime.Now;
        public DateTime UpdateAt { get; set; } = DateTime.Now;

        public Contracts? Contract { get; set; }

        // 🔗 Quan hệ 1-n: Một kế hoạch có nhiều kỳ trả
        public ICollection<InstallmentItems> Items { get; set; } = new List<InstallmentItems>();

        // 🔗 Quan hệ 1-n: Một kế hoạch có nhiều lần thanh toán
        public ICollection<InstallmentPayments> Payments { get; set; } = new List<InstallmentPayments>();
    }
}
