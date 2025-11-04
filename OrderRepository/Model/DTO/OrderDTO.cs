using GrpcService;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderRepository.Model.Request
{
    public class CustomerRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
    public class CustomerUpdateRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
    }

    public class CustomerResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string CreateAt { get; set; }
    }
    public class FeedbackRequest
    {
        public int CustomerId { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public string? Reply { get; set; }
        public string? Status { get; set; }
    }
    public class FeedbackUpdateRequest
    {
        public int? CustomerId { get; set; }
        public string? Type { get; set; }
        public string? Content { get; set; }
        public string? Reply { get; set; }
        public string? Status { get; set; }
    }
    public class FeedbackResponse
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string Type { get; set; }
        public string Content { get; set; }
        public string Reply { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateQuotationRequest
    {
        [Required]
        public int AgencyId { get; set; }

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public int VehicleInstanceId { get; set; }

        [Required]
        public string QuotationName { get; set; }

        [Required]
        public decimal QuotedPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? CreateBy { get; set; }
    }
    public class UpdateQuotationRequest
    {
        // Có thể cho phép cập nhật một số trường
        public int? AgencyId { get; set; }
        public string? QuotationName { get; set; }
        public int? CustomerId { get; set; }
        public decimal? QuotedPrice { get; set; }
        public int? VehicleInstanceId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public string? Status { get; set; }
    }

    public class QuotationResponse
    {
        // Các thuộc tính gốc của Quotation
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public string QuotationName { get; set; }
        public decimal QuotedPrice { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public int CreateBy { get; set; }

        // Các thuộc tính được làm giàu từ gRPC
        public AgencyReply Agency { get; set; }
        public VehicleInstanceReply Vehicle { get; set; }
        public UserReply User { get; set; }
    }

    public class CreateOrderRequest
    {
        public int CustomerId { get; set; }
        // TotalAmount đã bị xóa vì nó nên được tính toán bởi service
        public string? Status { get; set; }
        public int? CreateBy { get; set; }
        public List<CreateOrderDetailItem>? Details { get; set; }
    }
    public class CreateOrderDetailItem
    {
        public int QuotationId { get; set; }
        public decimal? UnitPrice { get; set; } // null => lấy QuotedPrice từ Quotation
    }
    public class UpdateOrderStatusRequest
    {
        public string Status { get; set; }
    }
    public class CreateOrderDetailRequest
    {
        public int OrderId { get; set; }
        public int QuotationId { get; set; }
        public decimal? UnitPrice { get; set; } // null => lấy QuotedPrice từ Quotation
    }
    public class UpdateOrderDetailPriceRequest
    {
        public decimal NewUnitPrice { get; set; }
    }
    public class OrderResponse
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int CreateBy { get; set; }
        public List<CreateOrderDetailItem>? Details { get; set; }
        public AgencyReply AgencyReply { get; set; }
    }

    //orderdetail response
    public class OrderDetailResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int QuotationId { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class CreateContractRequest
    {
        public int QuotationId { get; set; }
        public string ContractName { get; set; }
        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }
        public string Terms { get; set; }
    }

    public class UpdateContractRequest
    {
        public string? ContractName { get; set; }
        public DateTime? ContractDate { get; set; }
        public string? Terms { get; set; }
        public string? Status { get; set; }
        public IFormFile? ContractImagageUrl { get; set; }

    }

    public class ContractResponse
    {
        public int Id { get; set; }
        public int QuotationId { get; set; }
        public string ContractName { get; set; }
        public string ContractNumber { get; set; }
        public DateTime ContractDate { get; set; }
        public string Status { get; set; }
        public string Terms { get; set; }
        public string ContractImagageUrl { get; set; }
    }
    public class CreatePaymentRequest
    {
        public int? OrderId { get; set; }
        public int? AgencyOrderId { get; set; }
        public decimal Prepay { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
    }
    public class UpdatePaymentRequest
    {
        public decimal? Prepay { get; set; }
        public decimal? Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Status { get; set; }
    }
    public class PaymentResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int AgencyOrderId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Prepay { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string? TransactionCode { get; set; }
    }
    public class CreateTransactionRequest
    {
        public int? PaymentId { get; set; }
        public int? InstallPaymentId { get; set; }
        public string TransactionCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
    public class UpdateTransactionRequest
    {
        public string? TransactionCode { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal? Amount { get; set; }
        public string? Status { get; set; }
    }
    public class TransactionResponse
    {
        public int Id { get; set; }
        public int PaymentId { get; set; }
        public int InstallPaymentId { get; set; }
        public string TransactionCode { get; set; }
        public DateTime TransactionDate { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
    }
    public class DeliveryCreateRequest
    {
        public int OrderId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; } = null!; // bắt buộc nhập
        public string? Notes { get; set; }
        public IFormFile? ImgUrlBefore { get; set; }
        public IFormFile? ImgUrlAfter { get; set; }
    }
    public class DeliveryUpdateRequest
    {
        public DateTime? DeliveryDate { get; set; }
        public string? DeliveryStatus { get; set; } // optional update
        public string? Notes { get; set; }
        public IFormFile? ImgUrlBefore { get; set; }
        public IFormFile? ImgUrlAfter { get; set; }
    }
    public class DeliveryResponse
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryStatus { get; set; } = default!;
        public string? Notes { get; set; }
        public string? ImgUrlBefore { get; set; }
        public string? ImgUrlAfter { get; set; }
    }
    public class InstallmentPlanRequest
    {
        public int? ContractId { get; set; }
        public int? AgencyContractId { get; set; }
        public decimal PrincipalAmount { get; set; }          // Tổng gốc
        public decimal DepositAmount { get; set; }            // Tiền đặt cọc
        public decimal InterestRate { get; set; }             // Lãi suất %
        public string InterestMethod { get; set; } = "flat";  // flat / declining / none
        public string? RuleJson { get; set; }                 // Cấu hình kỳ (JSON)
        public string? Note { get; set; }                     // Ghi chú
    }
    public class InstallmentPlanUpdateRequest
    {
        public int? ContractId { get; set; }
        public int? AgencyContractId { get; set; }
        public decimal? PrincipalAmount { get; set; }
        public decimal? DepositAmount { get; set; }
        public decimal? InterestRate { get; set; }
        public string? InterestMethod { get; set; }
        public string? RuleJson { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; } // pending / active / closed
    }

    public class InstallmentPlanResponse
    {
        public int Id { get; set; }
        public int? ContractId { get; set; }
        public int? AgencyContractId { get; set; }
        public decimal PrincipalAmount { get; set; }
        public decimal DepositAmount { get; set; }
        public decimal InterestRate { get; set; }
        public string InterestMethod { get; set; }
        public string Status { get; set; }
        public string? RuleJson { get; set; }
        public string? Note { get; set; }
        public DateTime CreateAt { get; set; }
        public DateTime UpdateAt { get; set; }

        // Tổng tiền đã thanh toán
        public decimal TotalPaid { get; set; }

        // Danh sách kỳ trả
        public List<InstallmentItemResponse>? Items { get; set; }

        // Danh sách các thanh toán thực tế
        public List<InstallmentPaymentResponse>? Payments { get; set; }
    }
    public class InstallmentItemRequest
    {
        public int InstallmentPlanId { get; set; }
        public int InstallmentNo { get; set; }        // Kỳ thứ mấy
        public DateTime DueDate { get; set; }         // Ngày đến hạn

        public decimal Percentage { get; set; }       // % tổng tiền trong kỳ này
        public decimal AmountDue { get; set; }        // Tổng phải trả
        public decimal PrincipalComponent { get; set; }
        public decimal InterestComponent { get; set; }
        public decimal FeeComponent { get; set; }

        public string Status { get; set; } = "Pending"; // Pending / Paid / Overdue
        public string? Notes { get; set; }
    }
    public class InstallmentItemResponse
    {
        public int Id { get; set; }
        public int InstallmentPlanId { get; set; }
        public int InstallmentNo { get; set; }
        public DateTime DueDate { get; set; }

        public decimal Percentage { get; set; }               // % trong tổng số tiền
        public decimal AmountDue { get; set; }
        public decimal PrincipalComponent { get; set; }
        public decimal InterestComponent { get; set; }
        public decimal FeeComponent { get; set; }

        public decimal AmountPaid { get; set; }
        public decimal AmountRemaining { get; set; }

        public DateTime? PaidDate { get; set; }
        public string Status { get; set; } = "Pending";       // pending / partial / paid / overdue
        public string? Note { get; set; }
    }

    public class InstallmentPaymentRequest
    {
        public int InstallmentPlanId { get; set; }
        public int? InstallmentItemId { get; set; } // có thể null nếu trả nhiều kỳ cùng lúc
        public decimal AmountPaid { get; set; }
        public DateTime PaidDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }
    }

    public class UpdateInstallmentPaymentRequest
    {
        public decimal? AmountPaid { get; set; }
        public DateTime? PaidDate { get; set; }
        public string? PaymentMethod { get; set; }
        public string? Note { get; set; }
        public string? Status { get; set; }   // pending / completed / failed
    }

    public class InstallmentPaymentResponse
    {
        public int Id { get; set; }
        public int InstallmentPlanId { get; set; }
        public int? InstallmentItemId { get; set; }
        public decimal AmountPaid { get; set; }
        public DateTime PaidDate { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string? Note { get; set; }
    }

    public class InstallmentRule
    {
        public int Months { get; set; }
        public double Percentage { get; set; }
    }


}
