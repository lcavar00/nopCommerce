using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.LowestPrice.Domain;

namespace Nop.Plugin.Misc.LowestPrice.Services
{
    public interface IProductPriceHistoryService
    {
        Task<IList<ProductPriceLog>> GetAllProductPriceLogsAsync();
        Task<IList<ProductPriceLog>> GetProductPriceLogsAsyncByProduct(Product product);
        Task<IList<ProductPriceLog>> GetProductDiscountedPriceLogsByProductAsync(Product product);
        Task InsertProductPriceLogAsync(ProductPriceLog lowestPrice);
        Task InsertProductPriceLogsAsync(IEnumerable<ProductPriceLog> lowestPrices);
        Task UpdateProductPriceLogAsync(ProductPriceLog productPriceLog);
        Task UpdateProductPriceLogsAsync(IEnumerable<ProductPriceLog> productPriceLogs);
        Task DeleteProductPriceLogAsync(ProductPriceLog productPriceLog);
        Task DeleteProductPriceLogsAsync(IEnumerable<ProductPriceLog> productPriceLogs);
    }
}