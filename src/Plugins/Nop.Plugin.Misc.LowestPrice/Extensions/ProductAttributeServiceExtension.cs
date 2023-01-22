using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.LowestPrice.Domain;
using Nop.Plugin.Misc.LowestPrice.Services;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Orders;
using Nop.Services.Tax;

namespace Nop.Plugin.Misc.LowestPrice.Extensions
{
    public partial class ProductAttributeServiceExtension : ProductAttributeService, IProductAttributeService
    {
        #region Fields

        private readonly IProductAttributeCombinationPriceHistoryService _productAttributeCombinationPriceHistoryService;
        private readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;

        #endregion

        #region Ctor

        public ProductAttributeServiceExtension(IProductAttributeCombinationPriceHistoryService productAttributeCombinationPriceHistoryService,
            IRepository<PredefinedProductAttributeValue> predefinedProductAttributeValueRepository, 
            IRepository<Product> productRepository, 
            IRepository<ProductAttribute> productAttributeRepository, 
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository, 
            IRepository<ProductAttributeMapping> productAttributeMappingRepository, 
            IRepository<ProductAttributeValue> productAttributeValueRepository, 
            IStaticCacheManager staticCacheManager) : base(predefinedProductAttributeValueRepository, productRepository, productAttributeRepository, productAttributeCombinationRepository, productAttributeMappingRepository, productAttributeValueRepository, staticCacheManager)
        {
            _productAttributeCombinationPriceHistoryService = productAttributeCombinationPriceHistoryService;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
        }

        #endregion

        public override async Task InsertProductAttributeCombinationAsync(ProductAttributeCombination combination)
        {
            await _productAttributeCombinationRepository.InsertAsync(combination);

            if (combination.OverriddenPrice.HasValue)
            {
                var lowestPrice = new ProductAttributeCombinationPriceLog
                {
                    ProductAttributeCombinationId = combination.Id,
                    IsDiscountedPrice = false,
                    Price = combination.OverriddenPrice.Value,
                    Sku = combination.Sku,
                };

                await _productAttributeCombinationPriceHistoryService.InsertProductAttributeCombinationPriceLogAsync(lowestPrice);
            }
        }

        public override async Task UpdateProductAttributeCombinationAsync(ProductAttributeCombination combination)
        {

            if (combination.OverriddenPrice.HasValue)
            {
                var productAttributeCombinationPriceLogs = await _productAttributeCombinationPriceHistoryService.GetProductAttributeCombinationPriceLogsAsync(combination);
                if (productAttributeCombinationPriceLogs.Count == 0 || productAttributeCombinationPriceLogs.Any(b => b.Price == combination.OverriddenPrice.Value))
                {
                    var productAttributeCombinationPriceLog = new ProductAttributeCombinationPriceLog
                    {
                        ProductAttributeCombinationId = combination.Id,
                        IsDiscountedPrice = false,
                        Price = combination.OverriddenPrice.Value,
                        Sku = combination.Sku,
                    };

                    await _productAttributeCombinationPriceHistoryService.InsertProductAttributeCombinationPriceLogAsync(productAttributeCombinationPriceLog);
                }
                else
                {
                    var productAttributeCombinationPriceLog = productAttributeCombinationPriceLogs.First(a => a.Price == combination.OverriddenPrice);
                    productAttributeCombinationPriceLog.UpdatedOnUtc = DateTime.UtcNow;

                    await _productAttributeCombinationPriceHistoryService.UpdateProductAttributeCombinationPriceLogAsync(productAttributeCombinationPriceLog);
                }
            }

            await base.UpdateProductAttributeCombinationAsync(combination);
        }

    }
}
