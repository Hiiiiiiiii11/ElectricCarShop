using AllocationRepository.Model;
using AllocationRepository.Repositories;
using AllocationService.Services;
using Azure.Core;
using GrpcService;
using Microsoft.EntityFrameworkCore;
using OrderRepository.Model.OrderDTO;
using OrderRepository.Repositories;
using Share.ShareServices;
using System;
using System.Threading.Tasks;

namespace OrderAPIService.Services
{
    public class QuotationService : IQuotationService
    {
        private readonly IQuotationRepository _quotationRepository;
        private readonly IAgencyGrpcServiceClient _agencyClient;
        private readonly IVehicleInstanceGrpcServiceClient _vehicleClient;
        private readonly IUserGrpcServiceClient _userGrpcServiceClient;
        private readonly ICustomerRepository _customerRepository ;

        // Đã loại bỏ IMapper khỏi constructor
        public QuotationService(
            IQuotationRepository quotationRepository,
            IAgencyGrpcServiceClient agencyClient,
            IVehicleInstanceGrpcServiceClient vehicleClient,
            IUserGrpcServiceClient userGrpcServiceClient,
            ICustomerRepository customerRepository
            )
        {
            _quotationRepository = quotationRepository;
            _agencyClient = agencyClient;
            _vehicleClient = vehicleClient;
            _userGrpcServiceClient = userGrpcServiceClient;
            _customerRepository = customerRepository;
        }

        public async Task<QuotationResponse> GetQuotationByIdAsync(int id)
        {
            var quotation = await _quotationRepository.GetByIdAsync(id);
            if (quotation == null)
                throw new KeyNotFoundException($"Quotation with ID {id} not found.");

            // Gọi song song các dịch vụ gRPC
            var agencyTask = _agencyClient.GetAgencyByIdAsync(quotation.AgencyId);
            var vehicleTask = _vehicleClient.GetVehicleInstanceByIdAsync(quotation.VehicleInstanceId);
            var userTask = _userGrpcServiceClient.GetUserByIdAsync(quotation.CreateBy);

            await Task.WhenAll(agencyTask, vehicleTask, userTask);

            var response = MapToResponse(quotation);
            response.Agency = await agencyTask;
            response.Vehicle = await vehicleTask;
            response.User = await userTask;

            return response;
        }



        public async Task<QuotationResponse> CreateQuotationAsync(CreateQuotationRequest request)
        {
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId);
            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {request.CustomerId} not found.");
            var quotation = new Quotations
            {
                AgencyId = request.AgencyId,
                CustomerId = request.CustomerId,
                VehicleInstanceId = request.VehicleInstanceId,
                QuotationName = request.QuotationName,
                QuotedPrice = request.QuotedPrice,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                //CreatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreateBy = request.CreateBy ?? 0,
                Status = "Pending"
            };

            await _quotationRepository.AddAsync(quotation);
            await _quotationRepository.SaveChangesAsync();

            return await GetQuotationByIdAsync(quotation.Id);
        }

        public async Task<QuotationResponse> UpdateQuotationAsync(int id, UpdateQuotationRequest request)
        {
            var quotation = await _quotationRepository.GetByIdAsync(id);
            if (quotation == null)
                throw new KeyNotFoundException($"Quotation with ID {id} not found.");

            if (request.AgencyId.HasValue)
                quotation.AgencyId = request.AgencyId.Value;
            if (request.CustomerId.HasValue)
                quotation.CustomerId = request.CustomerId.Value;
            if (request.VehicleInstanceId.HasValue)
                quotation.VehicleInstanceId = request.VehicleInstanceId.Value;
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
                throw new KeyNotFoundException($"Quotation with ID {id} not found.");

            _quotationRepository.Remove(quotation);

            try
            {
                await _quotationRepository.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Không thể xóa báo giá vì đang được sử dụng ở bảng khác.", ex);
            }
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
                CreateBy = q.CreateBy,
                Agency = null,
                Vehicle = null,
                User = null
            };
        }

        public async Task<IEnumerable<QuotationResponse>> GetQuotationByUserCreateId(int userId)
        {
            // Gọi gRPC để lấy thông tin user
            var user = await _userGrpcServiceClient.GetUserByIdAsync(userId);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {userId} not found.");
            }

            // Lấy danh sách quotation từ DB
            var quotations = await _quotationRepository.GetQuotationByUserCreateId(userId);
            if (quotations == null || !quotations.Any())
            {
                return Enumerable.Empty<QuotationResponse>();
            }

            // Duyệt từng quotation và enrich dữ liệu bằng gRPC
            var responses = new List<QuotationResponse>();
            foreach (var q in quotations)
            {
                var agencyTask = _agencyClient.GetAgencyByIdAsync(q.AgencyId);
                var vehicleTask = _vehicleClient.GetVehicleInstanceByIdAsync(q.VehicleInstanceId);

                await Task.WhenAll(agencyTask, vehicleTask);

                var response = MapToResponse(q);
                response.Agency = await agencyTask;
                response.Vehicle = await vehicleTask;
                response.User = user;

                responses.Add(response);
            }

            return responses;
        }



        public async Task<IEnumerable<QuotationResponse>> GetAllQuotationsAsync()
        {
            var quotations = await _quotationRepository.GetAllAsync();
            if (quotations == null || !quotations.Any())
                return Enumerable.Empty<QuotationResponse>();

            var tasks = quotations.Select(async q =>
            {
                var agencyTask = _agencyClient.GetAgencyByIdAsync(q.AgencyId);
                var vehicleTask = _vehicleClient.GetVehicleInstanceByIdAsync(q.VehicleInstanceId);
                var userTask = _userGrpcServiceClient.GetUserByIdAsync(q.CreateBy);

                await Task.WhenAll(agencyTask, vehicleTask, userTask);

                var response = MapToResponse(q);
                response.Agency = await agencyTask;
                response.Vehicle = await vehicleTask;
                response.User = await userTask;
                return response;
            });

            return await Task.WhenAll(tasks);
        }

        public async Task<IEnumerable<QuotationResponse>> GetQuotationByAgencyId(int agencyId)
        {
            var agency = await _agencyClient.GetAgencyByIdAsync(agencyId);
            if (agency == null)
            {
                throw new KeyNotFoundException($"Agency with ID {agencyId} not found.");
            }
            var quotations = await _quotationRepository.GetQuotationByAgencyId(agencyId);
            if (quotations == null || !quotations.Any())
            {
                return Enumerable.Empty<QuotationResponse>();
            }
            var responses = new List<QuotationResponse>();
            foreach (var q in quotations)
            {
                var agencyTask = _agencyClient.GetAgencyByIdAsync(q.AgencyId);
                var vehicleTask = _vehicleClient.GetVehicleInstanceByIdAsync(q.VehicleInstanceId);


                await Task.WhenAll(agencyTask, vehicleTask);

                var response = MapToResponse(q);
                response.Agency = await agencyTask;
                response.Vehicle = await vehicleTask;


                responses.Add(response);
            }

            return responses;
        }
    }
}

