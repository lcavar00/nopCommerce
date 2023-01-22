using System;
using Nop.Core;

namespace Nop.Plugin.Shipping.Core.Domain
{
    public class ShipmentRequest : BaseEntity
    {
        public virtual Guid ShipmentRequestGuid { get; set; }

        public virtual string PackageReference { get; set; }

        public virtual int OrderId { get; set; }
        public virtual Guid OrderGuid { get; set; }

        public virtual int ShipmentPickupAddressId { get; set; }
        public virtual bool ShipmentCreated { get; set; }
        public virtual int? ShipmentId { get; set; }

        public virtual DateTime? CreatedAt { get; set; }
        public virtual DateTime? UpdatedAt { get; set; }

        public virtual string ShippingProvider { get; set; }
        public virtual string ShippingProviderSystemName { get; set; }
        public virtual bool NotificationSent { get; set; }
        public virtual bool IsDeleted { get; set; }
    }
}
