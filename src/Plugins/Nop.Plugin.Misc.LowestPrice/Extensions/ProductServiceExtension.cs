using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Plugin.Misc.LowestPrice.Domain;
using Nop.Plugin.Misc.LowestPrice.Services;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;

namespace Nop.Plugin.Misc.LowestPrice.Extensions
{
    public partial class ProductServiceExtension : ProductService, IProductService
    {
        private readonly IProductPriceHistoryService _productPriceHistoryService;
        private readonly ITaxService _taxService;

        #region Ctor

        public ProductServiceExtension(CatalogSettings catalogSettings,
            CommonSettings commonSettings, 
            IAclService aclService, 
            ICustomerService customerService, 
            IDateRangeService dateRangeService, 
            ILanguageService languageService, 
            ILocalizationService localizationService, 
            IProductPriceHistoryService productPriceHistoryService,
            IProductAttributeParser productAttributeParser, 
            IProductAttributeService productAttributeService, 
            IRepository<CrossSellProduct> crossSellProductRepository, 
            IRepository<DiscountProductMapping> discountProductMappingRepository, 
            IRepository<LocalizedProperty> localizedPropertyRepository, 
            IRepository<Product> productRepository, 
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository, 
            IRepository<ProductAttributeMapping> productAttributeMappingRepository, 
            IRepository<ProductCategory> productCategoryRepository, 
            IRepository<ProductManufacturer> productManufacturerRepository, 
            IRepository<ProductPicture> productPictureRepository, 
            IRepository<ProductProductTagMapping> productTagMappingRepository, 
            IRepository<ProductReview> productReviewRepository, 
            IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository, 
            IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository, 
            IRepository<ProductTag> productTagRepository, 
            IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository, 
            IRepository<RelatedProduct> relatedProductRepository, 
            IRepository<Shipment> shipmentRepository, 
            IRepository<StockQuantityHistory> stockQuantityHistoryRepository, 
            IRepository<TierPrice> tierPriceRepository, 
            IRepository<Warehouse> warehouseRepository, 
            IStaticCacheManager staticCacheManager, 
            IStoreService storeService,
            IStoreMappingService storeMappingService, 
            ITaxService taxService,
            IWorkContext workContext, 
            LocalizationSettings localizationSettings) : base(catalogSettings, commonSettings, aclService, customerService, dateRangeService, languageService, localizationService, productAttributeParser, productAttributeService, crossSellProductRepository, discountProductMappingRepository, localizedPropertyRepository, productRepository, productAttributeCombinationRepository, productAttributeMappingRepository, productCategoryRepository, productManufacturerRepository, productPictureRepository, productTagMappingRepository, productReviewRepository, productReviewHelpfulnessRepository, productSpecificationAttributeRepository, productTagRepository, productWarehouseInventoryRepository, relatedProductRepository, shipmentRepository, stockQuantityHistoryRepository, tierPriceRepository, warehouseRepository, staticCacheManager, storeService, storeMappingService, workContext, localizationSettings)
        {
            _productPriceHistoryService = productPriceHistoryService;
            _taxService = taxService;
        }

        #endregion

        public override async Task InsertProductAsync(Product product)
        {
            await _productRepository.InsertAsync(product);

            var lowestPrice = new Domain.ProductPriceLog
            {
                ProductId = product.Id,
                IsDiscountedPrice = false,
                Price = product.Price,
                Sku = product.Sku,
            };

            await _productPriceHistoryService.InsertProductPriceLogAsync(lowestPrice);
        }

        public override async Task UpdateProductAsync(Product product)
        {
            var productPriceLogs = await _productPriceHistoryService.GetProductPriceLogsAsyncByProduct(product);
            var (finalProductPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.Price);

            if (productPriceLogs.Count == 0 || productPriceLogs.Any(a => a.Price == finalProductPriceBase))
            {
                var productPriceLog = new ProductPriceLog
                {
                    ProductId = product.Id,
                    IsDiscountedPrice = false,
                    Price = finalProductPriceBase,
                    Sku = product.Sku,
                    CreatedOnUtc = DateTime.UtcNow,
                };

                await _productPriceHistoryService.InsertProductPriceLogAsync(productPriceLog);
            }

            await base.UpdateProductAsync(product);
        }

    }
}
