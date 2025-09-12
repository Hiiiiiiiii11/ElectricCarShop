
using Microsoft.EntityFrameworkCore;
using Share.ShareRepo;
using UserRepository.Data;
using UserRepository.Model;

namespace UserRepository.Repositories
{
    public interface IUserRepository : IGenericRepository<Users> {
        Task<Users?> GetUserWithRolesAsync(int userId);
        Task<Users?> GetByUserNameAsync(string userName);
    }
}
