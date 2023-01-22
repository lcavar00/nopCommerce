using Nop.Core;
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
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.MorePrices.Extensions
{
    public partial class ManufacturerServiceExtension : ManufacturerService, IManufacturerServiceExtension
    {
        private readonly IRepository<PriceManufacturer> _priceManufacturerRepository;
        private readonly IRepository<Price> _priceRepository;

        public ManufacturerServiceExtension(CatalogSettings catalogSettings,
            ICacheKeyService cacheKeyService, 
            ICustomerService customerService, 
            IEventPublisher eventPublisher, 
            IRepository<AclRecord> aclRepository, 
            IRepository<DiscountManufacturerMapping> discountManufacturerMappingRepository, 
            IRepository<Manufacturer> manufacturerRepository, 
            IRepository<PriceManufacturer> priceManufacturerRepository,
            IRepository<Price> priceRepository,
            IRepository<Product> productRepository, 
            IRepository<ProductManufacturer> productManufacturerRepository, 
            IRepository<StoreMapping> storeMappingRepository, 
            IStoreContext storeContext, 
            IWorkContext workContext) : base(catalogSettings, cacheKeyService, customerService, eventPublisher, aclRepository, discountManufacturerMappingRepository, manufacturerRepository, productRepository, productManufacturerRepository, storeMappingRepository, storeContext, workContext)
        {
            _priceManufacturerRepository = priceManufacturerRepository;
            _priceRepository = priceRepository;
        }

        public IEnumerable<PriceManufacturer> GetPriceManufacturersByPriceId(int id)
        {
            throw new NotImplementedException();
        }
    }
}
