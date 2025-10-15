using AllocationRepository.Model;
using AllocationRepository.Repositories;
using AllocationService.Services;
using OrderRepository.Model.Request;
using Share.ShareServices;
using System;
using System.Threading.Tasks;

namespace OrderAPIService.Services
{
    public class QuotationService : IQuotationService
    {
        private readonly IQuotationRepository _quotationRepository;
        private readonly IAgencyGrpcServiceClient _agencyClient;
        private readonly IVehicleGrpcServiceClient _vehicleClient;

        // Đã loại bỏ IMapper khỏi constructor
        public QuotationService(
            IQuotationRepository quotationRepository,
            IAgencyGrpcServiceClient agencyClient,
            IVehicleGrpcServiceClient vehicleClient)
        {
            _quotationRepository = quotationRepository;
            _agencyClient = agencyClient;
            _vehicleClient = vehicleClient;
        }

        public async Task<QuotationResponse> GetQuotationByIdAsync(int id)
        {
            var quotation = await _quotationRepository.GetByIdAsync(id);
            if (quotation == null)
            {
                throw new KeyNotFoundException($"Quotation with ID {id} not found.");
            }

            // Gọi gRPC song song để tối ưu hiệu năng
            var agencyTask = _agencyClient.GetAgencyByIdAsync(quotation.AgencyId);
            var vehicleTask = _vehicleClient.GetVehicleByIdAsync(quotation.VehicleId);

            // Chờ cả hai lời gọi hoàn tất
            await Task.WhenAll(agencyTask, vehicleTask);

            // Sử dụng phương thức mapping thủ công
            var response = MapToResponse(quotation);
            response.Agency = await agencyTask;
            response.Vehicle = await vehicleTask;

            return response;
        }

        public async Task<QuotationResponse> CreateQuotationAsync(CreateQuotationRequest request)
        {
            // Mapping thủ công từ request sang entity
            var quotation = new Quotations
            {
                AgencyId = request.AgencyId,
                CustomerId = request.CustomerId,
                VehicleId = request.VehicleId,
                QuotationName = request.QuotationName,
                QuotedPrice = request.QuotedPrice,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CreatedAt = DateTime.UtcNow,
                Status = "Pending" // Gán trạng thái mặc định khi tạo mới
            };

            await _quotationRepository.AddAsync(quotation);
            await _quotationRepository.SaveChangesAsync();

            // Gọi lại hàm Get để trả về dữ liệu đã được làm giàu thông tin
            return await GetQuotationByIdAsync(quotation.Id);
        }

        public async Task<QuotationResponse> UpdateQuotationAsync(int id, UpdateQuotationRequest request)
        {
            var quotation = await _quotationRepository.GetByIdAsync(id);
            if (quotation == null)
            {
                throw new KeyNotFoundException($"Quotation with ID {id} not found.");
            }

            // Cập nhật thủ công các thuộc tính
            if (!string.IsNullOrWhiteSpace(request.QuotationName))
                quotation.QuotationName = request.QuotationName;
            if (request.QuotedPrice.HasValue)
                quotation.QuotedPrice = request.QuotedPrice.Value;
            if (request.StartDate.HasValue)
                quotation.StartDate = request.StartDate.Value;
            if (request.EndDate.HasValue)
                quotation.EndDate = request.EndDate.Value;
            if (!string.IsNullOrWhiteSpace(request.Status))
                quotation.Status = request.Status;


            _quotationRepository.Update(quotation);
            await _quotationRepository.SaveChangesAsync();
            return await GetQuotationByIdAsync(id);
        }

        public async Task<bool> DeleteQuotationAsync(int id)
        {
            var quotation = await _quotationRepository.GetByIdAsync(id);
            if (quotation == null)
            {
                throw new KeyNotFoundException($"Quotation with ID {id} not found.");
            }

            _quotationRepository.Remove(quotation);
            await _quotationRepository.SaveChangesAsync();
            return true;
        }

        // Phương thức mapping thủ công tương tự CustomerService
        private QuotationResponse MapToResponse(Quotations q)
        {
            return new QuotationResponse
            {
                Id = q.Id,
                CustomerId = q.CustomerId,
                QuotationName = q.QuotationName,
                QuotedPrice = q.QuotedPrice,
                StartDate = q.StartDate,
                EndDate = q.EndDate,
                CreatedAt = q.CreatedAt,
                Status = q.Status,
                // Agency và Vehicle sẽ được gán sau khi gọi gRPC
                Agency = null,
                Vehicle = null
            };
        }
    }
}

