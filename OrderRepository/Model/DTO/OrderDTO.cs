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
        public DateTime? CreateAt { get; set; }
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
    }
    public class CreatePaymentRequest
    {
        public int? OrderId { get; set; }
        public int? AgencyOrderId { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Prepay { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
    }
    public class UpdatePaymentRequest
    {
        public DateTime? PaymentDate { get; set; }
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
    }
    public class CreateTransactionRequest
    {
        public int PaymentId { get; set; }
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
}
