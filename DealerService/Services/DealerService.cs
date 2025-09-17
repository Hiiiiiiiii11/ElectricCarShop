using DealerRepository.Model;
using DealerRepository.Model.DTO;
using DealerRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerService.Services
{
    public class DealerService : IDealerService
    {
        private readonly IDealerRepository _dealerRepository;
        public DealerService(IDealerRepository dealerRepository)
        {
            _dealerRepository = dealerRepository;
        }
        public async Task<DealerResponse> CreateDealerAsync(CreateDealerRequest request)
        {
            var existingDealer = await _dealerRepository.GetDealerByNameAsync(request.DealerName);
            if (existingDealer != null)
            {
                throw new InvalidOperationException($"Dealer with name [{request.DealerName}] already exists!");
            }
            var dealer = new Dealers
            {
                DealerName = request.DealerName,
                Address = request.Address,
                Phone = request.Phone,
                Email = request.Email,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status,
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            };
            await _dealerRepository.AddAsync(dealer);
            await _dealerRepository.SaveChangesAsync();
            return MapToResponse(dealer);
        }

        public async Task<bool> DeleteDealerAsync(int id)
        {
            var dealer =  await _dealerRepository.GetByIdAsync(id);
            if (dealer == null)
            {
                throw new KeyNotFoundException($"Dealer with ID {id} not found.");
            }
            _dealerRepository.Remove(dealer);
            await _dealerRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<DealerResponse>> GetAllDealersAsync()
        {
            var dealers = await _dealerRepository.GetAllAsync();
            return dealers.Select(MapToResponse);
        }

        public async Task<DealerResponse> GetDealerByIdAsync(int id)
        {
            var dealer = await _dealerRepository.GetByIdAsync(id);
            if (dealer == null)
            {
                throw new KeyNotFoundException($"Dealer with ID {id} not found.");
            }
            return MapToResponse(dealer);
        }

        public Task<IEnumerable<DealerResponse>> SearchDealersAsync(string searchTerm)
        {
            return _dealerRepository.SearchDealersAsync(searchTerm)
                .ContinueWith(task => task.Result.Select(MapToResponse));
        }

        public async Task<DealerResponse> UpdateDealerAsync(int id, UpdateDealerRequest request)
        {
            var dealer = await _dealerRepository.GetByIdAsync(id);
            if (dealer == null)
            {
                throw new KeyNotFoundException($"Dealer with ID {id} not found.");
            }
            // Nếu có giá trị mới thì update, còn nếu null/empty thì giữ nguyên cũ
            if (!string.IsNullOrWhiteSpace(request.DealerName))
                dealer.DealerName = request.DealerName;
            if (!string.IsNullOrWhiteSpace(request.Address))
                dealer.Address = request.Address;
            if (!string.IsNullOrWhiteSpace(request.Phone))
                dealer.Phone = request.Phone;
            if (!string.IsNullOrWhiteSpace(request.Email))
                dealer.Email = request.Email;
            if (!string.IsNullOrWhiteSpace(request.Status))
                dealer.Status = request.Status;
            dealer.Updated_At = DateTime.UtcNow;
            _dealerRepository.Update(dealer);
            await _dealerRepository.SaveChangesAsync();
            return MapToResponse(dealer);
        }

        public DealerResponse MapToResponse(Dealers dealer)
        {
            return new DealerResponse
            {
                Id = dealer.Id,
                DealerName = dealer.DealerName,
                Address = dealer.Address,
                Phone = dealer.Phone,
                Email = dealer.Email,
                Status = dealer.Status,
                Created_At = dealer.Created_At,
                Updated_At = dealer.Updated_At
            };
        }
    }
}
