using GrpcService;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Model.DTO
{

    //request to create Agency
    public class CreateAgencyRequest
    {
        [Required(ErrorMessage = "AgencyName is required")]
        public string AgencyName { get; set; }
        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; }
        [Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; }
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
    }

    //request to update Agency
    public class UpdateAgencyRequest
    {
        public string? AgencyName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Status { get; set; }
    }
    public class AssignUserAgencyRequest
    {
        public int UserId { get; set; }
    }
    //response model
    public class AgencyResponse
    {
        public int Id { get; set; }   
        public string AgencyName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
        public IEnumerable<UserReply> Users { get; set; }

    }

    public class AgencyWithUsersResponse
    {
        public int AgencyId { get; set; }
        public string AgencyName { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }

        public List<UserResponse> Users { get; set; } = new List<UserResponse>();
    }

    public class UserResponse
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string AvatarUrl { get; set; }


    }

    //create model cho AgencyContract
    public class CreateAgencyContractRequest
    {
        //[Required(ErrorMessage = "AgencyId is required")]
        //public int AgencyId { get; set; }
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
    //response model cho AgencyContract
    public class AgencyContractResponse {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }
        public DateTime ExpiredDate { get; set; }
        public string Terms { get; set; }
        public string Status { get; set; }

        // Navigation
        public AgencyResponse? Agency { get; set; }
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
    public class UpdateStatusAgencyContractRequest
    {
        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
    }

    //thêm công nợ 
    public class AddAgencyDebtRequest
    {
        public decimal Amount { get; set; }
    }

    // Request khi thanh toán
    public class MakePaymentRequest
    {
        public decimal Amount { get; set; }
    }
    //model response của Agency debt
    // Response trả về cho FE
    public class AgencyDebtResponse
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public decimal DebtAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }

        // Optionally trả về luôn Agency info
        public AgencyResponse Agency { get; set; }
    }
    //create Agency target request
    public class CreateAgencyTargetRequest
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
    public class AgencyTargetReportResponse
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public int TargetYear { get; set; }
        public int TargetMonth { get; set; }
        public int TargetSales { get; set; }
        public int AchievedSales { get; set; }
        public AgencyResponse? Agency { get; set; }
    }
    //request update Agency target
    public class UpdateAgencyTargetRequest
    {
        public int? TargetYear { get; set; }
        public int? TargetMonth { get; set; }
        public int? TargetSales { get; set; }
        public int? AchievedSales { get; set; }
    }

    //request create Agency inventory
    public class CreateAgencyInventoryRequest
    {
        public int VehicleId { get; set; }
        public int Quantity { get; set; }
    }
    //request update Agency inventory
    public class UpdateAgencyInventoryRequest
    {
        public int VehicleId { get; set; }
        public int Quantity { get; set; }
    }
    //response Agency inventory
    public class AgencyInventoryResponse
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public int VehicleId { get; set; }
        public int Quantity { get; set; }
        public AgencyResponse? Agency { get; set; }
    }
    //response model cho test drive
    public class TestDriveResponse
    {
        public int Id { get; set; }
        public int AgencyId { get; set; }
        public int VehicleId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } // e.g., Scheduled, Completed, Canceled
        public string Notes { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }
    }

    // create testdrive model
    public class CreateTestDrive
    {
        public int AgencyId { get; set; }
        public int VehicleId { get; set; }
    }
    //update test drive model
    public class UpdateTestDrive
    {
        public int AgencyId { get; set; }
        public int VehicleId { get; set; }
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
    }
}
