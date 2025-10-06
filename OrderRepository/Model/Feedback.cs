using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Model
{
    public class Feedback
    {
        public int Id { get; set; }
        public int CustomerId { get; set; } 
        public string Comment { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string Status { get; set; }

    }
}
