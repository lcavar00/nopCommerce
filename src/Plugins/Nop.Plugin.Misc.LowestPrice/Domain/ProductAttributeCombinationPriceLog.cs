using System;
using Nop.Core;

namespace Nop.Plugin.Misc.LowestPrice.Domain
{
    public class ProductAttributeCombinationPriceLog : BaseEntity
    {
        public int ProductAttributeCombinationId { get; set; }
        public bool IsDiscountedPrice { get; set; }
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }
}
