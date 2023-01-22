using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.LowestPrice.Domain;

namespace Nop.Plugin.Misc.LowestPrice.Services
{
    public partial class ProductPriceHistoryService : IProductPriceHistoryService
    {
        #region Fields

        private readonly IRepository<ProductPriceLog> _productPriceLogRepository;

        #endregion

        #region Ctor

        public ProductPriceHistoryService(IRepository<ProductPriceLog> productPriceLogRepository)
        {
            _productPriceLogRepository = productPriceLogRepository;
        }

        #endregion

        public async Task<IList<ProductPriceLog>> GetAllProductPriceLogsAsync()
        {
            return await _productPriceLogRepository.Table.ToListAsync();
        }

        public async Task<IList<ProductPriceLog>> GetProductPriceLogsAsyncByProduct(Product product)
        {
            var productPriceLogs = await _productPriceLogRepository.Table.Where(a => a.ProductId == product.Id && a.IsDiscountedPrice == false).ToListAsync();

            return productPriceLogs;
        }

        public async Task<IList<ProductPriceLog>> GetProductDiscountedPriceLogsByProductAsync(Product product)
        {
            var productPriceLogs = await _productPriceLogRepository.Table
                .Where(a => a.ProductId == product.Id && a.IsDiscountedPrice == true).ToListAsync();

            return productPriceLogs;
        }

        public async Task InsertProductPriceLogAsync(ProductPriceLog productPriceLogs)
        {
            productPriceLogs.CreatedOnUtc = DateTime.UtcNow;
            await _productPriceLogRepository.InsertAsync(productPriceLogs);
        }

        public async Task InsertProductPriceLogsAsync(IEnumerable<ProductPriceLog> productPriceLogs)
        {
            foreach(var lowestPrice in productPriceLogs)
            {
                lowestPrice.CreatedOnUtc = DateTime.UtcNow;
            }
            await _productPriceLogRepository.InsertAsync(productPriceLogs.ToArray());
        }

        public async Task UpdateProductPriceLogAsync(ProductPriceLog productPriceLog)
        {
            productPriceLog.UpdatedOnUtc = DateTime.UtcNow;
            await _productPriceLogRepository.UpdateAsync(productPriceLog);
        }

        public async Task UpdateProductPriceLogsAsync(IEnumerable<ProductPriceLog> productPriceLogs)
        {
            foreach(var productPriceLog in productPriceLogs)
            {
                productPriceLog.UpdatedOnUtc = DateTime.UtcNow;
            }
            await _productPriceLogRepository.UpdateAsync(productPriceLogs.ToArray());
        }

        public async Task DeleteProductPriceLogAsync(ProductPriceLog productPriceLog)
        {
            await _productPriceLogRepository.DeleteAsync(productPriceLog);
        }

        public async Task DeleteProductPriceLogsAsync(IEnumerable<ProductPriceLog> productPriceLogs)
        {
            await _productPriceLogRepository.DeleteAsync(productPriceLogs.ToArray());
        }
    }
}
