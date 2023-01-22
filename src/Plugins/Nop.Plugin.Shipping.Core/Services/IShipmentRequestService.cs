using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Shipping.Core.Domain;

namespace Nop.Plugin.Shipping.Core.Services
{
    public interface IShipmentRequestService
    {
        Task<IList<ShipmentRequest>> GetShipmentRequestsAsync();
        Task<IList<ShipmentRequest>> GetShipmentRequestsByPluginSystemNameAsync(string systemName);
        Task<ShipmentRequest> GetShipmentRequestByIdAsync(int shipmentRequestId);
        Task<ShipmentRequest> GetShipmentRequestByOrderIdAsync(int orderId);
        Task<ShipmentRequest> GetShipmentRequestByOrderGuidAsync(Guid orderGuid);
        Task<ShipmentRequest> GetShipmentRequestByShipmentIdAsync(int shipmentId);
        Task<ShipmentRequest> GetShipmentRequestByPackageReferenceAsync(string packageReference);
        Task InsertShipmentRequestAsync(ShipmentRequest shipmentRequest);
        Task InsertShipmentRequestsAsync(IEnumerable<ShipmentRequest> shipmentRequests);
        Task UpdateShipmentRequestAsync(ShipmentRequest shipmentRequest);
        Task UpdateShipmentRequestsAsync(IEnumerable<ShipmentRequest> shipmentRequests);
        Task DeleteShipmentRequestAsync(ShipmentRequest shipmentRequest);
        Task DeleteShipmentRequestsAsync(IEnumerable<ShipmentRequest> shipmentRequests);
    }
}