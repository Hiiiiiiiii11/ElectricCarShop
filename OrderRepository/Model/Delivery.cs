using CloudinaryDotNet.Actions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Model
{
    public class Delivery
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; }
        public string Notes { get; set; }
        public string? ImgUrlBefore { get; set; }
        public string? ImgUrlAfter { get; set; }

    }
}
