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


        public async Task<FeedbackResponse> CreateAsync(FeedbackRequest request)
        {
            var feedback = new Feedback
            {
                CustomerId = request.CustomerId,
                Comment = request.Comment,
                Status = request.Status ?? "Pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _feedbackRepository.AddAsync(feedback);
            await _feedbackRepository.SaveChangesAsync();

            return MapToResponse(feedback);
        }

        public async Task<FeedbackResponse> UpdateAsync(int id, FeedbackRequest request)
        {
            var feedback = await _feedbackRepository.GetByIdAsync(id);
            if (feedback == null)
                throw new KeyNotFoundException($"Feedback with ID {id} not found.");

            if (!string.IsNullOrWhiteSpace(request.Comment))
                feedback.Comment = request.Comment;

            if (!string.IsNullOrWhiteSpace(request.Status))
                feedback.Status = request.Status;

            feedback.UpdatedAt = DateTime.UtcNow;

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
                Comment = f.Comment,
                Status = f.Status,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            };
        }
    }
}
