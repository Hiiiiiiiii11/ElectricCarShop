using OrderRepository.Model;
using OrderRepository.Model.Request;
using OrderRepository.Repositories;
using OrderService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public class InstallmentItemsService : IInstallmentItemsService
    {
        private readonly IInstallmentItemsRepository _itemsRepository;
        private readonly IInstallmentPlansRepository _plansRepository;

        public InstallmentItemsService(IInstallmentItemsRepository itemsRepository, IInstallmentPlansRepository plansRepository)
        {
            _itemsRepository = itemsRepository;
            _plansRepository = plansRepository;
        }

        // 🟢 CREATE ITEM
        public async Task<InstallmentItemResponse> CreateAsync(InstallmentItemRequest request)
        {
            var installmentPlan = await _plansRepository.GetByIdAsync(request.InstallmentPlanId);
            if (installmentPlan == null)
            {
                throw new KeyNotFoundException($"Installment plan with ID {request.InstallmentPlanId} not found.");
            }
            var item = new InstallmentItems
            {
                InstallmentPlanId = request.InstallmentPlanId,
                InstallmentNo = request.InstallmentNo,
                DueDate = request.DueDate,
                AmountDue = request.AmountDue,
                PrincipalComponent = request.PrincipalComponent,
                InterestComponent = request.InterestComponent,
                FeeComponent = request.FeeComponent,
                Notes = request.Notes,
                Status = "Pending"
            };

            await _itemsRepository.AddAsync(item);
            await _itemsRepository.SaveChangesAsync();
            return MapToResponse(item);
        }

        // 🟡 GET BY PLAN
        public async Task<IEnumerable<InstallmentItemResponse>> GetByPlanIdAsync(int planId)
        {
            var items = await _itemsRepository.GetByPlanIdAsync(planId);
            return items.Select(MapToResponse);
        }

        private static InstallmentItemResponse MapToResponse(InstallmentItems i) => new InstallmentItemResponse
        {
            Id = i.Id,
            InstallmentPlanId = i.InstallmentPlanId,
            InstallmentNo = i.InstallmentNo,
            DueDate = i.DueDate,
            AmountDue = i.AmountDue,
            PrincipalComponent = i.PrincipalComponent,
            InterestComponent = i.InterestComponent,
            FeeComponent = i.FeeComponent,
            Status = i.Status,
            Note = i.Notes
        };
    }
}
