using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserRepository.Model.DTO
{

    //requet gửi mã otp
    public class VerifyOTPRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

}
