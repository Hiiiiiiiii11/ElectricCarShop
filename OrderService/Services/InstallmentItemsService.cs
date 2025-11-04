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
        //public async Task<InstallmentItemResponse> UpdateAsync(int id, InstallmentItemUpdateRequest request)
        //{
        //    var item = await _itemsRepository.GetByIdAsync(id);
        //    if (item == null)
        //    {
        //        throw new KeyNotFoundException($"Installment item with ID {id} not found.");
        //    }

        //    // Cập nhật các trường nếu chúng được cung cấp trong request
        //    if (request.InstallmentNo.HasValue)
        //        item.InstallmentNo = request.InstallmentNo.Value;
        //    if (request.DueDate.HasValue)
        //        item.DueDate = request.DueDate.Value;
        //    if (request.AmountDue.HasValue)
        //        item.AmountDue = request.AmountDue.Value;
        //    if (request.PrincipalComponent.HasValue)
        //        item.PrincipalComponent = request.PrincipalComponent.Value;
        //    if (request.InterestComponent.HasValue)
        //        item.InterestComponent = request.InterestComponent.Value;
        //    if (request.FeeComponent.HasValue)
        //        item.FeeComponent = request.FeeComponent.Value;
        //    if (request.Notes != null) // Cho phép cập nhật ghi chú (kể cả thành rỗng)
        //        item.Notes = request.Notes;
        //    if (!string.IsNullOrEmpty(request.Status))
        //        item.Status = request.Status;

        //    _itemsRepository.Update(item);
        //    await _itemsRepository.SaveChangesAsync();

        //    return MapToResponse(item);
        //}

        // 🟡 GET BY PLAN
        public async Task<IEnumerable<InstallmentItemResponse>> GetByPlanIdAsync(int planId)
        {
            var items = await _itemsRepository.GetByPlanIdAsync(planId);
            return items.Select(MapToResponse);
        }
        public async Task DeleteAsync(int id)
        {
            var item = await _itemsRepository.GetByIdAsync(id);
            if (item == null)
            {
                throw new KeyNotFoundException($"Installment item with ID {id} not found.");
            }

            _itemsRepository.Remove(item);
            await _itemsRepository.SaveChangesAsync();
        }

        // 🟡 GET BY ID (Hàm mới)
        public async Task<InstallmentItemResponse> GetByIdAsync(int id)
        {
            var item = await _itemsRepository.GetByIdAsync(id);
            if (item == null)
            {
                throw new KeyNotFoundException($"Installment item with ID {id} not found.");
            }
            return MapToResponse(item);
        }
        public async Task<IEnumerable<InstallmentItemResponse>> GetAllAsync()
        {
            var items = await _itemsRepository.GetAllAsync();
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
