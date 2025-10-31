using Grpc.Core;
using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public interface IAgencyGrpcServiceClient
    {
        Task<AgencyReply> GetAgencyByIdAsync(int agencyId);
        Task<AgencyContractReply> GetContractByIdAsync(int contractId);
        Task<AgencyOrderReply> GetAgencyOrderByIdAsync(int agencyOrderId);
        AsyncServerStreamingCall<AgencyReply> GetAllAgencies(GetAllAgencyRequest request);
        AsyncServerStreamingCall<AgencyTargetReply> GetAllAgencyTargets(GetAllAgencyTargetRequest request);
        AsyncServerStreamingCall<TestDriveReply> GetAllTestDrives(GetAllTestDriveRequest request);
        AsyncServerStreamingCall<AgencyOrderReply> GetAllAgencyOrders(GetAllAgencyOrderRequest request);
    }
}
