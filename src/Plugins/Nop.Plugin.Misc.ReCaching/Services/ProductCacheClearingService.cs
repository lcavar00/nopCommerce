using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Discounts;

namespace Nop.Plugin.Misc.ReCaching.Services
{
    public class ProductCacheClearingService : CacheEventConsumer<Product>, ICacheClearingService<Product>
    {
        private readonly IRepository<Product> _productRepository;

        public ProductCacheClearingService(IRepository<Product> productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task ClearCacheAsync()
        {
            var products = _productRepository.Table.Where(a => !a.Deleted);

            await ClearCacheAsync(products);
        }

        public async Task ClearCacheAsync(IEnumerable<Product> entities)
        {
            foreach(var entity in entities)
            {
                await ClearCacheAsync(entity);
            }
        }

        protected override async Task ClearCacheAsync(Product product) 
        {
            await RemoveAsync(NopEntityCacheDefaults<Product>.ByIdCacheKey, product.Id);
            await RemoveByPrefixAsync(NopCatalogDefaults.ProductManufacturersByProductPrefix, product);
            await RemoveAsync(NopCatalogDefaults.ProductsHomepageCacheKey);
            await RemoveByPrefixAsync(NopCatalogDefaults.ProductPricePrefix, product);
            await RemoveByPrefixAsync(NopEntityCacheDefaults<ShoppingCartItem>.AllPrefix);
            await RemoveByPrefixAsync(NopCatalogDefaults.FeaturedProductIdsPrefix);

            await RemoveAsync(NopDiscountDefaults.AppliedDiscountsCacheKey, nameof(Product), product);

            await base.ClearCacheAsync(product);
        }
    }
}
