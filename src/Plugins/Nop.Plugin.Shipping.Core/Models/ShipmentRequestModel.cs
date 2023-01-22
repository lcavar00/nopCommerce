using System;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Shipping.Core.Models
{
    public class ShipmentRequestModel
    {
        public int Id { get; set; }
        public Guid ShipmentRequestGuid { get; set; }

        public string PackageReference { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }
        public Guid OrderGuid { get; set; }
        public int ShipmentPickupAddressId { get; set; }
        public bool ShipmentCreated { get; set; }
        public int? ShipmentId { get; set; }
        public Shipment Shipment { get; set; }
        public ShipmentModel ShipmentModel { get; set; }

        public string ShippingProvider { get; set; }
        public string ShippingProviderSystemName { get; set; }
    }
}
