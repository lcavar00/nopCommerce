using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Plugin.Misc.LowestPrice.Domain;
using Nop.Plugin.Misc.LowestPrice.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Misc.LowestPrice.Extensions
{
    public partial class ProductModelFactoryExtension : ProductModelFactory
    {
        #region Fields

        private readonly CatalogSettings _catalogSettings;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IDateRangeService _dateRangeService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductPriceHistoryService _productPriceHistoryService;
        private readonly IProductService _productService;
        private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IVendorService _vendorService;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly MediaSettings _mediaSettings;
        private readonly OrderSettings _orderSettings;
        private readonly SeoSettings _seoSettings;
        private readonly ShippingSettings _shippingSettings;
        private readonly VendorSettings _vendorSettings;

        #endregion

        #region Ctor

        public ProductModelFactoryExtension(CaptchaSettings captchaSettings,
            CatalogSettings catalogSettings, 
            CustomerSettings customerSettings, 
            ICategoryService categoryService, 
            ICurrencyService currencyService, 
            ICustomerService customerService, 
            IDateRangeService dateRangeService, 
            IDateTimeHelper dateTimeHelper, 
            IDownloadService downloadService, 
            IGenericAttributeService genericAttributeService, 
            ILocalizationService localizationService, 
            IManufacturerService manufacturerService, 
            IPermissionService permissionService, 
            IPictureService pictureService, 
            IPriceCalculationService priceCalculationService, 
            IPriceFormatter priceFormatter, 
            IProductAttributeParser productAttributeParser, 
            IProductAttributeService productAttributeService, 
            IProductPriceHistoryService productPriceHistoryService,
            IProductService productService, 
            IProductTagService productTagService, 
            IProductTemplateService productTemplateService, 
            IReviewTypeService reviewTypeService, 
            ISpecificationAttributeService specificationAttributeService, 
            IStaticCacheManager staticCacheManager, 
            IStoreContext storeContext, 
            IShoppingCartModelFactory shoppingCartModelFactory, 
            ITaxService taxService, 
            IUrlRecordService urlRecordService, 
            IVendorService vendorService, 
            IWebHelper webHelper, 
            IWorkContext workContext, 
            MediaSettings mediaSettings, 
            OrderSettings orderSettings, 
            SeoSettings seoSettings, 
            ShippingSettings shippingSettings, 
            VendorSettings vendorSettings) : base(captchaSettings, catalogSettings, customerSettings, categoryService, currencyService, customerService, dateRangeService, dateTimeHelper, downloadService, genericAttributeService, localizationService, manufacturerService, permissionService, pictureService, priceCalculationService, priceFormatter, productAttributeParser, productAttributeService, productService, productTagService, productTemplateService, reviewTypeService, specificationAttributeService, staticCacheManager, storeContext, shoppingCartModelFactory, taxService, urlRecordService, vendorService, webHelper, workContext, mediaSettings, orderSettings, seoSettings, shippingSettings, vendorSettings)
        {
            _catalogSettings = catalogSettings;
            _currencyService = currencyService;
            _customerService = customerService;
            _dateRangeService = dateRangeService;
            _localizationService = localizationService;
            _permissionService = permissionService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productAttributeService = productAttributeService;
            _productPriceHistoryService = productPriceHistoryService;
            _productService = productService;
            _shoppingCartModelFactory = shoppingCartModelFactory;
            _storeContext = storeContext;
            _taxService = taxService;
            _urlRecordService = urlRecordService;
            _vendorService = vendorService;
            _webHelper = webHelper;
            _workContext = workContext;
            _mediaSettings = mediaSettings;
            _orderSettings = orderSettings;
            _seoSettings = seoSettings;
            _shippingSettings = shippingSettings;
            _vendorSettings = vendorSettings;
        }

        #endregion

        /// <summary>
        /// Prepare the product overview models
        /// </summary>
        /// <param name="products">Collection of products</param>
        /// <param name="preparePriceModel">Whether to prepare the price model</param>
        /// <param name="preparePictureModel">Whether to prepare the picture model</param>
        /// <param name="productThumbPictureSize">Product thumb picture size (longest side); pass null to use the default value of media settings</param>
        /// <param name="prepareSpecificationAttributes">Whether to prepare the specification attribute models</param>
        /// <param name="forceRedirectionAfterAddingToCart">Whether to force redirection after adding to cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the collection of product overview model
        /// </returns>
        public override async Task<IEnumerable<ProductOverviewModel>> PrepareProductOverviewModelsAsync(IEnumerable<Product> products,
            bool preparePriceModel = true, bool preparePictureModel = true,
            int? productThumbPictureSize = null, bool prepareSpecificationAttributes = false,
            bool forceRedirectionAfterAddingToCart = false)
        {
            if (products == null)
                throw new ArgumentNullException(nameof(products));

            var models = new List<ProductOverviewModel>();
            foreach (var product in products)
            {
                var model = new ProductOverviewModel
                {
                    Id = product.Id,
                    Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                    ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                    FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                    SeName = await _urlRecordService.GetSeNameAsync(product),
                    Sku = product.Sku,
                    ProductType = product.ProductType,
                    MarkAsNew = product.MarkAsNew &&
                        (!product.MarkAsNewStartDateTimeUtc.HasValue || product.MarkAsNewStartDateTimeUtc.Value < DateTime.UtcNow) &&
                        (!product.MarkAsNewEndDateTimeUtc.HasValue || product.MarkAsNewEndDateTimeUtc.Value > DateTime.UtcNow)
                };

                //price
                if (preparePriceModel)
                {
                    model.ProductPrice = await PrepareProductOverviewPriceModelAsync(product, forceRedirectionAfterAddingToCart);
                }

                //picture
                if (preparePictureModel)
                {
                    model.DefaultPictureModel = await PrepareProductOverviewPictureModelAsync(product, productThumbPictureSize);
                }

                //specs
                if (prepareSpecificationAttributes)
                {
                    model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);
                }

                //reviews
                model.ReviewOverviewModel = await PrepareProductReviewOverviewModelAsync(product);

                models.Add(model);
            }

            return models;
        }

        /// <summary>
        /// Prepare the product details model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="updatecartitem">Updated shopping cart item</param>
        /// <param name="isAssociatedProduct">Whether the product is associated</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product details model
        /// </returns>
        public override async Task<ProductDetailsModel> PrepareProductDetailsModelAsync(Product product,
            ShoppingCartItem updatecartitem = null, bool isAssociatedProduct = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            //standard properties
            var model = new ProductDetailsModel
            {
                Id = product.Id,
                Name = await _localizationService.GetLocalizedAsync(product, x => x.Name),
                ShortDescription = await _localizationService.GetLocalizedAsync(product, x => x.ShortDescription),
                FullDescription = await _localizationService.GetLocalizedAsync(product, x => x.FullDescription),
                MetaKeywords = await _localizationService.GetLocalizedAsync(product, x => x.MetaKeywords),
                MetaDescription = await _localizationService.GetLocalizedAsync(product, x => x.MetaDescription),
                MetaTitle = await _localizationService.GetLocalizedAsync(product, x => x.MetaTitle),
                SeName = await _urlRecordService.GetSeNameAsync(product),
                ProductType = product.ProductType,
                ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage,
                Sku = product.Sku,
                ShowManufacturerPartNumber = _catalogSettings.ShowManufacturerPartNumber,
                FreeShippingNotificationEnabled = _catalogSettings.ShowFreeShippingNotification,
                ManufacturerPartNumber = product.ManufacturerPartNumber,
                ShowGtin = _catalogSettings.ShowGtin,
                Gtin = product.Gtin,
                ManageInventoryMethod = product.ManageInventoryMethod,
                StockAvailability = await _productService.FormatStockMessageAsync(product, string.Empty),
                HasSampleDownload = product.IsDownload && product.HasSampleDownload,
                DisplayDiscontinuedMessage = !product.Published && _catalogSettings.DisplayDiscontinuedMessageForUnpublishedProducts,
                AvailableEndDate = product.AvailableEndDateTimeUtc,
                VisibleIndividually = product.VisibleIndividually,
                AllowAddingOnlyExistingAttributeCombinations = product.AllowAddingOnlyExistingAttributeCombinations
            };

            //automatically generate product description?
            if (_seoSettings.GenerateProductMetaDescription && string.IsNullOrEmpty(model.MetaDescription))
            {
                //based on short description
                model.MetaDescription = model.ShortDescription;
            }

            //shipping info
            model.IsShipEnabled = product.IsShipEnabled;
            if (product.IsShipEnabled)
            {
                model.IsFreeShipping = product.IsFreeShipping;
                //delivery date
                var deliveryDate = await _dateRangeService.GetDeliveryDateByIdAsync(product.DeliveryDateId);
                if (deliveryDate != null)
                {
                    model.DeliveryDate = await _localizationService.GetLocalizedAsync(deliveryDate, dd => dd.Name);
                }
            }

            var store = await _storeContext.GetCurrentStoreAsync();
            //email a friend
            model.EmailAFriendEnabled = _catalogSettings.EmailAFriendEnabled;
            //compare products
            model.CompareProductsEnabled = _catalogSettings.CompareProductsEnabled;
            //store name
            model.CurrentStoreName = await _localizationService.GetLocalizedAsync(store, x => x.Name);

            //vendor details
            if (_vendorSettings.ShowVendorOnProductDetailsPage)
            {
                var vendor = await _vendorService.GetVendorByIdAsync(product.VendorId);
                if (vendor != null && !vendor.Deleted && vendor.Active)
                {
                    model.ShowVendor = true;

                    model.VendorModel = new VendorBriefInfoModel
                    {
                        Id = vendor.Id,
                        Name = await _localizationService.GetLocalizedAsync(vendor, x => x.Name),
                        SeName = await _urlRecordService.GetSeNameAsync(vendor),
                    };
                }
            }

            //page sharing
            if (_catalogSettings.ShowShareButton && !string.IsNullOrEmpty(_catalogSettings.PageShareCode))
            {
                var shareCode = _catalogSettings.PageShareCode;
                if (_webHelper.IsCurrentConnectionSecured())
                {
                    //need to change the add this link to be https linked when the page is, so that the page doesn't ask about mixed mode when viewed in https...
                    shareCode = shareCode.Replace("http://", "https://");
                }

                model.PageShareCode = shareCode;
            }

            switch (product.ManageInventoryMethod)
            {
                case ManageInventoryMethod.DontManageStock:
                    model.InStock = true;
                    break;

                case ManageInventoryMethod.ManageStock:
                    model.InStock = product.BackorderMode != BackorderMode.NoBackorders
                        || await _productService.GetTotalStockQuantityAsync(product) > 0;
                    model.DisplayBackInStockSubscription = !model.InStock && product.AllowBackInStockSubscriptions;
                    break;

                case ManageInventoryMethod.ManageStockByAttributes:
                    model.InStock = (await _productAttributeService
                        .GetAllProductAttributeCombinationsAsync(product.Id))
                        ?.Any(c => c.StockQuantity > 0 || c.AllowOutOfStockOrders)
                        ?? false;
                    break;
            }

            //breadcrumb
            //do not prepare this model for the associated products. anyway it's not used
            if (_catalogSettings.CategoryBreadcrumbEnabled && !isAssociatedProduct)
            {
                model.Breadcrumb = await PrepareProductBreadcrumbModelAsync(product);
            }

            //product tags
            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                model.ProductTags = await PrepareProductTagModelsAsync(product);
            }

            //pictures
            model.DefaultPictureZoomEnabled = _mediaSettings.DefaultPictureZoomEnabled;
            IList<PictureModel> allPictureModels;
            (model.DefaultPictureModel, allPictureModels) = await PrepareProductDetailsPictureModelAsync(product, isAssociatedProduct);
            model.PictureModels = allPictureModels;

            //price
            model.ProductPrice = await PrepareProductPriceModelAsync(product);

            //'Add to cart' model
            model.AddToCart = await PrepareProductAddToCartModelAsync(product, updatecartitem);
            var customer = await _workContext.GetCurrentCustomerAsync();
            //gift card
            if (product.IsGiftCard)
            {
                model.GiftCard.IsGiftCard = true;
                model.GiftCard.GiftCardType = product.GiftCardType;

                if (updatecartitem == null)
                {
                    model.GiftCard.SenderName = await _customerService.GetCustomerFullNameAsync(customer);
                    model.GiftCard.SenderEmail = customer.Email;
                }
                else
                {
                    _productAttributeParser.GetGiftCardAttribute(updatecartitem.AttributesXml,
                        out var giftCardRecipientName, out var giftCardRecipientEmail,
                        out var giftCardSenderName, out var giftCardSenderEmail, out var giftCardMessage);

                    model.GiftCard.RecipientName = giftCardRecipientName;
                    model.GiftCard.RecipientEmail = giftCardRecipientEmail;
                    model.GiftCard.SenderName = giftCardSenderName;
                    model.GiftCard.SenderEmail = giftCardSenderEmail;
                    model.GiftCard.Message = giftCardMessage;
                }
            }

            //product attributes
            model.ProductAttributes = await PrepareProductAttributeModelsAsync(product, updatecartitem);

            //product specifications
            //do not prepare this model for the associated products. anyway it's not used
            if (!isAssociatedProduct)
            {
                model.ProductSpecificationModel = await PrepareProductSpecificationModelAsync(product);
            }

            //product review overview
            model.ProductReviewOverview = await PrepareProductReviewOverviewModelAsync(product);

            //tier prices
            if (product.HasTierPrices && await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                model.TierPrices = await PrepareProductTierPriceModelsAsync(product);
            }

            //manufacturers
            model.ProductManufacturers = await PrepareProductManufacturerModelsAsync(product);

            //rental products
            if (product.IsRental)
            {
                model.IsRental = true;
                //set already entered dates attributes (if we're going to update the existing shopping cart item)
                if (updatecartitem != null)
                {
                    model.RentalStartDate = updatecartitem.RentalStartDateUtc;
                    model.RentalEndDate = updatecartitem.RentalEndDateUtc;
                }
            }

            //estimate shipping
            if (_shippingSettings.EstimateShippingProductPageEnabled && !model.IsFreeShipping)
            {
                var wrappedProduct = new ShoppingCartItem
                {
                    StoreId = store.Id,
                    ShoppingCartTypeId = (int)ShoppingCartType.ShoppingCart,
                    CustomerId = customer.Id,
                    ProductId = product.Id,
                    CreatedOnUtc = DateTime.UtcNow
                };

                var estimateShippingModel = await _shoppingCartModelFactory.PrepareEstimateShippingModelAsync(new[] { wrappedProduct });

                model.ProductEstimateShipping.ProductId = product.Id;
                model.ProductEstimateShipping.RequestDelay = estimateShippingModel.RequestDelay;
                model.ProductEstimateShipping.Enabled = estimateShippingModel.Enabled;
                model.ProductEstimateShipping.CountryId = estimateShippingModel.CountryId;
                model.ProductEstimateShipping.StateProvinceId = estimateShippingModel.StateProvinceId;
                model.ProductEstimateShipping.ZipPostalCode = estimateShippingModel.ZipPostalCode;
                model.ProductEstimateShipping.UseCity = estimateShippingModel.UseCity;
                model.ProductEstimateShipping.City = estimateShippingModel.City;
                model.ProductEstimateShipping.AvailableCountries = estimateShippingModel.AvailableCountries;
                model.ProductEstimateShipping.AvailableStates = estimateShippingModel.AvailableStates;
            }

            //associated products
            if (product.ProductType == ProductType.GroupedProduct)
            {
                //ensure no circular references
                if (!isAssociatedProduct)
                {
                    var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id, store.Id);
                    foreach (var associatedProduct in associatedProducts)
                        model.AssociatedProducts.Add(await PrepareProductDetailsModelAsync(associatedProduct, null, true));
                }
                model.InStock = model.AssociatedProducts.Any(associatedProduct => associatedProduct.InStock);
            }

            return model;
        }

        /// <summary>
        /// Prepare the product overview price model
        /// </summary>
        /// <param name="product">Product</param>
        /// <param name="forceRedirectionAfterAddingToCart">Whether to force redirection after adding to cart</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product overview price model
        /// </returns>
        protected override async Task<ProductOverviewModel.ProductPriceModel> PrepareProductOverviewPriceModelAsync(Product product, bool forceRedirectionAfterAddingToCart = false)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var priceModel = new ProductOverviewModel.ProductPriceModel
            {
                ForceRedirectionAfterAddingToCart = forceRedirectionAfterAddingToCart
            };

            switch (product.ProductType)
            {
                case ProductType.GroupedProduct:
                    //grouped product
                    await PrepareGroupedProductOverviewPriceModelAsync(product, priceModel);

                    break;
                case ProductType.SimpleProduct:
                default:
                    //simple product
                    await PrepareSimpleProductOverviewPriceModelAsync(product, priceModel);

                    break;
            }

            return priceModel;
        }

        /// <returns>A task that represents the asynchronous operation</returns>
        protected override async Task PrepareSimpleProductOverviewPriceModelAsync(Product product, ProductOverviewModel.ProductPriceModel priceModel)
        {
            //add to cart button
            priceModel.DisableBuyButton = product.DisableBuyButton ||
                                          !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart) ||
                                          !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

            //add to wishlist button
            priceModel.DisableWishlistButton = product.DisableWishlistButton ||
                                               !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist) ||
                                               !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);
            //compare products
            priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

            //rental
            priceModel.IsRental = product.IsRental;

            //pre-order
            if (product.AvailableForPreOrder)
            {
                priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                                  product.PreOrderAvailabilityStartDateTimeUtc.Value >=
                                                  DateTime.UtcNow;
                priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
            }

            //prices
            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                if (product.CustomerEntersPrice)
                    return;

                if (product.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                     _workContext.OriginalCustomerIfImpersonated == null))
                {
                    //call for price
                    priceModel.OldPrice = null;
                    priceModel.OldPriceValue = null;
                    priceModel.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
                    priceModel.PriceValue = null;
                }
                else
                {
                    //prices
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    var (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer);

                    if (product.HasTierPrices)
                    {
                        var (tierPriceMinPossiblePriceWithoutDiscount, tierPriceMinPossiblePriceWithDiscount, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, quantity: int.MaxValue);

                        //calculate price for the maximum quantity if we have tier prices, and choose minimal
                        minPossiblePriceWithoutDiscount = Math.Min(minPossiblePriceWithoutDiscount, tierPriceMinPossiblePriceWithoutDiscount);
                        minPossiblePriceWithDiscount = Math.Min(minPossiblePriceWithDiscount, tierPriceMinPossiblePriceWithDiscount);
                    }

                    var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
                    var (finalPriceWithoutDiscountBase, _) = await _taxService.GetProductPriceAsync(product, minPossiblePriceWithoutDiscount);
                    var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, minPossiblePriceWithDiscount);
                    var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                    var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, currentCurrency);
                    var finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithoutDiscountBase, currentCurrency);
                    var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, currentCurrency);

                    //we will try to create a price history log here if it doesn't exist
                    var productPriceLogs = await _productPriceHistoryService.GetProductPriceLogsAsyncByProduct(product);
                    if (productPriceLogs.Count == 0 || !productPriceLogs.Any(b => b.Price == priceModel.PriceValue))
                    {
                        var productPriceLog = new ProductPriceLog
                        {
                            CreatedOnUtc = DateTime.UtcNow,
                            IsDiscountedPrice = false,
                            Price = finalPriceWithoutDiscount,
                            ProductId = product.Id,
                            Sku = product.Sku,
                        };

                        await _productPriceHistoryService.InsertProductPriceLogAsync(productPriceLog);
                    }
                    else
                    {
                        var productPriceLog = productPriceLogs.First(a => a.Price == priceModel.PriceValue);
                        productPriceLog.UpdatedOnUtc = DateTime.UtcNow;

                        await _productPriceHistoryService.UpdateProductPriceLogAsync(productPriceLog);
                    }

                    if (finalPriceWithDiscount != finalPriceWithoutDiscount)
                    {
                        var discountedProductPriceLogs = await _productPriceHistoryService.GetProductDiscountedPriceLogsByProductAsync(product);
                        if (discountedProductPriceLogs.Count == 0 || !discountedProductPriceLogs.Any(b => b.Price == finalPriceWithDiscount))
                        {
                            var discountedProductPriceLog = new ProductPriceLog
                            {
                                CreatedOnUtc = DateTime.UtcNow,
                                IsDiscountedPrice = true,
                                Price = finalPriceWithDiscount,
                                ProductId = product.Id,
                                Sku = product.Sku,
                            };

                            await _productPriceHistoryService.InsertProductPriceLogAsync(discountedProductPriceLog);
                        }
                        else
                        {
                            var discountedProductPriceLog = discountedProductPriceLogs.First(a => a.Price == finalPriceWithDiscount);
                            discountedProductPriceLog.UpdatedOnUtc = DateTime.UtcNow;

                            await _productPriceHistoryService.UpdateProductPriceLogAsync(discountedProductPriceLog);
                        }
                    }

                    //do we have tier prices configured?
                    var tierPrices = new List<TierPrice>();
                    if (product.HasTierPrices)
                    {
                        var store = await _storeContext.GetCurrentStoreAsync();
                        tierPrices.AddRange(await _productService.GetTierPricesAsync(product, customer, store.Id));
                    }
                    //When there is just one tier price (with  qty 1), there are no actual savings in the list.
                    var displayFromMessage = tierPrices.Any() && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);
                    if (displayFromMessage)
                    {
                        priceModel.OldPrice = null;
                        priceModel.OldPriceValue = null;
                        priceModel.Price = string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"), await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount));
                        priceModel.PriceValue = finalPriceWithDiscount;
                    }
                    else
                    {
                        var strikeThroughPrice = decimal.Zero;

                        if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                            strikeThroughPrice = oldPrice;

                        if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                            strikeThroughPrice = finalPriceWithoutDiscount;

                        if (strikeThroughPrice > decimal.Zero)
                        {
                            priceModel.OldPrice = await _priceFormatter.FormatPriceAsync(strikeThroughPrice);
                            priceModel.OldPriceValue = strikeThroughPrice;
                        }

                        priceModel.Price = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                        priceModel.PriceValue = finalPriceWithDiscount;
                    }

                    if (product.IsRental)
                    {
                        //rental product
                        priceModel.OldPrice = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceModel.OldPrice);
                        priceModel.OldPriceValue = priceModel.OldPriceValue;
                        priceModel.Price = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceModel.Price);
                        priceModel.PriceValue = priceModel.PriceValue;
                    }

                    //property for German market
                    //we display tax/shipping info only with "shipping enabled" for this product
                    //we also ensure this it's not free shipping
                    priceModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductBoxes && product.IsShipEnabled && !product.IsFreeShipping;

                    //PAngV default baseprice (used in Germany)
                    priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscount);
                    priceModel.BasePricePAngVValue = finalPriceWithDiscount;
                }
            }
            else
            {
                //hide prices
                priceModel.OldPrice = null;
                priceModel.OldPriceValue = null;
                priceModel.Price = null;
                priceModel.PriceValue = null;
            }
        }

        /// <summary>
        /// Prepare the product price model
        /// </summary>
        /// <param name="product">Product</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the product price model
        /// </returns>
        protected override async Task<ProductDetailsModel.ProductPriceModel> PrepareProductPriceModelAsync(Product product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var model = new ProductDetailsModel.ProductPriceModel
            {
                ProductId = product.Id
            };

            if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
            {
                model.HidePrices = false;
                if (product.CustomerEntersPrice)
                {
                    model.CustomerEntersPrice = true;
                }
                else
                {
                    if (product.CallForPrice &&
                        //also check whether the current user is impersonated
                        (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                    {
                        model.CallForPrice = true;
                    }
                    else
                    {
                        var customer = await _workContext.GetCurrentCustomerAsync();
                        var (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, product.OldPrice);
                        var (finalPriceWithoutDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product, customer, includeDiscounts: false)).finalPrice);
                        var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product, customer)).finalPrice);
                        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                        var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, currentCurrency);
                        var finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithoutDiscountBase, currentCurrency);
                        var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, currentCurrency);

                        if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                        {
                            model.OldPrice = await _priceFormatter.FormatPriceAsync(oldPrice);
                            model.OldPriceValue = oldPrice;
                        }

                        model.Price = await _priceFormatter.FormatPriceAsync(finalPriceWithoutDiscount);

                        if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                        {
                            model.PriceWithDiscount = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                            model.PriceWithDiscountValue = finalPriceWithDiscount;
                        }

                        model.PriceValue = finalPriceWithDiscount;

                        //property for German market
                        //we display tax/shipping info only with "shipping enabled" for this product
                        //we also ensure this it's not free shipping
                        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                            && product.IsShipEnabled &&
                            !product.IsFreeShipping;

                        //PAngV baseprice (used in Germany)
                        model.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscountBase);
                        model.BasePricePAngVValue = finalPriceWithDiscountBase;
                        //currency code
                        model.CurrencyCode = (await _workContext.GetWorkingCurrencyAsync()).CurrencyCode;

                        //rental
                        if (product.IsRental)
                        {
                            model.IsRental = true;
                            var priceStr = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                            model.RentalPrice = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceStr);
                            model.RentalPriceValue = finalPriceWithDiscount;
                        }
                    }
                }
            }
            else
            {
                model.HidePrices = true;
                model.OldPrice = null;
                model.OldPriceValue = null;
                model.Price = null;
            }

            //we will try to create a price history log here if it doesn't exist
            var productPriceLogs = await _productPriceHistoryService.GetProductPriceLogsAsyncByProduct(product);
            if(productPriceLogs.Count == 0 || !productPriceLogs.Any(b => b.Price == model.PriceValue))
            {
                var productPriceLog = new ProductPriceLog
                {
                    CreatedOnUtc = DateTime.UtcNow,
                    IsDiscountedPrice = false,
                    Price = model.PriceValue,
                    ProductId = product.Id,
                    Sku = product.Sku,
                };

                await _productPriceHistoryService.InsertProductPriceLogAsync(productPriceLog);
            }
            else
            {
                var productPriceLog = productPriceLogs.First(a => a.Price == model.PriceValue);
                productPriceLog.UpdatedOnUtc = DateTime.UtcNow;

                await _productPriceHistoryService.UpdateProductPriceLogAsync(productPriceLog);
            }

            if(model.PriceWithDiscountValue.HasValue && model.PriceWithDiscountValue != model.PriceValue)
            {
                var discountedProductPriceLogs = await _productPriceHistoryService.GetProductDiscountedPriceLogsByProductAsync(product);
                if (discountedProductPriceLogs.Count == 0 || discountedProductPriceLogs.Any(b => b.Price == model.PriceWithDiscountValue.Value))
                {
                    var discountedProductPriceLog = new ProductPriceLog
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        IsDiscountedPrice = true,
                        Price = model.PriceWithDiscountValue.Value,
                        ProductId = product.Id,
                        Sku = product.Sku,
                    };

                    await _productPriceHistoryService.InsertProductPriceLogAsync(discountedProductPriceLog);
                }
                else
                {
                    var discountedProductPriceLog = discountedProductPriceLogs.First(a => a.Price == model.PriceWithDiscountValue.Value);
                    discountedProductPriceLog.UpdatedOnUtc = DateTime.UtcNow;

                    await _productPriceHistoryService.UpdateProductPriceLogAsync(discountedProductPriceLog);
                }
                
            }

            return model;
        }


    }
}
