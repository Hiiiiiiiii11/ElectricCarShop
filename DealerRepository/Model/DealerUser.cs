using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Model
{
    public class DealerUser
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public int UserId { get; set; }
        public string Position { get; set; }
    }
}
