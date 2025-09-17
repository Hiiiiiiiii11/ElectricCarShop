using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Model.DTO
{

    //request to create dealer
    public class CreateDealerRequest
    {
        [Required(ErrorMessage = "DealerName is required")]
        public string DealerName { get; set; }
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
    }

    //request to update dealer
    public class UpdateDealerRequest
    {
        public string? DealerName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
    }

    //response model
    public class DealerResponse
    {
        public int Id { get; set; }   
        public string DealerName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }

    }
    //cretae dealer user request model
    public class CreateDealerUserRequest
    {
        [Required(ErrorMessage = "DealerId is required")]
        public int DealerId { get; set; }
        [Required(ErrorMessage = "UserId is required")]
        public int UserId { get; set; }
        [Required(ErrorMessage = "Position is required")]
        public string Position { get; set; }
    }
    //response model cho dealer user
    public class DealerUserResponse
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public int UserId { get; set; }
        public string Position { get; set; }

        // Thông tin user từ UserService (gRPC)
        public string UserName { get; set; }
        public string Email { get; set; }
        public string AvartarUrl { get; set; }
        public string Phone { get; set; }
    }
}
