using GrpcService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalyticRepository.Model.DTO
{
    public class ETLStatusResponse
    {
        public string Status { get; set; } // "Running", "Idle"
        public DateTime? LastSuccessfulRun { get; set; }
        public string Message { get; set; }
    }
    public class DemandFeatureResponse
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int VehicleId { get; set; } // Khóa ngoại (logic) tới bảng Vehicle
        public int AgencyId { get; set; }  // Khóa ngoại (logic) tới bảng Agency

        public int UnitsSold { get; set; }

        // === 3. ĐẶC TRƯNG THỜI GIAN (Time-Series Features) ===
        // Dữ liệu quá khứ để AI học các mẫu (patterns)
        public int UnitsSoldLastMonth { get; set; }
        public int UnitsSoldSameMonthLastYear { get; set; }
        public double RollingAvgSales3Months { get; set; }
        public double RollingAvgSales6Months { get; set; }
        public decimal AvgPrice { get; set; }            // Giá bán trung bình trong tháng
        public bool WasOnPromotion { get; set; }         // Có khuyến mãi không?
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
