using DealerRepository.Model;
using DealerRepository.Model.DTO;
using DealerRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerService.Services
{
    public class DealerDebtService : IDealerDebtService
    {
        private readonly IDealerDebtRepository _dealerDebtRepository;
        public DealerDebtService(IDealerDebtRepository dealerDebtRepository)
        {
            _dealerDebtRepository = dealerDebtRepository;
        }
        public async Task<DealerDebtResponse> AddDebtAsync(int dealerId, AddDealerDebtRequest request)
        {
            if(request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");
            await _dealerDebtRepository.AddDebtAsync(dealerId, request.Amount);
            var updateDebt = await _dealerDebtRepository.GetByDealerIdAsync(dealerId);
            return MapToResponse(updateDebt);
        }

        public async Task<DealerDebtResponse> ClearDebtAsync(int dealerId)
        {
            var debt = await _dealerDebtRepository.GetByDealerIdAsync(dealerId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt record for DealerId {dealerId} not found.");
            await _dealerDebtRepository.ClearDebtAsync(dealerId);
            var updateDebt = await _dealerDebtRepository.GetByDealerIdAsync(dealerId);
            return MapToResponse(updateDebt);

        }

        public async Task<IEnumerable<DealerDebtResponse>> GetAllDebtsAsync()
        {
            var debts = await _dealerDebtRepository.GetAllAsync(d => d.Dealer);
            return debts.Select(MapToResponse);
        }

        public async Task<IEnumerable<DealerDebtResponse>> GetDealersWithRemainingDebtAsync()
        {
            var debts =  await _dealerDebtRepository.GetDealersWithRemainingDebtAsync();
            return debts.Select(MapToResponse);
        }

        public async Task<DealerDebtResponse> GetDebtByDealerIdAsync(int dealerId)
        {
            var debt = await _dealerDebtRepository.GetByDealerIdAsync(dealerId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt record for DealerId {dealerId} not found.");
            return MapToResponse(debt);
        }

        public async Task<DealerDebtResponse> MakePaymentAsync(int dealerId, MakePaymentRequest request)
        {
            var debt = await _dealerDebtRepository.GetByDealerIdAsync(dealerId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt record for DealerId {dealerId} not found.");
            if (request.Amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.");
            await _dealerDebtRepository.UpdatePaymentAsync(dealerId, request.Amount);
            var updatedDebt = await _dealerDebtRepository.GetByDealerIdAsync(dealerId);
            return MapToResponse(updatedDebt);
        }

        public async Task<IEnumerable<DealerDebtResponse>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var debts = await _dealerDebtRepository.SearchDebtsAsync(fromDate, toDate);
            return debts.Select(MapToResponse);
        }
        public DealerDebtResponse MapToResponse(DealerDebts debt)
        {
            return new DealerDebtResponse
            {
                Id = debt.Id,
                DealerId = debt.DealerId,
                TotalDebt = debt.TotalDebt,
                PaidAmount = debt.PaidAmount,
                RemainingDebt = debt.RemainingDebt,
                LastUpdate = debt.LastUpdate,
                Dealer = debt.Dealer == null ? null : new DealerResponse
                {
                    Id = debt.Dealer.Id,
                    DealerName = debt.Dealer.DealerName,
                    Address = debt.Dealer.Address,
                    Phone = debt.Dealer.Phone,
                    Email = debt.Dealer.Email,
                    Status = debt.Dealer.Status,
                    Created_At = debt.Dealer.Created_At,
                    Updated_At = debt.Dealer.Updated_At,
                }
            };
        }
    }
}
