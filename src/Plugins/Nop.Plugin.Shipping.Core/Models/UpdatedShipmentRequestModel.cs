using Nop.Services.Shipping.Tracking;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Core.Models
{
    public class UpdatedShipmentRequestModel
    {
        public UpdatedShipmentRequestModel()
        {
            ShipmentStatusEvent = new List<ShipmentStatusEvent>();
        }

        public ShipmentRequestModel ShipmentRequestModel { get; set; }
        public IList<ShipmentStatusEvent> ShipmentStatusEvent { get; set; } 
        public string ShipmentRequestStatus { get; set; }
    }
}
