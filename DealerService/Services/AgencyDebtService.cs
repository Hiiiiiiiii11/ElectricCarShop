using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public class AgencyDebtService : IAgencyDebtService
    {
        private readonly IAgencyDebtRepository _AgencyDebtRepository;
        public AgencyDebtService(IAgencyDebtRepository AgencyDebtRepository)
        {
            _AgencyDebtRepository = AgencyDebtRepository;
        }
        public async Task<AgencyDebtResponse> AddDebtAsync(int AgencyId, AddAgencyDebtRequest request)
        {
            if(request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");
            await _AgencyDebtRepository.AddDebtAsync(AgencyId, request.Amount);
            var updateDebt = await _AgencyDebtRepository.GetByAgencyIdAsync(AgencyId);
            return MapToResponse(updateDebt);
        }

        public async Task<AgencyDebtResponse> ClearDebtAsync(int AgencyId)
        {
            var debt = await _AgencyDebtRepository.GetByAgencyIdAsync(AgencyId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt record for AgencyId {AgencyId} not found.");
            await _AgencyDebtRepository.ClearDebtAsync(AgencyId);
            var updateDebt = await _AgencyDebtRepository.GetByAgencyIdAsync(AgencyId);
            return MapToResponse(updateDebt);

        }

        public async Task<IEnumerable<AgencyDebtResponse>> GetAllDebtsAsync()
        {
            var debts = await _AgencyDebtRepository.GetAllAsync(d => d.Agency);
            return debts.Select(MapToResponse);
        }

        public async Task<IEnumerable<AgencyDebtResponse>> GetAgencysWithRemainingDebtAsync()
        {
            var debts =  await _AgencyDebtRepository.GetAgencysWithRemainingDebtAsync();
            return debts.Select(MapToResponse);
        }

        public async Task<AgencyDebtResponse> GetDebtByAgencyIdAsync(int AgencyId)
        {
            var debt = await _AgencyDebtRepository.GetByAgencyIdAsync(AgencyId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt record for AgencyId {AgencyId} not found.");
            return MapToResponse(debt);
        }

        public async Task<AgencyDebtResponse> MakePaymentAsync(int AgencyId, MakePaymentRequest request)
        {
            var debt = await _AgencyDebtRepository.GetByAgencyIdAsync(AgencyId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt record for AgencyId {AgencyId} not found.");
            if (request.Amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.");
            await _AgencyDebtRepository.UpdatePaymentAsync(AgencyId, request.Amount);
            var updatedDebt = await _AgencyDebtRepository.GetByAgencyIdAsync(AgencyId);
            return MapToResponse(updatedDebt);
        }

        public async Task<IEnumerable<AgencyDebtResponse>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var debts = await _AgencyDebtRepository.SearchDebtsAsync(fromDate, toDate);
            return debts.Select(MapToResponse);
        }
        public AgencyDebtResponse MapToResponse(AgencyDebts debt)
        {
            return new AgencyDebtResponse
            {
                Id = debt.Id,
                AgencyId = debt.AgencyId,
                DebtAmount = debt.DebtAmount,
                PaidAmount = debt.PaidAmount,
                RemainingAmount = debt.RemainingAmount,
                CreateAt = debt.CreateAt,
                UpdateAt = debt.UpdateAt,
                Agency = debt.Agency == null ? null : new AgencyResponse
                {
                    Id = debt.Agency.Id,
                    AgencyName = debt.Agency.AgencyName,
                    Address = debt.Agency.Address,
                    Phone = debt.Agency.Phone,
                    Email = debt.Agency.Email,
                    Status = debt.Agency.Status,
                    //Created_At = debt.Agency.Created_At,
                    //Updated_At = debt.Agency.Updated_At,
                }
            };
        }
    }
}
