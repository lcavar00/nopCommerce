using Nop.Core;

namespace Nop.Plugin.Misc.MorePrices.Domain
{
    public class PriceManufacturer : BaseEntity
    {
        public virtual int PriceId { get; set; }
        public virtual int ManufacturerId { get; set; }
    }
}
