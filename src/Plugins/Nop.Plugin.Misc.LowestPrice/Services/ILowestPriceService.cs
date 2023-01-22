using System.Threading.Tasks;

namespace Nop.Plugin.Misc.LowestPrice.Services
{
    public interface ILowestPriceService
    {
        Task<decimal> GetLowestPriceAsync(string sku);
        Task<decimal?> GetLowestDiscountedPriceAsync(string sku);
        Task<decimal> GetLowestPriceAsync(int productId);
        Task<decimal?> GetLowestDiscountPriceAsync(int productId);
        Task LogPricesAndDiscountsAsync();
    }
}