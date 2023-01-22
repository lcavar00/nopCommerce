using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Plugin.Misc.LowestPrice.Domain;

namespace Nop.Plugin.Misc.LowestPrice.Services
{
    public partial class ProductAttributeCombinationPriceHistoryService : IProductAttributeCombinationPriceHistoryService
    {
        #region Fields

        private readonly IRepository<ProductAttributeCombinationPriceLog> _pacPriceHistoryRepository;

        #endregion

        #region Ctor

        public ProductAttributeCombinationPriceHistoryService(IRepository<ProductAttributeCombinationPriceLog> pacPriceHistoryRepository)
        {
            _pacPriceHistoryRepository = pacPriceHistoryRepository;
        }

        #endregion

        public async Task<List<ProductAttributeCombinationPriceLog>> GetAllProductAttributeCombintaionPriceLogsAsync()
        {
            return await _pacPriceHistoryRepository.Table.ToListAsync();
        }

        public async Task<IList<ProductAttributeCombinationPriceLog>> GetProductAttributeCombinationPriceLogsAsync(ProductAttributeCombination productAttributeCombination)
        {
            var productAttributeCombinationLogs = await _pacPriceHistoryRepository.Table.Where(a => a.ProductAttributeCombinationId == productAttributeCombination.Id && a.IsDiscountedPrice == false).ToListAsync();

            return productAttributeCombinationLogs;
        }

        public async Task<IList<ProductAttributeCombinationPriceLog>> GetProductAttributeCombinationDiscountedPriceLogsAsync(ProductAttributeCombination productAttributeCombination)
        {
            var productAttributeCombinationLogs = await _pacPriceHistoryRepository.Table.Where(a => a.ProductAttributeCombinationId == productAttributeCombination.Id && a.IsDiscountedPrice == true).ToListAsync();

            return productAttributeCombinationLogs;
        }

        public async Task InsertProductAttributeCombinationPriceLogAsync(ProductAttributeCombinationPriceLog productAttributeCombinationPriceLog)
        {
            productAttributeCombinationPriceLog.CreatedOnUtc = DateTime.UtcNow;
            await _pacPriceHistoryRepository.InsertAsync(productAttributeCombinationPriceLog);
        }

        public async Task InsertProductAttributeCombinationPriceLogsAsync(IEnumerable<ProductAttributeCombinationPriceLog> productAttributeCombinationPriceLog)
        {
            foreach (var lowestPrice in productAttributeCombinationPriceLog)
            {
                lowestPrice.CreatedOnUtc = DateTime.UtcNow;
            }
            await _pacPriceHistoryRepository.InsertAsync(productAttributeCombinationPriceLog.ToArray());
        }

        public async Task UpdateProductAttributeCombinationPriceLogAsync(ProductAttributeCombinationPriceLog productAttributeCombinationPriceLog)
        {
            productAttributeCombinationPriceLog.UpdatedOnUtc = DateTime.UtcNow;
            await _pacPriceHistoryRepository.UpdateAsync(productAttributeCombinationPriceLog);
        }

        public async Task UpdateProductAttributeCombinationPriceLogsAsync(IEnumerable<ProductAttributeCombinationPriceLog> productAttributeCombinationPriceLogs)
        {
            foreach(var productAttributeCombinationPriceLog in productAttributeCombinationPriceLogs)
            {
                productAttributeCombinationPriceLog.UpdatedOnUtc = DateTime.UtcNow;
            }
            await _pacPriceHistoryRepository.UpdateAsync(productAttributeCombinationPriceLogs.ToArray());
        }

        public async Task DeleteProductAttributeCombinationPriceLogAsync(ProductAttributeCombinationPriceLog productAttributeCombinationPriceLog)
        {
            await _pacPriceHistoryRepository.DeleteAsync(productAttributeCombinationPriceLog);
        }

        public async Task DeleteProductAttributeCombinationPriceLogsAsync(IEnumerable<ProductAttributeCombinationPriceLog> productAttributeCombinationPriceLogs)
        {
            await _pacPriceHistoryRepository.DeleteAsync(productAttributeCombinationPriceLogs.ToArray());
        }
    }
}
