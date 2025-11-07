using CloudinaryDotNet.Actions;
using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Share.ShareServices;
using Microsoft.EntityFrameworkCore;

namespace AgencyService.Services
{
    public class AgencyContractService : IAgencyContractService
    {
        private readonly IAgencyContractRepository _AgencyContractRepository;
        private readonly IUploadPhotoService _uploadPhotoService;
        public AgencyContractService(IAgencyContractRepository AgencyContractRepository, IUploadPhotoService uploadPhotoService)
        {
            _AgencyContractRepository = AgencyContractRepository;
            _uploadPhotoService = uploadPhotoService;
        }
        public async Task<AgencyContractResponse> CreateAgencyContractAsync(int AgencyId, CreateAgencyContractRequest request)
        {
            var existingContract = await _AgencyContractRepository.GetByContractNumberAsync(request.ContractNumber);
            if (existingContract != null)
                throw new ArgumentException($"Contract with number {request.ContractNumber} already exists.");
            var AgencyContract = new AgencyContracts
            {
                AgencyId = AgencyId,
                ContractNumber = request.ContractNumber,
                ContractDate = request.ContractDate,
                ContractEndDate = request.ContractEndDate ?? DateTime.UtcNow.AddYears(1),
                Terms = request.Terms,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status,
            };
             _AgencyContractRepository.AddAsync(AgencyContract);
            await _AgencyContractRepository.SaveChangesAsync();

            return MapToResponse(AgencyContract);
        }
        public async Task<AgencyContractResponse> UpdateAgencyContractAsync(int contractId, UpdateAgencyContractRequest request)
        {
            var agencyContract = await _AgencyContractRepository.GetByIdAsync(contractId);
            if (agencyContract == null)
                throw new KeyNotFoundException($"Không tìm thấy hợp đồng với ID {contractId}");

            // ✅ Cập nhật thông tin text
            if (!string.IsNullOrWhiteSpace(request.ContractNumber))
                agencyContract.ContractNumber = request.ContractNumber;

            if (request.ContractDate.HasValue)
                agencyContract.ContractDate = request.ContractDate.Value;

            if (request.ContractEndDate.HasValue)
                agencyContract.ContractEndDate = request.ContractEndDate.Value;

            if (!string.IsNullOrWhiteSpace(request.Terms))
                agencyContract.Terms = request.Terms;

            if (!string.IsNullOrWhiteSpace(request.Status))
                agencyContract.Status = request.Status;

            // 🖼️ Upload ảnh hợp đồng mới nếu có
            if (request.ContractImageUrl != null && request.ContractImageUrl.Length > 0)
            {
                var uploadResult = _uploadPhotoService.UploadPhoto(request.ContractImageUrl);
                if (!string.IsNullOrEmpty(uploadResult))
                    agencyContract.ContractImageUrl = uploadResult;
            }

            _AgencyContractRepository.Update(agencyContract);
            await _AgencyContractRepository.SaveChangesAsync();

            return MapToResponse(agencyContract);
        }

        public async Task<IEnumerable<AgencyContractResponse>> GetActiveByAgencyIdAsync(int AgencyId)
        {
            var contracts =  await _AgencyContractRepository.GetActiveByAgencyIdAsync(AgencyId);
            return contracts.Select(MapToResponse);
        }

        public async Task<IEnumerable<AgencyContractResponse>> GetByAgencyIdAsync(int AgencyId)
        {
            var contracts =  await _AgencyContractRepository.GetByAgencyIdAsync(AgencyId);
            if (contracts == null || !contracts.Any())
                throw new KeyNotFoundException($"No contracts found for Agency with Id {AgencyId}.");
            return contracts.Select(MapToResponse);
        }

        public async Task<IEnumerable<AgencyContractResponse>> GetExpiredByAgencyIdAsync(int AgencyId)
        {
            var contract = await _AgencyContractRepository.GetExpiredByAgencyIdAsync(AgencyId);
            if (contract == null || !contract.Any())
                throw new KeyNotFoundException($"No expired contracts found for Agency with Id {AgencyId}.");
            return contract.Select(MapToResponse);
        }

        public async Task<AgencyContractResponse> RenewContractAsync(int contractId, RenewContractRequest request)
        {
            var contract = await _AgencyContractRepository.GetByIdAsync(contractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with Id {contractId} not found.");

            contract.ContractDate = DateTime.UtcNow;
            contract.Terms = request.NewTerms;
            contract.Status = "Active";
            _AgencyContractRepository.Update(contract);
            await _AgencyContractRepository.SaveChangesAsync();
            return MapToResponse(contract);
        }

        public async Task<IEnumerable<AgencyContractResponse>> SearchAsync(string? contractNumber = null, string? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var contracts = await _AgencyContractRepository.SearchAsync(contractNumber, status, startDate, endDate);
            return contracts.Select(MapToResponse);
        }

        public async Task<AgencyContractResponse> TerminateContractAsync(int contractId, TerminateContractRequest request)
        {
            var contract = await _AgencyContractRepository.GetByIdAsync(contractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with Id {contractId} not found.");
            contract.Status = "Terminated";
            _AgencyContractRepository.Update(contract);
            await _AgencyContractRepository.SaveChangesAsync();
            return MapToResponse(contract);
        }

        public async Task<AgencyContractResponse> UpdateStatusAsync(int contractId, UpdateStatusAgencyContractRequest request)
        {
            var contract = await _AgencyContractRepository.GetByIdAsync(contractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with Id {contractId} not found.");
            contract.Status = request.Status;
            _AgencyContractRepository.Update(contract);
            await _AgencyContractRepository.SaveChangesAsync();
            return MapToResponse(contract);
        }
        public async Task DeleteAgencyAsync(int contractId)
        {
            var contract = await _AgencyContractRepository.GetByIdAsync(contractId);
            if (contract == null)
                throw new KeyNotFoundException($"Agency contract with ID {contractId} not found.");

            _AgencyContractRepository.Remove(contract);

            try
            {
                await _AgencyContractRepository.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Không thể xóa hợp đồng đại lý vì đang được tham chiếu ở bảng khác.", ex);
            }
        }


        //mapping
        public AgencyContractResponse MapToResponse(AgencyContracts contract)
        {
            return new AgencyContractResponse
            {
                Id = contract.Id,
                AgencyId = contract.AgencyId,
                ContractNumber = contract.ContractNumber,
                ContractDate = contract.ContractDate,
                ContractEndDate = contract.ContractEndDate,
                Terms = contract.Terms,
                Status =contract.Status,
                ContractImageUrl = contract.ContractImageUrl,
                Agency = contract.Agency == null ? null : new AgencyResponse
                {
                    Id = contract.Agency.Id,
                    AgencyName = contract.Agency.AgencyName,
                    Address = contract.Agency.Address,
                    Phone = contract.Agency.Phone,
                    Email = contract.Agency.Email,
                    Status = contract.Agency.Status,
                }
            };
        }


    }

}
