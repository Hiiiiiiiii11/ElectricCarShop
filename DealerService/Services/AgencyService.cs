using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public class AgencyService : IAgencyService
    {
        private readonly IAgencyRepository _AgencyRepository;
        private readonly IUserGrpcServiceClient _userGrpcServiceClient;
        public AgencyService(IAgencyRepository AgencyRepository, IUserGrpcServiceClient userGrpcServiceClient)
        {
            _AgencyRepository = AgencyRepository;
            _userGrpcServiceClient = userGrpcServiceClient;
        }
        public async Task<AgencyResponse> CreateAgencyAsync(CreateAgencyRequest request)
        {
            var existingAgency = await _AgencyRepository.GetAgencyByNameAsync(request.AgencyName);
            if (existingAgency != null)
            {
                throw new InvalidOperationException($"Agency with name [{request.AgencyName}] already exists!");
            }
            var Agency = new Agency
            {
                AgencyName = request.AgencyName,
                Address = request.Address,
                Phone = request.Phone,
                Email = request.Email,
                Status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status,
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            };
            await _AgencyRepository.AddAsync(Agency);
            await _AgencyRepository.SaveChangesAsync();
            return MapToResponse(Agency);
        }

        public async Task<bool> DeleteAgencyAsync(int id)
        {
            var Agency =  await _AgencyRepository.GetByIdAsync(id);
            if (Agency == null)
            {
                throw new KeyNotFoundException($"Agency with ID {id} not found.");
            }
            _AgencyRepository.Remove(Agency);
            await _AgencyRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<AgencyResponse>> GetAllAgencysAsync()
        {
            var agencies = await _AgencyRepository.GetAllAsync();

            var result = new List<AgencyResponse>();
            foreach (var agency in agencies)
            {
                var response = await MapToResponseWithUsersAsync(agency);
                result.Add(response);
            }

            return result;
        }

        public async Task<AgencyResponse> GetAgencyByIdAsync(int id)
        {
            var agency = await _AgencyRepository.GetByIdAsync(id);
            if (agency == null)
                throw new KeyNotFoundException($"Agency with ID {id} not found.");

            return await MapToResponseWithUsersAsync(agency);
        }

        public Task<IEnumerable<AgencyResponse>> SearchAgencysAsync(string searchTerm)
        {
            return _AgencyRepository.SearchAgencysAsync(searchTerm)
                .ContinueWith(task => task.Result.Select(MapToResponse));
        }

        public async Task<AgencyResponse> UpdateAgencyAsync(int id, UpdateAgencyRequest request)
        {
            var Agency = await _AgencyRepository.GetByIdAsync(id);
            if (Agency == null)
            {
                throw new KeyNotFoundException($"Agency with ID {id} not found.");
            }
            // Nếu có giá trị mới thì update, còn nếu null/empty thì giữ nguyên cũ
            if (!string.IsNullOrWhiteSpace(request.AgencyName))
                Agency.AgencyName = request.AgencyName;
            if (!string.IsNullOrWhiteSpace(request.Address))
                Agency.Address = request.Address;
            if (!string.IsNullOrWhiteSpace(request.Phone))
                Agency.Phone = request.Phone;
            if (!string.IsNullOrWhiteSpace(request.Email))
                Agency.Email = request.Email;
            if (!string.IsNullOrWhiteSpace(request.Status))
                Agency.Status = request.Status;
            Agency.Updated_At = DateTime.UtcNow;
            _AgencyRepository.Update(Agency);
            await _AgencyRepository.SaveChangesAsync();
            return MapToResponse(Agency);
        }
        public AgencyResponse MapToResponse(Agency agency)
        {
            return new AgencyResponse
            {
                Id = agency.Id,
                AgencyName = agency.AgencyName,
                Address = agency.Address,
                Phone = agency.Phone,
                Email = agency.Email,
                Status = agency.Status,
                Created_At = agency.Created_At,
                Updated_At = agency.Updated_At
            };
        }

        // map kèm user (async)
        private async Task<AgencyResponse> MapToResponseWithUsersAsync(Agency agency)
        {
            var response = MapToResponse(agency);

            // gọi gRPC lấy danh sách user theo agencyId
            var users = await _userGrpcServiceClient.GetUsersByAgencyIdAsync(agency.Id);
            response.Users = users;

            return response;
        }

        public async Task<bool> AssignUserAsync(AssignUserAgencyRequest request, int agencyId)
        {
            var agency = await _AgencyRepository.GetByIdAsync(agencyId);
            if (agency == null)
                throw new Exception("Agency not found");

            // Chỉ truyền int cho wrapper thôi, wrapper sẽ tạo request gRPC
            return await _userGrpcServiceClient.AssignUserToAgencyAsync(request.UserId, agencyId);
        }
    }
}
