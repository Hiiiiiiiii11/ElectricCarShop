using Microsoft.AspNetCore.Http;
using OrderRepository.Model;
using OrderRepository.Repositories;
using Share.Setting;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail; // 👈 Thêm
using System.Text;    // 👈 Thêm
using System.Threading.Tasks;

namespace OrderService.Services
{
    public class ContractEmailService : IContractEmailService
    {
        private readonly IUploadPhotoService _uploadPhotoService;
        private readonly IContractRepository _contractRepository;
        private readonly EmailSetting _emailSetting;
        

        // ✅ Constructor đã sửa (xóa IContractEmailService)
        public ContractEmailService(
            IUploadPhotoService uploadPhotoService,
            IContractRepository contractRepository,
            EmailSetting emailSetting)
        {
            _uploadPhotoService = uploadPhotoService;
            _contractRepository = contractRepository;
            _emailSetting = emailSetting;
        }

        /// <summary>
        /// Hàm WORKFLOW: Tải ảnh, cập nhật DB, và gửi mail
        /// </summary>
        public async Task UploadAndUpdateContractEmailAsync(int contractId, string customerEmail, IFormFile file)
        {
            // --- BƯỚC 1: DÙNG SERVICE UPLOAD CÓ SẴN CỦA BẠN ---
            string imageUrl;
            try
            {
                imageUrl = _uploadPhotoService.UploadPhoto(file);
            }
            catch (System.Exception ex)
            {
                throw new System.InvalidOperationException($"Lỗi khi tải ảnh: {ex.Message}");
            }

            if (string.IsNullOrEmpty(imageUrl))
            {
                throw new System.Exception("Không thể tải ảnh lên, URL trả về rỗng.");
            }

            // --- BƯỚC 2: LẤY VÀ CẬP NHẬT HỢP ĐỒNG ---
            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy hợp đồng với ID {contractId}");
            }

            contract.ContractImageUrl = imageUrl;
            _contractRepository.Update(contract);
            await _contractRepository.SaveChangesAsync();

            // --- BƯỚC 3: GỬI EMAIL (✅ Đã sửa) ---
            // Gọi hàm SendContractEmailAsync (ở bên dưới) của chính class này
            await this.SendContractEmailAsync(customerEmail, contract);
        }

        // -----------------------------------------------------------------
        // 🚀 HÀM GỬI MAIL BẠN YÊU CẦU
        // -----------------------------------------------------------------
        public async Task SendContractEmailAsync(string customerEmail, Contracts contract)
        {
            string subject = $"Thông tin hợp đồng của bạn: {contract.ContractNumber}";
            string body = BuildContractEmailBody(contract);

            // Gọi hàm gửi mail private
            await SendEmailInternalAsync(customerEmail, subject, body);
        }

        /// <summary>
        /// Hàm private xây dựng nội dung HTML của email
        /// </summary>
        private string BuildContractEmailBody(Contracts contract)
        {
            var bodyBuilder = new StringBuilder();

            bodyBuilder.AppendLine("<html><body>");
            bodyBuilder.AppendLine($"<h2>Xin chào,</h2>");
            bodyBuilder.AppendLine($"<p>Chúng tôi xin gửi thông tin chi tiết về hợp đồng của bạn:</p>");
            bodyBuilder.AppendLine("<hr />");

            bodyBuilder.AppendLine("<h3>Thông tin hợp đồng</h3>");
            bodyBuilder.AppendLine("<ul>");
            bodyBuilder.AppendLine($"<li><strong>Tên hợp đồng:</strong> {contract.ContractName}</li>");
            bodyBuilder.AppendLine($"<li><strong>Số hợp đồng:</strong> {contract.ContractNumber}</li>");
            bodyBuilder.AppendLine($"<li><strong>Ngày ký:</strong> {contract.ContractDate.ToString("dd/MM/yyyy")}</li>");
            bodyBuilder.AppendLine($"<li><strong>Trạng thái:</strong> {contract.Status}</li>");
            bodyBuilder.AppendLine("</ul>");

            if (!string.IsNullOrEmpty(contract.ContractImageUrl))
            {
                bodyBuilder.AppendLine("<h3>Hình ảnh hợp đồng</h3>");
                bodyBuilder.AppendLine($"<img src='{contract.ContractImageUrl}' alt='Hình ảnh hợp đồng' style='max-width: 100%; height: auto;' />");
            }

            bodyBuilder.AppendLine("<br /><p>Cảm ơn bạn đã tin tưởng dịch vụ của chúng tôi.</p>");
            bodyBuilder.AppendLine("</body></html>");

            return bodyBuilder.ToString();
        }

        /// <summary>
        /// Hàm private thực thi gửi mail (dùng SmtpClient)
        /// </summary>
        private Task SendEmailInternalAsync(string toEmail, string subject, string body)
        {
            // Logic này được lấy từ EmailVerificationService của bạn
            var client = new SmtpClient(_emailSetting.SmtpServer, _emailSetting.SmtpPort)
            {
                Credentials = new System.Net.NetworkCredential(_emailSetting.SenderEmail, _emailSetting.SenderPassword),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailSetting.SenderEmail, _emailSetting.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true // Quan trọng: để hiển thị HTML
            };
            mailMessage.To.Add(toEmail);

            return client.SendMailAsync(mailMessage);
        }
    }
}