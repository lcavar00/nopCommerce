using System;
using Nop.Core;

namespace Nop.Plugin.Misc.LowestPrice.Domain
{
    public class ProductPriceLog : BaseEntity
    {
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public decimal Price { get; set; }
        public bool IsDiscountedPrice { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }
}
