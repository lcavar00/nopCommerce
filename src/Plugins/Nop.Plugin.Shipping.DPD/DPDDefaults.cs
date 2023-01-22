using System.Collections.Generic;
using Nop.Core.Domain.Shipping;

namespace Nop.Plugin.Shipping.DPD
{
    public static class DPDDefaults
    {
        public static Dictionary<string, ShippingStatus> Statuses = new Dictionary<string, ShippingStatus>
        {
            {"CREATED", ShippingStatus.NotYetShipped },
            { "PRINTED", ShippingStatus.NotYetShipped },
            { "CANCELLED", ShippingStatus.NotYetShipped },
            { "CANCELLED_SENT", ShippingStatus.NotYetShipped },
            { "PREPARED_TO_SEND", ShippingStatus.NotYetShipped },
            { "PREPARED_TO_CANCEL", ShippingStatus.NotYetShipped },
            { "SENT", ShippingStatus.NotYetShipped },
            { "DRIVERS_PICK_UP", ShippingStatus.Shipped },
            { "PICK_UP", ShippingStatus.Shipped },
            { "INBOUND", ShippingStatus.Shipped  },
            { "OUT_FOR_DELIVERY", ShippingStatus.Shipped  },
            { "DELIVERED", ShippingStatus.Delivered },
            { "DELIVERY_ATTEMPT_NOT_SUCCESSFUL", ShippingStatus.Shipped  },
            { "DRIVERS_RETURN", ShippingStatus.Shipped  },
            { "WAREHOUSE", ShippingStatus.Shipped  },
            { "SYSTEM_RETURN", ShippingStatus.NotYetShipped  },
            { "RETURN_TO_SENDER", ShippingStatus.NotYetShipped  },
            { "Parcel does not exist", ShippingStatus.NotYetShipped  },
            { "CANCELLED_AND_SENT", ShippingStatus.NotYetShipped },
            { "DELETED", ShippingStatus.NotYetShipped },


        };
    }
}
