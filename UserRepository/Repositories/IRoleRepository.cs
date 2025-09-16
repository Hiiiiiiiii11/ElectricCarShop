using Share.ShareRepo;
using UserRepository.Data;
using UserRepository.Model;

namespace UserRepository.Repositories
{

    public interface IRoleRepository : IGenericRepository<Roles> {
        Task<bool> GetByRoleNameAsync(string roleName);
    }


}
