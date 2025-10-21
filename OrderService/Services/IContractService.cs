using OrderRepository.Model.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface IContractService
    {
        Task<ContractResponse> CreateContractAsync(CreateContractRequest request);
        Task<ContractResponse?> GetContractByIdAsync(int id);
        Task<IEnumerable<ContractResponse>> GetContractsByQuotationIdAsync(int quotationId);
        Task<ContractResponse?> GetByContractNumberAsync(string contractNumber);
        Task<ContractResponse> UpdateContractAsync(int id, UpdateContractRequest request);
        Task<bool> DeleteContractAsync(int id);
        Task<IEnumerable<ContractResponse>> GetAllContractByAgencyId(int agencyId);
    }
}
