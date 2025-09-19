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
    public class DealerTargetService : IDealerTargetService
    {
        private readonly IDealerTargetRepository _dealerTargetRepository;
        public DealerTargetService(IDealerTargetRepository dealerTargetRepository)
        {
            _dealerTargetRepository = dealerTargetRepository;
        }

        public async Task<DealerTargetReportResponse> CreateTargetAsync(int dealerId, CreateDealerTargetRequest request)
        {
            var existing = await _dealerTargetRepository.GetByDealerAndPeriodAsync(dealerId, request.TargetYear, request.TargetMonth);
            if (existing != null)
                throw new ArgumentException($"Target for {request.TargetMonth}/{request.TargetYear} already exists.");

            var target = new DealerTargets
            {
                DealerId = dealerId,
                TargetYear = request.TargetYear,
                TargetMonth = request.TargetMonth,
                TargetSales = request.TargetSales,
                AchievedSales = 0 // Mặc định ban đầu là 0
            };
            await _dealerTargetRepository.AddAsync(target);
            await _dealerTargetRepository.SaveChangesAsync();
            var savedTarget = await _dealerTargetRepository.GetByDealerAndPeriodAsync(dealerId, request.TargetYear, request.TargetMonth);
            return MapToResponse(savedTarget);
        }

        public async Task<IEnumerable<DealerTargetReportResponse>> GetAllTargetsAsync(GetTargetReportRequest request)
        {
            var targets = await _dealerTargetRepository.GetTargetsReportAsync(request.TargetYear, request.TargetMonth);
            return targets.Select(MapToResponse);
        }

        public async Task<DealerTargetReportResponse> GetCurrentTargetByDealerIdAsync(int dealerId)
        {
            var now = DateTime.UtcNow;
            var target = await _dealerTargetRepository.GetByDealerAndPeriodAsync(dealerId, now.Year, now.Month);
            if (target == null)
                throw new KeyNotFoundException($"No target found for dealer {dealerId} in {now.Month}/{now.Year}");

            return MapToResponse(target);
        }

        public async Task<IEnumerable<DealerTargetReportResponse>> GetDealerTargetAsync(
            int dealerId,
            GetTargetReportRequest request)
        {
            var targets = await _dealerTargetRepository.GetTargetsByDealerAsync(
                dealerId,
                request.TargetYear,
                request.TargetMonth
            );

            if (targets == null || !targets.Any())
                throw new KeyNotFoundException(
                    $"No targets found for dealer {dealerId} " +
                    $"{(request.TargetYear.HasValue ? $"in year {request.TargetYear}" : "")} " +
                    $"{(request.TargetMonth.HasValue ? $"month {request.TargetMonth}" : "")}"
                );

            return targets.Select(MapToResponse);
        }

        public async Task<IEnumerable<DealerTargetReportResponse>> GetTargetsReportAsync(GetTargetReportRequest request)
        {
            var targets = await _dealerTargetRepository.GetTargetsReportAsync(request.TargetYear, request.TargetMonth);
            return targets.Select(MapToResponse);

        }

        public async Task<DealerTargetReportResponse> UpdateAchievedSalesAsync(int dealerId,int targetId, UpdateDealerTargetRequest request)
        {
            var target = await _dealerTargetRepository.GetByIdAsync(targetId);

            if (target == null || target.DealerId != dealerId)
                throw new KeyNotFoundException(
                    $"No target found for dealer {dealerId} with targetId {targetId}"
                );
            if (request.AchievedSales.HasValue)
            {
                // update qua repo
                await _dealerTargetRepository.UpdateAchievedSalesAsync(target.Id, request.AchievedSales.Value);
                // reload entity
                target = await _dealerTargetRepository.GetByDealerAndPeriodAsync(dealerId, request.TargetYear.Value, request.TargetMonth.Value);
            }

            return MapToResponse(target);
        }
        public async Task RemoveDealerTarget(int dealerId, int targetId)
        {
            var target = await _dealerTargetRepository
                .GetByIdAsync(targetId);

            if (target == null || target.DealerId != dealerId)
            {
                throw new KeyNotFoundException(
                    $"No target found for dealer {dealerId} with targetId {targetId}"
                );
            }

            _dealerTargetRepository.Remove(target);
            await _dealerTargetRepository.SaveChangesAsync();
        }

        public DealerTargetReportResponse MapToResponse(DealerTargets target)
        {
            return new DealerTargetReportResponse
            {
                Id = target.Id,
                DealerId = target.DealerId,
                TargetYear = target.TargetYear,
                TargetMonth = target.TargetMonth,
                TargetSales = target.TargetSales,
                AchievedSales = target.AchievedSales,
                Dealer = target.Dealer == null ? null : new DealerResponse
                {
                    Id = target.Dealer.Id,
                    DealerName = target.Dealer.DealerName,
                    Email = target.Dealer.Email,
                    Phone = target.Dealer.Phone,
                    Address = target.Dealer.Address,
                    Status = target.Dealer.Status,
                    Created_At =target.Dealer.Created_At,
                    Updated_At = target.Dealer.Updated_At
                }
            };
        }

        
    }
}
