namespace Nop.Plugin.Misc.LowestPrice.Models
{
    public class LowestPriceModel
    {
        public int ProductId { get; set; }
        public string Sku { get; set; }
        public decimal PriceValue { get; set; }
        public decimal? DiscountedPriceValue { get; set; }
        public string Price { get; set; }
        public string DiscountedPrice { get; set; }
    }
}
