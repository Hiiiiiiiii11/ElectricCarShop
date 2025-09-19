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

    //create model cho dealerContract
    public class CreateDealerContractRequest
    {
        //[Required(ErrorMessage = "DealerId is required")]
        //public int DealerId { get; set; }
        //[Required(ErrorMessage = "ContractNumber is required")]
        public string ContractNumber { get; set; }
        [Required(ErrorMessage = "ContractDate is required")]
        //public DateTime ContractDate { get; set; }
        //[Required(ErrorMessage = "ExpiredDate is required")]
        //public DateTime ExpiredDate { get; set; }
        //[Required(ErrorMessage = "Terms is required")]

        public string Terms { get; set; }
        public string Status { get; set; }
    }
    //response model cho dealerContract
    public class DealerContractResponse {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public string Terms { get; set; }
        public string Status { get; set; }

        // Navigation
        public DealerResponse? Dealer { get; set; }
    }
  
    //request gia hạn hợp đồng 
    public class RenewContractRequest
    {
        [Required(ErrorMessage = "NewTerms is required")]
        public string NewTerms { get; set; }
    }
    //request hủy hợp đồng
    public class TerminateContractRequest
    {
        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }
    }
    //request cập nhật trạng thái hợp đồng
    public class UpdateStatusDealerContractRequest
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
    }

    //thêm công nợ 
    public class AddDealerDebtRequest
    {
        public decimal Amount { get; set; }
    }

    // Request khi thanh toán
    public class MakePaymentRequest
    {
        public decimal Amount { get; set; }
    }
    //model response của dealer debt
    // Response trả về cho FE
    public class DealerDebtResponse
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public decimal TotalDebt { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingDebt { get; set; }
        public DateTime LastUpdate { get; set; }

        // Optionally trả về luôn Dealer info
        public DealerResponse Dealer { get; set; }
    }
    //create dealer target request
    public class CreateDealerTargetRequest
    {
        public int TargetYear { get; set; }
        public int TargetMonth { get; set; }
        public int TargetSales { get; set; }
    }
    //request lấy báo cáo doanh số theo đại lý
    public class GetTargetReportRequest
    {
        public int? TargetYear { get; set; }
        public int? TargetMonth { get; set; }
    }
    //response báo cáo doanh số theo đại lý
    public class DealerTargetReportResponse
    {
        public int Id { get; set; }
        public int DealerId { get; set; }
        public int TargetYear { get; set; }
        public int TargetMonth { get; set; }
        public int TargetSales { get; set; }
        public int AchievedSales { get; set; }
        public DealerResponse? Dealer { get; set; }
    }
    //request update dealer target
    public class UpdateDealerTargetRequest
    {
        public int? TargetYear { get; set; }
        public int? TargetMonth { get; set; }
        public int? TargetSales { get; set; }
        public int? AchievedSales { get; set; }
    }
}
