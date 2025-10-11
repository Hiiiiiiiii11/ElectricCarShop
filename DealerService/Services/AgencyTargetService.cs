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
            var existing = await _AgencyTargetRepository.GetByAgencyAndPeriodAsync(AgencyId, request.TargetYear, request.TargetMonth);
            if (existing != null)
                throw new ArgumentException($"Target for {request.TargetMonth}/{request.TargetYear} already exists.");

            var target = new AgencyTargets
            {
                AgencyId = AgencyId,
                TargetYear = request.TargetYear,
                TargetMonth = request.TargetMonth,
                TargetSales = request.TargetSales,
                AchievedSales = 0 // Mặc định ban đầu là 0
            };
            await _AgencyTargetRepository.AddAsync(target);
            await _AgencyTargetRepository.SaveChangesAsync();
            var savedTarget = await _AgencyTargetRepository.GetByAgencyAndPeriodAsync(AgencyId, request.TargetYear, request.TargetMonth);
            return MapToResponse(savedTarget);
        }

        public async Task<IEnumerable<AgencyTargetReportResponse>> GetAllTargetsAsync(GetTargetReportRequest request)
        {
            var targets = await _AgencyTargetRepository.GetTargetsReportAsync(request.TargetYear, request.TargetMonth);
            return targets.Select(MapToResponse);
        }

        public async Task<AgencyTargetReportResponse> GetCurrentTargetByAgencyIdAsync(int AgencyId)
        {
            var now = DateTime.UtcNow;
            var target = await _AgencyTargetRepository.GetByAgencyAndPeriodAsync(AgencyId, now.Year, now.Month);
            if (target == null)
                throw new KeyNotFoundException($"No target found for Agency {AgencyId} in {now.Month}/{now.Year}");

            return MapToResponse(target);
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

        public async Task<AgencyTargetReportResponse> UpdateAchievedSalesAsync(int AgencyId,int targetId, UpdateAgencyTargetRequest request)
        {
            var target = await _AgencyTargetRepository.GetByIdAsync(targetId);

            if (target == null || target.AgencyId != AgencyId)
                throw new KeyNotFoundException(
                    $"No target found for Agency {AgencyId} with targetId {targetId}"
                );
            if (request.AchievedSales.HasValue)
            {
                // update qua repo
                await _AgencyTargetRepository.UpdateAchievedSalesAsync(target.Id, request.AchievedSales.Value);
                // reload entity
                target = await _AgencyTargetRepository.GetByAgencyAndPeriodAsync(AgencyId, request.TargetYear.Value, request.TargetMonth.Value);
            }

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
                TargetYear = target.TargetYear,
                TargetMonth = target.TargetMonth,
                TargetSales = target.TargetSales,
                AchievedSales = target.AchievedSales,
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
