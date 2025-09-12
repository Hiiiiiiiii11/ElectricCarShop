using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserRepository.Model;

namespace UserRepository.Repositories
{
    public interface IAuthenticationRepository: IGenericRepository<Users>
    {
        Task<Users> GetUserByEmailAsync(string email);
    }
}
