using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Shipping.Core.Models;
using Nop.Plugin.Shipping.Core.Services;
using Nop.Services.Orders;
using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.Core.Factories
{
    public class ShipmentRequestModelFactory : IShipmentRequestModelFactory
    {
        #region Fields

        private readonly IOrderService _orderService;
        private readonly IShipmentRequestService _shipmentRequestService;
        private readonly IShipmentService _shipmentService;

        #endregion

        #region Ctor

        public ShipmentRequestModelFactory(IOrderService orderService,
            IShipmentRequestService shipmentRequestService,
            IShipmentService shipmentService)
        {
            _orderService = orderService;
            _shipmentRequestService = shipmentRequestService;
            _shipmentService = shipmentService;
        }

        #endregion

        public async Task<ShipmentRequestModel> PrepareShipmentRequestModelAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            return await PrepareShipmentRequestModelAsync(order);
        }

        public async Task<ShipmentRequestModel> PrepareShipmentRequestModelAsync(Order order)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByOrderIdAsync(order.Id);
            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentRequest?.ShipmentId ?? 0);

            if(shipmentRequest != null)
            {
                var model = new ShipmentRequestModel
                {
                    Id = shipmentRequest.Id,
                    ShipmentRequestGuid = shipmentRequest.ShipmentRequestGuid,
                    Order = order,
                    OrderGuid = order.OrderGuid,
                    OrderId = order.Id,
                    PackageReference = null,
                    Shipment = shipment,
                    ShipmentCreated = shipmentRequest.ShipmentCreated,
                    ShipmentId = shipment?.Id,
                    ShippingProvider = shipmentRequest.ShippingProvider,
                    ShippingProviderSystemName = shipmentRequest.ShippingProviderSystemName,
                };

                return model;
            }
            else
            {
                var model = new ShipmentRequestModel
                {
                    Order = order,
                    OrderGuid = order.OrderGuid,
                    OrderId = order.Id,
                };

                return model;
            }
        }
    }
}
