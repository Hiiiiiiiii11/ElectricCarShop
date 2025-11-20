using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public class AgencyTargetService : IAgencyTargetService
    {
        private readonly IAgencyTargetRepository _AgencyTargetRepository;
        public AgencyTargetService(IAgencyTargetRepository AgencyTargetRepository)
        {
            _AgencyTargetRepository = AgencyTargetRepository;
        }

        public async Task<AgencyTargetReportResponse> CreateTargetAsync(int AgencyId, CreateAgencyTargetRequest request)
        {
            // 1. KIỂM TRA TRÙNG LẶP (MỚI)
            // Tìm xem đã có target nào cho Agency, Vehicle, Năm, Tháng này chưa
            var existingTarget = await _AgencyTargetRepository.FindAsync(t =>
                t.AgencyId == AgencyId &&
                t.VehicleId == request.VehicleId &&
                t.TargetYear == request.TargetYear &&
                t.TargetMonth == request.TargetMonth
            );

            if (existingTarget.Any())
            {
                throw new InvalidOperationException(
                    $"Đã tồn tại chỉ tiêu cho Agency {AgencyId}, Xe {request.VehicleId} vào tháng {request.TargetMonth}/{request.TargetYear}."
                );
            }

            // 2. TẠO MỚI (Nếu không trùng)
            var target = new AgencyTargets
            {
                AgencyId = AgencyId,
                VehicleId = request.VehicleId,
                TargetYear = request.TargetYear,
                TargetMonth = request.TargetMonth,
                TargetUnits = request.TargetUnits,
                AchievedUnits = 0, // Mặc định ban đầu là 0
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            await _AgencyTargetRepository.AddAsync(target);
            await _AgencyTargetRepository.SaveChangesAsync();

            return MapToResponse(target);
        }

        public async Task<IEnumerable<AgencyTargetReportResponse>> GetAllTargetsAsync(GetTargetReportRequest request)
        {
            var targets = await _AgencyTargetRepository.GetTargetsReportAsync(request.TargetYear, request.TargetMonth);
            return targets.Select(MapToResponse);
        }

        public async Task<IEnumerable<AgencyTargetReportResponse>> GetCurrentTargetByAgencyIdAsync(int AgencyId)
        {
            var now = DateTime.UtcNow;
            var targets = await _AgencyTargetRepository.GetByAgencyAndPeriodAsync(AgencyId, now.Year, now.Month);
            if (targets == null)
                throw new KeyNotFoundException($"No target found for Agency {AgencyId} in {now.Month}/{now.Year}");

            return targets.Select(MapToResponse);
        }

        public async Task<IEnumerable<AgencyTargetReportResponse>> GetAgencyTargetAsync(
            int AgencyId,
            GetTargetReportRequest request)
        {
            var targets = await _AgencyTargetRepository.GetTargetsByAgencyAsync(
                AgencyId,
                request.TargetYear,
                request.TargetMonth
            );

            if (targets == null || !targets.Any())
                throw new KeyNotFoundException(
                    $"No targets found for Agency {AgencyId} " +
                    $"{(request.TargetYear.HasValue ? $"in year {request.TargetYear}" : "")} " +
                    $"{(request.TargetMonth.HasValue ? $"month {request.TargetMonth}" : "")}"
                );

            return targets.Select(MapToResponse);
        }

        public async Task<IEnumerable<AgencyTargetReportResponse>> GetTargetsReportAsync(GetTargetReportRequest request)
        {
            var targets = await _AgencyTargetRepository.GetTargetsReportAsync(request.TargetYear, request.TargetMonth);
            return targets.Select(MapToResponse);

        }

        public async Task<AgencyTargetReportResponse> UpdateTargetAsync(
    int agencyId,
    int targetId,
    UpdateAgencyTargetRequest request)
        {
            var target = await _AgencyTargetRepository.GetByIdAsync(targetId);

            if (target == null || target.AgencyId != agencyId)
                throw new KeyNotFoundException(
                    $"No target found for Agency {agencyId} with targetId {targetId}"
                );

            // ✔ Update VehicleId
            if (request.VehicleId.HasValue)
                target.VehicleId = request.VehicleId.Value;

            // ✔ Update Year
            if (request.TargetYear.HasValue)
                target.TargetYear = request.TargetYear.Value;

            // ✔ Update Month
            if (request.TargetMonth.HasValue)
                target.TargetMonth = request.TargetMonth.Value;

            // ✔ Update TargetUnits
            if (request.TargetUnits.HasValue)
                target.TargetUnits = request.TargetUnits.Value;

            //target.UpdatedAt = DateTime.UtcNow;
            target.UpdatedAt = DateTime.UtcNow.AddMonths(-1);

            _AgencyTargetRepository.Update(target);
            await _AgencyTargetRepository.SaveChangesAsync();

            return MapToResponse(target);
        }


        public async Task RemoveAgencyTarget(int AgencyId, int targetId)
        {
            var target = await _AgencyTargetRepository
                .GetByIdAsync(targetId);

            if (target == null || target.AgencyId != AgencyId)
            {
                throw new KeyNotFoundException(
                    $"No target found for Agency {AgencyId} with targetId {targetId}"
                );
            }

            _AgencyTargetRepository.Remove(target);
            await _AgencyTargetRepository.SaveChangesAsync();
        }

        public AgencyTargetReportResponse MapToResponse(AgencyTargets target)
        {
            return new AgencyTargetReportResponse
            {
                Id = target.Id,
                AgencyId = target.AgencyId,
                VehicleId = target.VehicleId,
                TargetYear = target.TargetYear,
                TargetMonth = target.TargetMonth,
                TargetUnits = target.TargetUnits,
                AchievedUnits = target.AchievedUnits,
                CreateAt = target.CreatedAt,
                UpdateAt = target.UpdatedAt,
                Agency = target.Agency == null ? null : new AgencyResponse
                {
                    Id = target.Agency.Id,
                    AgencyName = target.Agency.AgencyName,
                    Email = target.Agency.Email,
                    Phone = target.Agency.Phone,
                    Address = target.Agency.Address,
                    Status = target.Agency.Status,
                    Created_At =target.Agency.Created_At,
                    Updated_At = target.Agency.Updated_At
                }
            };
        }

        
    }
}
