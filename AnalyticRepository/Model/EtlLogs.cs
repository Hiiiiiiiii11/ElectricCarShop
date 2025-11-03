using System;
using System.ComponentModel.DataAnnotations;

namespace AnalyticRepository.Model
{
    // Bảng này chỉ lưu 1 dòng duy nhất,
    // dùng để ghi lại trạng thái của các lần chạy ETL
    public class EtlLogs
    {
        public int Id { get; set; } // Chỉ có 1 dòng với Id = 1
        public DateTime? LastSuccessfulRun { get; set; }
        public bool IsRunning { get; set; }
        public string LastStatusMessage { get; set; }
    }
}