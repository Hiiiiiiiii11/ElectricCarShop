using DealerRepository.Model;
using DealerRepository.Model.DTO;
using DealerRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace DealerService.Services
{
    public class DealerContractService : IDealerContractService
    {
        private readonly IDealerContractRepository _dealerContractRepository;
        public DealerContractService(IDealerContractRepository dealerContractRepository)
        {
            _dealerContractRepository = dealerContractRepository;
        }
        public async Task<DealerContractResponse> CreateDealerContractAsync(CreateDealerContractRequest request)
        {
            var existingContract =  _dealerContractRepository.GetByContractNumberAsync(request.ContractNumber);
            if (existingContract != null)
                throw new ArgumentException($"Contract with number {request.ContractNumber} already exists.");
            var dealerContract = new DealerContracts
            {
                DealerId = request.DealerId,
                ContractNumber = request.ContractNumber,
                ContractDate = DateTime.UtcNow,
                Terms = request.Terms,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status,
            };
             _dealerContractRepository.AddAsync(dealerContract);
            await _dealerContractRepository.SaveChangesAsync();

            return MapToResponse(dealerContract);
        }

        public async Task<IEnumerable<DealerContractResponse>> GetActiveByDealerIdAsync(int dealerId)
        {
            var contracts =  await _dealerContractRepository.GetActiveByDealerIdAsync(dealerId);
            return contracts.Select(MapToResponse);
        }

        public async Task<IEnumerable<DealerContractResponse>> GetByDealerIdAsync(int dealerId)
        {
            var contracts =  await _dealerContractRepository.GetByDealerIdAsync(dealerId);
            return contracts.Select(MapToResponse);
        }

        public async Task<IEnumerable<DealerContractResponse>> GetExpiredByDealerIdAsync(int dealerId)
        {
            var contract = await _dealerContractRepository.GetExpiredByDealerIdAsync(dealerId);
            return contract.Select(MapToResponse);
        }

        public async Task RenewContractAsync(int contractId, DateTime newDate, string newTerms)
        {
            var contract = await _dealerContractRepository.GetByIdAsync(contractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with Id {contractId} not found.");

            contract.ContractDate = newDate;
            contract.Terms = newTerms;
            contract.Status = "Active";
            _dealerContractRepository.Update(contract);
            await _dealerContractRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<DealerContractResponse>> SearchAsync(string? contractNumber = null, string? status = null, DateTime? startDate = null, DateTime? endDate = null)
        {
            var contracts = await _dealerContractRepository.SearchAsync(contractNumber, status, startDate, endDate);
            return contracts.Select(MapToResponse);
        }

        public async Task TerminateContractAsync(int contractId, string reason)
        {
            var contract = await _dealerContractRepository.GetByIdAsync(contractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with Id {contractId} not found.");
            contract.Status = "Terminated";
            _dealerContractRepository.Update(contract);
            await _dealerContractRepository.SaveChangesAsync();
        }

        public async Task UpdateStatusAsync(int contractId, string status)
        {
            var contract = await _dealerContractRepository.GetByIdAsync(contractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with Id {contractId} not found.");
            contract.Status = status;
            _dealerContractRepository.Update(contract);
            await _dealerContractRepository.SaveChangesAsync();
        }

        //mapping
        public DealerContractResponse MapToResponse(DealerContracts contract)
        {
            return new DealerContractResponse
            {
                Id = contract.Id,
                DealerId = contract.DealerId,
                ContractNumber = contract.ContractNumber,
                ContractDate = contract.ContractDate,
                Terms = contract.Terms,
                Status = contract.Status,
            };
        }
    }

}
