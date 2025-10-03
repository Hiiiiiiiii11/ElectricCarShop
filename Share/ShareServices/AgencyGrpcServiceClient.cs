using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public class AgencyGrpcServiceClient : IAgencyGrpcServiceClient
    {
        private readonly AgencyGrpcService.AgencyGrpcServiceClient _client;

        public AgencyGrpcServiceClient(AgencyGrpcService.AgencyGrpcServiceClient client)
        {
            _client = client;
        }

        public async Task<AgencyReply> GetAgencyByIdAsync(int agencyId)
        {
            return await _client.GetAgencyByIdAsync(new GetAgencyByIdRequest
            {
                Id = agencyId
            });
        }
    }
}
