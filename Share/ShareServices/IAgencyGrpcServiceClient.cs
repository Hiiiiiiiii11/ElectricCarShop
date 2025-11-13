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
        Task<RemoveFromAgencyInventoryReply> RemoveVehicleFromInventoryAsync(int agencyId, int vehicleInstanceId);
        Task<bool> IncreaseAchievedUnitsAsync(int agencyId, int vehicleId, int year, int month, int units);
        Task<bool> DecreaseAchievedUnitsAsync(int agencyId, int vehicleId, int year, int month, int units);
    }
}
