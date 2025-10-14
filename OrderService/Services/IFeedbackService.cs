using OrderRepository.Model.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface IFeedbackService
    {
        Task<IEnumerable<FeedbackResponse>> GetAllAsync();
        Task<FeedbackResponse?> GetByIdAsync(int id);
        Task<IEnumerable<FeedbackResponse>> GetByCustomerIdAsync(int customerId);
        Task<IEnumerable<FeedbackResponse>> GetByStatusAsync(string status);
        Task<FeedbackResponse> CreateAsync(FeedbackRequest request);
        Task<FeedbackResponse> UpdateAsync(int id, FeedbackRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
