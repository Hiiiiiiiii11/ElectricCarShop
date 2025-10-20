using OrderRepository.Model;
using OrderRepository.Model.Request;
using OrderRepository.Repositories;
using OrderService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderAPIService.Services
{
    public class ContractService : IContractService
    {
        private readonly IContractRepository _contractRepository;

        public ContractService(IContractRepository contractRepository)
        {
            _contractRepository = contractRepository;
        }

        public async Task<ContractResponse> CreateContractAsync(CreateContractRequest request)
        {
            var newContract = new Contracts
            {
                QuotationId = request.QuotationId,
                ContractName = request.ContractName,
                ContractNumber = request.ContractNumber,
                ContractDate = request.ContractDate,
                Terms = request.Terms
            };

            await _contractRepository.AddAsync(newContract);

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

            // Giữ giá trị cũ nếu không có dữ liệu mới
            if (!string.IsNullOrWhiteSpace(request.ContractName))
                contract.ContractName = request.ContractName;

            if (request.ContractDate.HasValue)
                contract.ContractDate = request.ContractDate.Value;

            if (!string.IsNullOrWhiteSpace(request.Terms))
                contract.Terms = request.Terms;

            // Cập nhật thời gian sửa đổi nếu có


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
            await _contractRepository.SaveChangesAsync();
            return true;
        }

        private static ContractResponse MapToResponse(Contracts c) => new ContractResponse
        {
            Id = c.Id,
            QuotationId = c.QuotationId,
            ContractName = c.ContractName,
            ContractNumber = c.ContractNumber,
            ContractDate = c.ContractDate,
            Terms = c.Terms
        };
    }
}
