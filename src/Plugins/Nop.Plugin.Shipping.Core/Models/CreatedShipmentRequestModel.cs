using System.Collections.Generic;

namespace Nop.Plugin.Shipping.Core.Models
{
    public class CreatedShipmentRequestModel
    {
        public CreatedShipmentRequestModel()
        {
            ShipmentRequests = new List<ShipmentRequestModel>();
        }

        public string ResponseMessage { get; set; }
        public List<ShipmentRequestModel> ShipmentRequests { get; set; } 
    }
}
