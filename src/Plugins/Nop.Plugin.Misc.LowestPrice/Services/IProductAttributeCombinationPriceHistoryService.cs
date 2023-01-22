using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.LowestPrice.Domain;

namespace Nop.Plugin.Misc.LowestPrice.Services
{
    public interface IProductAttributeCombinationPriceHistoryService
    {
        Task<List<ProductAttributeCombinationPriceLog>> GetAllProductAttributeCombintaionPriceLogsAsync();
        Task<IList<ProductAttributeCombinationPriceLog>> GetProductAttributeCombinationPriceLogsAsync(ProductAttributeCombination productAttributeCombination);
        Task<IList<ProductAttributeCombinationPriceLog>> GetProductAttributeCombinationDiscountedPriceLogsAsync(ProductAttributeCombination productAttributeCombination);
        Task InsertProductAttributeCombinationPriceLogAsync(ProductAttributeCombinationPriceLog lowestPrice);
        Task InsertProductAttributeCombinationPriceLogsAsync(IEnumerable<ProductAttributeCombinationPriceLog> lowestPrices);
        Task UpdateProductAttributeCombinationPriceLogAsync(ProductAttributeCombinationPriceLog productAttributeCombinationPriceLog);
        Task UpdateProductAttributeCombinationPriceLogsAsync(IEnumerable<ProductAttributeCombinationPriceLog> productAttributeCombinationPriceLogs);
        Task DeleteProductAttributeCombinationPriceLogAsync(ProductAttributeCombinationPriceLog productAttributeCombinationPriceLog);
        Task DeleteProductAttributeCombinationPriceLogsAsync(IEnumerable<ProductAttributeCombinationPriceLog> productAttributeCombinationPriceLogs);
    }
}