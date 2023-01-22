using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Plugin.Shipping.Core.Domain;
using Nop.Plugin.Shipping.Core.Models;
using Nop.Services.Plugins;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Shipping.Core.Services
{
    /// <summary>
    /// Provides an interface of shipping provider plugin
    /// </summary>
    public interface IShippingProviderPlugin : IPlugin
    {
        /// <summary>
        /// Gets shipping response from shipping provider for shipment
        /// </summary>
        /// <param name="shipment">Shipment instance</param>
        /// <returns>Shipping provider response</returns>
        Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderAsync(Shipment shipment);

        /// <summary>
        /// Gets shipping response from shipping provider for shipment Id
        /// </summary>
        /// <param name="shipmentId">Shipment Id</param>
        /// <returns>Shipping provider response</returns>
        Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderByShipmentIdAsync(int shipmentId);

        /// <summary>
        /// Gets shipping responses from shipping provider for shipments
        /// </summary>
        /// <param name="shipments">Shipment instances</param>
        /// <returns>Shipping provider responses</returns>
        Task<IList<UpdatedShipmentRequestModel>> GetShippingRequestsFromShippingProviderAsync(IEnumerable<Shipment> shipments);

        /// <summary>
        /// Gets shipping responses from shipping provider for shipment Ids
        /// </summary>
        /// <param name="shipmentIds">Shipment ids</param>
        /// <returns>Shipping provider responses</returns>
        Task<IList<UpdatedShipmentRequestModel>> GetShippingRequestsFromShippingProviderByShipmentIdsAsync(IEnumerable<int> shipmentIds);

        /// <summary>
        /// Gets shipping response from shipping provider for order Id
        /// </summary>
        /// <param name="orderId">Order Id</param>
        /// <returns>Shipping provider response</returns>
        Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderByOrderIdAsync(int orderId);

        /// <summary>
        /// Gets shipping responses from shipping provider for order Ids
        /// </summary>
        /// <param name="orderIds">Order Ids</param>
        /// <returns>Shipping provider response</returns>
        Task<IList<UpdatedShipmentRequestModel>> GetShippingRequestsFromShippingProviderByOrderIdsAsync(IEnumerable<int> orderIds);

        /// <summary>
        /// Gets shipping response from shipping provider for order
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <returns>Shipping provider response</returns>
        Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderAsync(Order order);

        /// <summary>
        /// Gets shipping responses from shipping provider for orders
        /// </summary>
        /// <param name="orders">Order instances</param>
        /// <returns>Shipping provider responses</returns>
        Task<IList<UpdatedShipmentRequestModel>> GetShippingRequestsFromShippingProviderAsync(IEnumerable<Order> orders);

        /// <summary>
        /// Gets shipping response from shipping provider for shipment request Id
        /// </summary>
        /// <param name="shipmentRequestId">Shipment request Id</param>
        /// <returns>Shipping provider response</returns>
        Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderByShipmentRequestIdAsync(int shipmentRequestId);

        /// <summary>
        /// Gets shipping responses from sjipping provider for shipment request Ids
        /// </summary>
        /// <param name="shipmentRequestIds">Shipment request Ids</param>
        /// <returns>Shipping provider responses</returns>
        Task<IList<UpdatedShipmentRequestModel>> GetShippingRequestsFromShippingProviderByShipmentRequestIdsAsync(IEnumerable<int> shipmentRequestIds);

        /// <summary>
        /// Gets shipping response from shipping provider for shipment request
        /// </summary>
        /// <param name="shipmentRequest">Shipment request instance</param>
        /// <returns>Shipping provider response</returns>
        Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderAsync(ShipmentRequest shipmentRequest);

        /// <summary>
        /// Gets shipping responses from shipping provider for shipment requests
        /// </summary>
        /// <param name="shipmentRequests">Shipment requests</param>
        /// <returns>Shipping provider responses</returns>
        Task<IList<UpdatedShipmentRequestModel>> GetShippingRequestsFromShippingProviderAsync(IEnumerable<ShipmentRequest> shipmentRequests);

        /// <summary>
        /// Gets shipping response from shipping provider for shipment request and order
        /// </summary>
        /// <param name="shipmentRequest">Shipment request instance</param>
        /// <param name="order">Order instance</param>
        /// <returns>Shipping provider response</returns>
        Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderAsync(ShipmentRequest shipmentRequest, Order order);

        /// <summary>
        /// Send a shipping request to a shipping provider
        /// </summary>
        /// <param name="orderId">Order Id</param>
        /// <returns>Shipping provider response</returns>
        Task<IList<CreatedShipmentRequestModel>> SendShippingRequestToShippingProviderAsync(int orderId, int addressId);

        /// <summary>
        /// Send a shipping request to a shipping provider
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <returns>Shipping provider response</returns>
        Task<IList<CreatedShipmentRequestModel>> SendShippingRequestToShippingProviderAsync(Order order, Address address);

        /// <summary>
        /// Cancel a shipping request to a shipping provider
        /// </summary>
        /// <param name="shipmentRequestId">Shipment request Id</param>
        /// <returns>Shipping provider response</returns>
        Task<IList<ShipmentCanceledModel>> CancelShippingRequestToShippingProviderByShipmentRequestIdAsync(int shipmentRequestId);

        /// <summary>
        /// Cancel a shipping request to shipping provider
        /// </summary>
        /// <param name="shipmentRequest">Shipment request instance</param>
        /// <returns>Shipping provider response</returns>
        Task<IList<ShipmentCanceledModel>> CancelShippingRequestToShippingProviderAsync(ShipmentRequest shipmentRequest);

        /// <summary>
        /// Cancel a shipping request to a shipping provider
        /// </summary>
        /// <param name="orderId">Order Id</param>
        /// <returns>Shipping provider response</returns>
        Task<IList<ShipmentCanceledModel>> CancelShippingRequestToShippingProviderByOrderIdAsync(int orderId);

        /// <summary>
        /// Cancel a shipping request to a shipping provider
        /// </summary>
        /// <param name="order">Order instance</param>
        /// <returns>Shipping provider response</returns>
        Task<IList<ShipmentCanceledModel>> CancelShippingRequestToShippingProviderAsync(Order order);
    }
}
