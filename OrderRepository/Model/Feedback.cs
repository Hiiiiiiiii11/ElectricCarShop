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
        public Customers Customer { get; set; }
        public string Type { get; set; }    
        public string Content { get; set; }
        public string Reply { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public string Status { get; set; }


    }
}
