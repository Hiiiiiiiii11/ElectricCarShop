using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserRepository.Model
{
    public class EmailVerification
    {
        public int Id { get; set; } 
        public string Email { get; set; }
        public string Code { get; set; }
        public DateTime ExpiredAt { get; set; }
        public bool IsVerified { get; set; } = false;
    }
}
