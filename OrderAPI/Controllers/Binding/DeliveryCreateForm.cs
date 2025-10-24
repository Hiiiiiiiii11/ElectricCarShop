using Microsoft.AspNetCore.Http;

namespace OrderAPI.Controllers.Binding
{
    public class DeliveryCreateForm
    {
        public int OrderId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; } = null!;
        public string? Notes { get; set; }

        // Chọn 1 trong 2: gửi URL sẵn, hoặc upload file
        public string? ImgUrlBefore { get; set; }
        public string? ImgUrlAfter { get; set; }
        public IFormFile? ImgBeforeFile { get; set; }
        public IFormFile? ImgAfterFile { get; set; }
    }

    public class DeliveryUpdateForm
    {
        public DateTime? DeliveryDate { get; set; }
        public string? DeliveryStatus { get; set; }
        public string? Notes { get; set; }

        public string? ImgUrlBefore { get; set; }
        public string? ImgUrlAfter { get; set; }
        public IFormFile? ImgBeforeFile { get; set; }
        public IFormFile? ImgAfterFile { get; set; }
    }
}
