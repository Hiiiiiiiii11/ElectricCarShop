using AllocationRepository.Model;
using OrderRepository.Model.OrderDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public interface IQuotationService
    {
        Task<IEnumerable<QuotationResponse>> GetAllQuotationsAsync();
        Task<QuotationResponse> GetQuotationByIdAsync(int id);
        Task<QuotationResponse> CreateQuotationAsync(CreateQuotationRequest request);
        Task<QuotationResponse> UpdateQuotationAsync(int id, UpdateQuotationRequest request);
        Task<bool> DeleteQuotationAsync(int id);
        Task<IEnumerable<QuotationResponse>> GetQuotationByUserCreateId(int userId);
        Task<IEnumerable<QuotationResponse>> GetQuotationByAgencyId(int agencyId);
    }
}
