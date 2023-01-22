using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Shipping.Core.Domain;

namespace Nop.Plugin.Shipping.Core.Services
{
    public class ShipmentRequestService : IShipmentRequestService
    {
        #region Fields

        private readonly IRepository<ShipmentRequest> _shipmentRequestRepository;

        #endregion

        #region Ctor

        public ShipmentRequestService(IRepository<ShipmentRequest> shipmentRequestRepository)
        {
            _shipmentRequestRepository = shipmentRequestRepository;
        }

        #endregion

        public async Task<IList<ShipmentRequest>> GetShipmentRequestsAsync()
        {
            return await _shipmentRequestRepository.Table.Where(a => !a.IsDeleted).ToListAsync();
        }

        public async Task<IList<ShipmentRequest>> GetShipmentRequestsByPluginSystemNameAsync(string systemName)
        {
            return await _shipmentRequestRepository.Table.Where(a => !a.IsDeleted && a.ShippingProviderSystemName == systemName).ToListAsync();
        }

        public async Task<ShipmentRequest> GetShipmentRequestByIdAsync(int shipmentRequestId)
        {
            return await _shipmentRequestRepository.GetByIdAsync(shipmentRequestId);
        }

        public async Task<ShipmentRequest> GetShipmentRequestByOrderIdAsync(int orderId)
        {
            return await _shipmentRequestRepository.Table.FirstOrDefaultAsync(a => a.OrderId == orderId && !a.IsDeleted);
        }

        public async Task<ShipmentRequest> GetShipmentRequestByOrderGuidAsync(Guid orderGuid)
        {
            return await _shipmentRequestRepository.Table.FirstOrDefaultAsync(a => a.OrderGuid == orderGuid && !a.IsDeleted);
        }

        public async Task<ShipmentRequest> GetShipmentRequestByShipmentIdAsync(int shipmentId)
        {
            return await _shipmentRequestRepository.Table.FirstOrDefaultAsync(a => a.ShipmentId == shipmentId && !a.IsDeleted);
        }

        public async Task<ShipmentRequest> GetShipmentRequestByPackageReferenceAsync(string packageReference)
        {
            return await _shipmentRequestRepository.Table.FirstOrDefaultAsync(a => a.PackageReference == packageReference && !a.IsDeleted);
        }

        public async Task InsertShipmentRequestAsync(ShipmentRequest shipmentRequest)
        {
            await _shipmentRequestRepository.InsertAsync(shipmentRequest);
        }

        public async Task InsertShipmentRequestsAsync(IEnumerable<ShipmentRequest> shipmentRequests)
        {
            await _shipmentRequestRepository.InsertAsync(shipmentRequests.ToList());
        }

        public async Task UpdateShipmentRequestAsync(ShipmentRequest shipmentRequest)
        {
            await _shipmentRequestRepository.UpdateAsync(shipmentRequest);
        }

        public async Task UpdateShipmentRequestsAsync(IEnumerable<ShipmentRequest> shipmentRequests)
        {
            await _shipmentRequestRepository.UpdateAsync(shipmentRequests.ToList());
        }

        public async Task DeleteShipmentRequestAsync(ShipmentRequest shipmentRequest)
        {
            shipmentRequest.IsDeleted = true;
            await UpdateShipmentRequestAsync(shipmentRequest);
        }

        public async Task DeleteShipmentRequestsAsync(IEnumerable<ShipmentRequest> shipmentRequests)
        {
            foreach(var shipmentRequest in shipmentRequests)
            {
                shipmentRequest.IsDeleted = true;
            }
            await UpdateShipmentRequestsAsync(shipmentRequests);
        }
    }
}
