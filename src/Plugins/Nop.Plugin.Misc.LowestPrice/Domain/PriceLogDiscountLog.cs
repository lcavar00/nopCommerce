using Nop.Core;

namespace Nop.Plugin.Misc.LowestPrice.Domain
{
    /// <summary>
    /// Represents a mapping table between a price log (ProductPriceLog / ProductAttributeCombinationPriceLog) and DiscountLog
    /// </summary>
    public class PriceLogDiscountLog : BaseEntity
    {
        public virtual string EntityName { get; set; }
        public virtual int EntityId { get; set; }
        public virtual int DiscountLogId { get; set; }
    }
}
