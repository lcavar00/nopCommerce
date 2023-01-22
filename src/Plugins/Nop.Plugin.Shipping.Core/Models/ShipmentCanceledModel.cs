using System;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Shipping.Core.Domain;

namespace Nop.Plugin.Shipping.Core.Models
{
    public class ShipmentCanceledModel
    {
        public int ShipmentRequestId { get; set; }
        public Guid ShipmentRequestGuid { get; set; }
        public ShipmentRequest ShipmentRequest { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string ResponseMessage { get; set; }
    }
}
