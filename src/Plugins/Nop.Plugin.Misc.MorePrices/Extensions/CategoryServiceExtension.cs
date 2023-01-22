using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Plugin.Misc.MorePrices.Domain;
using Nop.Services.Caching;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Services.Stores;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.MorePrices.Extensions
{
    public partial class CategoryServiceExtension : CategoryService, ICategoryServiceExtension
    {
        private readonly IRepository<PriceCategory> _priceCategoryRepository;
        private readonly IRepository<Price> _priceRepository;

        public CategoryServiceExtension(CatalogSettings catalogSettings,
            IAclService aclService, 
            ICacheKeyService cacheKeyService, 
            ICustomerService customerService, 
            IEventPublisher eventPublisher, 
            ILocalizationService localizationService, 
            IRepository<AclRecord> aclRepository, 
            IRepository<Category> categoryRepository, 
            IRepository<DiscountCategoryMapping> discountCategoryMappingRepository, 
            IRepository<PriceCategory> priceCategoryRepository,
            IRepository<Price> priceRepository,
            IRepository<Product> productRepository, 
            IRepository<ProductCategory> productCategoryRepository, 
            IRepository<StoreMapping> storeMappingRepository, 
            IStaticCacheManager staticCacheManager, 
            IStoreContext storeContext, 
            IStoreMappingService storeMappingService, 
            IWorkContext workContext) : base(catalogSettings, aclService, cacheKeyService, customerService, eventPublisher, localizationService, aclRepository, categoryRepository, discountCategoryMappingRepository, productRepository, productCategoryRepository, storeMappingRepository, staticCacheManager, storeContext, storeMappingService, workContext)
        {
            _priceCategoryRepository = priceCategoryRepository;
            _priceRepository = priceRepository;
        }

        public IEnumerable<PriceCategory> GetPriceCategoriesByPriceId(int id)
        {
            throw new System.NotImplementedException();
        }
    }
}
