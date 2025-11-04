using OrderRepository.Model;
using OrderRepository.Model.Request;
using OrderRepository.Repositories;
using OrderService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Service
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepository;

        public FeedbackService(IFeedbackRepository feedbackRepository)
        {
            _feedbackRepository = feedbackRepository;
        }

        public async Task<IEnumerable<FeedbackResponse>> GetAllAsync()
        {
            var feedbacks = await _feedbackRepository.GetAllAsync();
            return feedbacks.Select(MapToResponse);
        }

        public async Task<FeedbackResponse?> GetByIdAsync(int id)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(id);
            if (feedback == null)
                throw new KeyNotFoundException($"Feedback with ID {id} not found.");
            return MapToResponse(feedback);
        }

        public async Task<IEnumerable<FeedbackResponse>> GetByCustomerIdAsync(int customerId)
        {
            var feedbacks = await _feedbackRepository.GetByCustomerIdAsync(customerId);
            return feedbacks.Select(MapToResponse);
        }

        public async Task<IEnumerable<FeedbackResponse>> GetByStatusAsync(string status)
        {
            var feedbacks = await _feedbackRepository.GetByStatusAsync(status);
            return feedbacks.Select(MapToResponse);
        }
        public async Task<IEnumerable<FeedbackResponse>> GetByAgencyIdAsync(int agencyId)
        {
            var feedbacks = await _feedbackRepository.GetFeedbackByAgencyId(agencyId);
            return feedbacks.Select(MapToResponse);
        }


        public async Task<FeedbackResponse> CreateAsync(FeedbackRequest request)
        {
            var feedback = new Feedback
            {
                CustomerId = request.CustomerId,
                Content = request.Content,
                Type = request.Type,
                Status = request.Status ?? "Pending",
                Reply = string.Empty,
                AgencyId = request.AgencyId ?? 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _feedbackRepository.AddAsync(feedback);
            await _feedbackRepository.SaveChangesAsync();

            return MapToResponse(feedback);
        }

        public async Task<FeedbackResponse> UpdateAsync(int id, FeedbackUpdateRequest request)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(id);
            if (feedback == null)
                throw new KeyNotFoundException($"Feedback with ID {id} not found.");

            if (!string.IsNullOrWhiteSpace(request.Content))
                feedback.Content = request.Content;

            if (!string.IsNullOrWhiteSpace(request.Status))
                feedback.Status = request.Status;
           if(!string.IsNullOrEmpty(request.Type))
                feedback.Type = request.Type;
           if(!string.IsNullOrEmpty(request.Reply))
                feedback.Reply = request.Reply;
            feedback.UpdatedAt = DateTime.UtcNow;
            if(request.AgencyId.HasValue)
                feedback.AgencyId = request.AgencyId.Value;

            _feedbackRepository.Update(feedback);
            await _feedbackRepository.SaveChangesAsync();

            return MapToResponse(feedback);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(id);
            if (feedback == null)
                throw new KeyNotFoundException($"Feedback with ID {id} not found.");

            _feedbackRepository.Remove(feedback);
            await _feedbackRepository.SaveChangesAsync();

            return true;
        }

        private FeedbackResponse MapToResponse(Feedback f)
        {
            return new FeedbackResponse
            {
                Id = f.Id,
                CustomerId = f.CustomerId,
                Type = f.Type,
                Content = f.Content,
                Reply= f.Reply, 
                Status = f.Status,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            };
        }


    }
}
