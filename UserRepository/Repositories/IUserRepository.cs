
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using UserRepository.Data;
using UserRepository.Model;

namespace UserRepository.Repositories
{
    public interface IUserRepository : IGenericRepository<Users> {
        Task<IEnumerable<Users>> GetAllUsersWithRolesAsync();
        Task<Users?> GetUserWithRolesAsync(int userId);
        Task<Users?> GetByUserNameAsync(string userName);
        Task<IEnumerable<Users>> GetUsersByAgencyIdWithRolesAsync(int agencyId);
        //Task<bool> AssignUserToAgencyAsync(int userId, int agencyId);
    }
}
