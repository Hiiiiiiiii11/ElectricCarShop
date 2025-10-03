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

namespace AgencyService.Services
{
    public class AgencyContractService : IAgencyContractService
    {
        private readonly IAgencyContractRepository _AgencyContractRepository;
        public AgencyContractService(IAgencyContractRepository AgencyContractRepository)
        {
            _AgencyContractRepository = AgencyContractRepository;
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
                ContractDate = DateTime.UtcNow,
                ContracrEndDate = DateTime.UtcNow.AddYears(1),
                Terms = request.Terms,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status,
            };
             _AgencyContractRepository.AddAsync(AgencyContract);
            await _AgencyContractRepository.SaveChangesAsync();

            return MapToResponse(AgencyContract);
        }

        public async Task<IEnumerable<AgencyContractResponse>> GetActiveByAgencyIdAsync(int AgencyId)
        {
            var contracts =  await _AgencyContractRepository.GetActiveByAgencyIdAsync(AgencyId);
            return contracts.Select(MapToResponse);
        }

        public async Task<IEnumerable<AgencyContractResponse>> GetByAgencyIdAsync(int AgencyId)
        {
            var contracts =  await _AgencyContractRepository.GetByAgencyIdAsync(AgencyId);
            return contracts.Select(MapToResponse);
        }

        public async Task<IEnumerable<AgencyContractResponse>> GetExpiredByAgencyIdAsync(int AgencyId)
        {
            var contract = await _AgencyContractRepository.GetExpiredByAgencyIdAsync(AgencyId);
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

        //mapping
        public AgencyContractResponse MapToResponse(AgencyContracts contract)
        {
            return new AgencyContractResponse
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                ContractDate = contract.ContractDate,
                Terms = contract.Terms,
                Status = contract.Status,
                Agency = contract.Agency == null ? null : new AgencyResponse
                {
                    Id = contract.Agency.Id,
                    AgencyName = contract.Agency.AgencyName,
                    Address = contract.Agency.Address,
                    Phone = contract.Agency.Phone,
                    Email = contract.Agency.Email,
                    Status = contract.Agency.Status,
                    //Created_At = contract.Agency.Created_At,
                    //Updated_At = contract.Agency.Updated_At,
                }
            };
        }
    }

}
