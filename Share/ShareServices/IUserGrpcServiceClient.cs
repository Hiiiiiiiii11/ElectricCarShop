using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public interface IUserGrpcServiceClient
    {
        Task<UserReply> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserReply>> GetUsersByAgencyIdAsync(int agencyId);
        Task<bool> AssignUserToAgencyAsync(int userId, int agencyId);
        Task<bool> RemoveUserFromAgencyAsync(int userId, int agencyId);
    }
}
