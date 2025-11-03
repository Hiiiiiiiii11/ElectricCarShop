using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticRepository.Model
{
    public class Monthly_Demand_Features
    {
        public long Id { get; set; } // Khóa chính cho bảng tổng hợp

        // === 1. ĐỊNH DANH (Identifiers) ===
        // Đây là "khóa nghiệp vụ" xác định một hàng dữ liệu
        public int Year { get; set; }
        public int Month { get; set; }
        public int VehicleId { get; set; } // Khóa ngoại (logic) tới bảng Vehicle
        public int AgencyId { get; set; }  // Khóa ngoại (logic) tới bảng Agency

        // === 2. BIẾN MỤC TIÊU (Target Variable) ===
        // Đây là thứ chúng ta muốn AI dự đoán
        public int UnitsSold { get; set; }

        // === 3. ĐẶC TRƯNG THỜI GIAN (Time-Series Features) ===
        // Dữ liệu quá khứ để AI học các mẫu (patterns)
        public int UnitsSoldLastMonth { get; set; }
        public int UnitsSoldSameMonthLastYear { get; set; }
        public double RollingAvgSales3Months { get; set; }
        public double RollingAvgSales6Months { get; set; }

        // === 4. ĐẶC TRƯNG ĐỘNG (Dynamic Features) ===
        // Các yếu tố thay đổi theo tháng mà bạn thu thập được
        [Column(TypeName = "decimal(18,2)")]
        public decimal AvgPrice { get; set; }            // Giá bán trung bình trong tháng
        public bool WasOnPromotion { get; set; }         // Có khuyến mãi không?

        [Column(TypeName = "decimal(18,2)")]
        public decimal PromotionDiscountAmount { get; set; } // Mức giảm giá trung bình
        public int TestDrivesCount { get; set; }        // Số lượt lái thử
        public int QuotationsAcceptedCount { get; set; }  // Số báo giá được chấp nhận
        public int AgencyOrdersQuantity { get; set; }   // Số lượng đại lý đặt hàng hãng
        public int AgencyTarget { get; set; }           // Chỉ tiêu đại lý được giao

        // === 5. ĐẶC TRƯNG TĨNH (Static Features) ===
        // Các đặc trưng không đổi của xe và đại lý (sao chép sang để AI dễ học)
        public string VehicleBatteryCapacity { get; set; } // VD: "100kWh"
        public int VehicleRangeKM { get; set; }            // VD: 610
        public string AgencyRegion { get; set; }           // VD: "Mien Bac", "Mien Nam"


        // Dấu thời gian cho biết lần cuối hàng này được cập nhật
        public DateTime LastUpdatedAt { get; set; }
    }
}
