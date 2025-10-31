using Grpc.Core;
using GrpcService;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
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
        public async Task<AgencyContractReply> GetContractByIdAsync(int contractId)
        {
            return await _client.GetAgencyContractByIdAsync(new GetAgencyContractByIdRequest { Id = contractId });
        }
        public AsyncServerStreamingCall<AgencyReply> GetAllAgencies(GetAllAgencyRequest request)
        {
            return _client.GetAllAgencies(request);
        }

        public AsyncServerStreamingCall<AgencyTargetReply> GetAllAgencyTargets(GetAllAgencyTargetRequest request)
        {
            return _client.GetAllAgencyTargets(request);
        }

        public AsyncServerStreamingCall<TestDriveReply> GetAllTestDrives(GetAllTestDriveRequest request)
        {
            return _client.GetAllTestDrives(request);
        }

        public AsyncServerStreamingCall<AgencyOrderReply> GetAllAgencyOrders(GetAllAgencyOrderRequest request)
        {
            return _client.GetAllAgencyOrders(request);
        }
        public Task<AgencyOrderReply> GetAgencyOrderByIdAsync(int agencyOrderId)
        {
            return _client.GetAgencyOrderByIdAsync(new GetAgencyOrderByIdRequest { Id = agencyOrderId }).ResponseAsync;
        }
    }
}
