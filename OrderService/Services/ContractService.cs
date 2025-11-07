using Microsoft.EntityFrameworkCore;
using OrderRepository.Model;
using OrderRepository.Model.OrderDTO;
using OrderRepository.Repositories;
using OrderService.Services;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderAPIService.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;
        private readonly IAgencyGrpcServiceClient _agencyGrpcServiceClient;
        private readonly IUploadPhotoService _uploadPhotoService;

        public ContractService(IContractRepository contractRepository , IAgencyGrpcServiceClient agencyGrpcServiceClient, IUploadPhotoService uploadPhotoService)
        {
            _contractRepository = contractRepository;
            _agencyGrpcServiceClient = agencyGrpcServiceClient;
            _uploadPhotoService = uploadPhotoService;
        }

        public async Task<ContractResponse> CreateContractAsync(CreateContractRequest request)
        {
            var newContract = new Contracts
            {
                QuotationId = request.QuotationId,
                ContractName = request.ContractName,
                ContractNumber = request.ContractNumber,
                ContractDate = request.ContractDate,
                Status = "Pending",
                Terms = request.Terms
            };

            await _contractRepository.AddAsync(newContract);
            await _contractRepository.SaveChangesAsync();

            return MapToResponse(newContract);
        }

        public async Task<ContractResponse?> GetContractByIdAsync(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {id} not found.");
            return MapToResponse(contract);
        }

        public async Task<IEnumerable<ContractResponse>> GetContractsByQuotationIdAsync(int quotationId)
        {
            var contracts = await _contractRepository.GetByQuotationIdAsync(quotationId);
            return contracts.Select(MapToResponse);
        }

        public async Task<ContractResponse?> GetByContractNumberAsync(string contractNumber)
        {
            var contract = await _contractRepository.GetByContractNumberAsync(contractNumber);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with Number {contractNumber} not found.");
            return MapToResponse(contract);
        }

        public async Task<ContractResponse> UpdateContractAsync(int id, UpdateContractRequest request)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {id} not found.");

            // 📝 Cập nhật các trường thông tin cơ bản
            if (!string.IsNullOrWhiteSpace(request.ContractName))
                contract.ContractName = request.ContractName;

            if (request.ContractDate.HasValue)
                contract.ContractDate = request.ContractDate.Value;

            if (!string.IsNullOrWhiteSpace(request.Terms))
                contract.Terms = request.Terms;

            if (!string.IsNullOrWhiteSpace(request.Status))
                contract.Status = request.Status;

            // 🖼️ Upload ảnh mới (nếu có)
            if (request.ContractImagageUrl != null && request.ContractImagageUrl.Length > 0)
            {
                // Upload ảnh lên dịch vụ (ví dụ: Cloudinary, Firebase,...)
                var uploadResult = _uploadPhotoService.UploadPhoto(request.ContractImagageUrl);

                if (!string.IsNullOrEmpty(uploadResult))
                {
                    contract.ContractImageUrl = uploadResult;
                }
            }

            _contractRepository.Update(contract);
            await _contractRepository.SaveChangesAsync();

            return MapToResponse(contract);
        }



        public async Task<bool> DeleteContractAsync(int id)
        {
            var contract = await _contractRepository.GetByIdAsync(id);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {id} not found.");

            _contractRepository.Remove(contract);

            try
            {
                await _contractRepository.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Không thể xóa hợp đồng vì đang được tham chiếu ở bảng khác.", ex);
            }
        }

        private static ContractResponse MapToResponse(Contracts c) => new ContractResponse
        {
            Id = c.Id,
            QuotationId = c.QuotationId,
            ContractName = c.ContractName,
            ContractNumber = c.ContractNumber,
            ContractDate = c.ContractDate,
            ContractImagageUrl = c.ContractImageUrl,
            Status = c.Status,
            Terms = c.Terms
        };

        public async Task<IEnumerable<ContractResponse>> GetAllContractByAgencyId(int agencyId)
        {
            var agency = await _agencyGrpcServiceClient.GetAgencyByIdAsync(agencyId);
            if (agency == null)
                throw new KeyNotFoundException($"Agency with ID {agencyId} not found.");
            var contract = await _contractRepository.GetContractsByAgencyIdAsync(agencyId);
            return contract.Select(MapToResponse);
        }
    }
}
