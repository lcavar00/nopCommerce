using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Data;
using Nop.Plugin.Misc.LowestPrice.Domain;
using Nop.Services.Catalog;
using Nop.Services.Discounts;
using Nop.Services.Logging;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.LowestPrice.Services
{
    public partial class LowestPriceService : ILowestPriceService
    {

        #region Fields

        private readonly IDiscountLogService _discountLogService;
        private readonly IDiscountService _discountService;
        private readonly ILogger _logger;
        private readonly IProductAttributeCombinationPriceHistoryService _productAttributeCombinationPriceHistoryService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IProductPriceHistoryService _productPriceHistoryService;
        private readonly IProductService _productService;
        private readonly IRepository<Discount> _discountRepository;
        private readonly IRepository<DiscountProductMapping> _discountProductMappingRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public LowestPriceService(IDiscountLogService discountLogService,
            IDiscountService discountService,
            ILogger logger,
            IProductAttributeCombinationPriceHistoryService productAttributeCombinationPriceHistoryService,
            IProductAttributeService productAttributeService,
            IProductPriceHistoryService productPriceHistoryService,
            IProductService productService,
            IRepository<Discount> discountRepository,
            IRepository<DiscountProductMapping> discountProductMappingRepository,
            IRepository<Product> productRepository,
            IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext)
        {
            _discountLogService = discountLogService;
            _discountService = discountService;
            _logger = logger;
            _productAttributeCombinationPriceHistoryService = productAttributeCombinationPriceHistoryService;
            _productAttributeService = productAttributeService;
            _productPriceHistoryService = productPriceHistoryService;
            _productService = productService;
            _discountRepository = discountRepository;
            _discountProductMappingRepository = discountProductMappingRepository;
            _productRepository = productRepository;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
        }

        #endregion

        public async Task<decimal> GetLowestPriceAsync(string sku)
        {
            var productAttributeCombination = await _productAttributeService.GetProductAttributeCombinationBySkuAsync(sku);
            var product = await _productService.GetProductBySkuAsync(sku)
                ?? await _productService.GetProductByIdAsync(productAttributeCombination.ProductId);

            var customer = await _workContext.GetCurrentCustomerAsync();

            var (price, _, _) = await _shoppingCartService.GetUnitPriceAsync(product, customer, ShoppingCartType.ShoppingCart, 1, productAttributeCombination?.AttributesXml ?? string.Empty, 0, null, null, false);

            if (product != null)
            {
                var productPriceLogs = await _productPriceHistoryService.GetProductPriceLogsAsyncByProduct(product);
                if (productPriceLogs.Count == 0)
                {
                    return price;
                }

                price = productPriceLogs.Min(a => a.Price);
            }
            else if (productAttributeCombination != null)
            {
                var productAttributeCombinationPriceLogs = await _productAttributeCombinationPriceHistoryService.GetProductAttributeCombinationPriceLogsAsync(productAttributeCombination);
                if (productAttributeCombinationPriceLogs.Count == 0)
                {
                    return price;
                }

                price = productAttributeCombinationPriceLogs.Min(a => a.Price);
            }
            else
            {
                throw new Exception($"No product or product attribute combination with SKU: {sku}");
            }

            return price;
        }

        public async Task<decimal?> GetLowestDiscountedPriceAsync(string sku)
        {
            var productAttributeCombination = await _productAttributeService.GetProductAttributeCombinationBySkuAsync(sku);
            var product = await _productService.GetProductBySkuAsync(sku)
                ?? await _productService.GetProductByIdAsync(productAttributeCombination.ProductId);

            var customer = await _workContext.GetCurrentCustomerAsync();

            var (_, _, appliedDiscounts) = await _shoppingCartService.GetUnitPriceAsync(product, customer, ShoppingCartType.ShoppingCart, 1, productAttributeCombination?.AttributesXml ?? string.Empty, 0, null, null, true);

            var consideredDiscounts = await _discountRepository.Table.Where(a => a.EndDateUtc < DateTime.UtcNow || !appliedDiscounts.Any(b => b.Id == a.Id)).ToListAsync();

            decimal? discountedPrice = null;
            if (product != null)
            {
                var productDiscountedPriceLogs = await _productPriceHistoryService.GetProductDiscountedPriceLogsByProductAsync(product);
                if (productDiscountedPriceLogs.Count == 0 || !consideredDiscounts.Any())
                {
                    return discountedPrice;
                }

                var discountLogs = await _discountLogService.GetDiscountLogsByDiscountIdsAsync(consideredDiscounts.Select(a => a.Id), "ProductPriceLog");
                var priceLogDiscountLogs = await _discountLogService.GetPriceLogDiscountLogsByDiscountLogIdsAsync(discountLogs.Select(a => a.Id).Distinct());

                if (!priceLogDiscountLogs.Any())
                    return null;

                productDiscountedPriceLogs = productDiscountedPriceLogs.Where(a => priceLogDiscountLogs.Any(b => b.EntityId == a.Id)).ToList();

                discountedPrice = productDiscountedPriceLogs.Any() ? productDiscountedPriceLogs.Min(a => a.Price) : discountedPrice;
            }
            else if (productAttributeCombination != null)
            {
                var productAttributeCombinationDiscountedPriceLogs = await _productAttributeCombinationPriceHistoryService.GetProductAttributeCombinationDiscountedPriceLogsAsync(productAttributeCombination);
                if (productAttributeCombinationDiscountedPriceLogs.Count == 0 || !consideredDiscounts.Any())
                {
                    return discountedPrice;
                }

                var discountLogs = await _discountLogService.GetDiscountLogsByDiscountIdsAsync(consideredDiscounts.Select(a => a.Id), "ProductAttributeCombinationPriceLog");
                var priceLogDiscountLogs = await _discountLogService.GetPriceLogDiscountLogsByDiscountLogIdsAsync(discountLogs.Select(a => a.Id).Distinct());

                if (!priceLogDiscountLogs.Any())
                    return null;

                discountedPrice = productAttributeCombinationDiscountedPriceLogs.Any() ?
                    productAttributeCombinationDiscountedPriceLogs.Min(a => a.Price)
                    : discountedPrice;
            }
            else
            {
                throw new Exception($"No product or product attribute combination with SKU: {sku}");
            }

            return discountedPrice;
        }

        public async Task<decimal> GetLowestPriceAsync(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            var customer = await _workContext.GetCurrentCustomerAsync();

            var (price, _, _) = await _shoppingCartService.GetUnitPriceAsync(product, customer, ShoppingCartType.ShoppingCart, 1, string.Empty, 0, null, null, false);

            if (product != null)
            {
                var productPriceLogs = await _productPriceHistoryService.GetProductPriceLogsAsyncByProduct(product);
                if (productPriceLogs.Count == 0)
                {
                    return price;
                }

                price = productPriceLogs.Min(a => a.Price);
            }

            return price;
        }

        public async Task<decimal?> GetLowestDiscountPriceAsync(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            var customer = await _workContext.GetCurrentCustomerAsync();

            var (_, _, appliedDiscounts) = await _shoppingCartService.GetUnitPriceAsync(product, customer, ShoppingCartType.ShoppingCart, 1, string.Empty, 0, null, null, false);

            var consideredDiscounts = await _discountRepository.Table.Where(a => a.EndDateUtc < DateTime.UtcNow || !appliedDiscounts.Any(b => b.Id == a.Id)).ToListAsync();

            decimal? price = null;
            if (product != null)
            {
                var productDiscountedPriceLogs = await _productPriceHistoryService.GetProductDiscountedPriceLogsByProductAsync(product);
                if (productDiscountedPriceLogs.Count == 0)
                {
                    return price;
                }

                var discountLogs = await _discountLogService.GetDiscountLogsByDiscountIdsAsync(consideredDiscounts.Select(a => a.Id), "ProductPriceLog");
                var priceLogDiscountLogs = await _discountLogService.GetPriceLogDiscountLogsByDiscountLogIdsAsync(discountLogs.Select(a => a.Id).Distinct());

                if (!priceLogDiscountLogs.Any())
                    return null;

                productDiscountedPriceLogs = productDiscountedPriceLogs.Where(a => priceLogDiscountLogs.Any(b => b.EntityId == a.Id)).ToList();

                price = productDiscountedPriceLogs.Any() ? productDiscountedPriceLogs.Min(a => a.Price) : price;
            }

            return price;
        }

        public async Task LogPricesAndDiscountsAsync()
        {
            var allProducts = await _productRepository.Table.Where(a => !a.Deleted).ToListAsync();
            var allPriceLogs = await _productPriceHistoryService.GetAllProductPriceLogsAsync();

            await LogProductPricesAsync(allProducts, allPriceLogs);

            var allProductAttributeCombinations = await _productAttributeCombinationRepository.Table.Where(a => allProducts.Any(b => b.Id == a.ProductId)).ToListAsync();
            var allProductAttributeCombinationPriceLogs = await _productAttributeCombinationPriceHistoryService.GetAllProductAttributeCombintaionPriceLogsAsync();

            await LogProductAttributeCombinationPricesAsync(allProducts, allProductAttributeCombinations, allProductAttributeCombinationPriceLogs);
        }

        public async Task LogProductPricesAsync(IList<Product> allProducts,
            IList<ProductPriceLog> allPriceLogs)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();

            var insertProductPriceLogs = new List<ProductPriceLog>();
            var updateProductPriceLogs = new List<ProductPriceLog>();
            var insertDiscountLogs = new List<DiscountLog>();
            var discountPriceLogDictionary = new Dictionary<ProductPriceLog, DiscountLog>();
            foreach (var product in allProducts)
            {
                var (discountedPrice, discountAmount, appliedDiscounts) = await _shoppingCartService.GetUnitPriceAsync(product, customer, ShoppingCartType.ShoppingCart, 1, string.Empty, 0, null, null, false);
                var unitPrice = discountedPrice + discountAmount;

                var productPriceLog = allPriceLogs.FirstOrDefault(a => !a.IsDiscountedPrice && a.Price == unitPrice && a.ProductId == product.Id)
                    ?? insertProductPriceLogs.FirstOrDefault(a => !a.IsDiscountedPrice && a.ProductId == product.Id && a.Price == unitPrice);
                if (productPriceLog != null)
                {
                    productPriceLog.UpdatedOnUtc = DateTime.UtcNow;
                    updateProductPriceLogs.Add(productPriceLog);
                }
                else
                {
                    productPriceLog = new ProductPriceLog
                    {
                        Price = unitPrice,
                        ProductId = product.Id,
                        IsDiscountedPrice = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        Sku = product.Sku,
                    };
                    insertProductPriceLogs.Add(productPriceLog);
                }

                if (appliedDiscounts.Count > 0 && discountAmount != 0)
                {
                    var discountedProductPriceLog = allPriceLogs.FirstOrDefault(a => a.IsDiscountedPrice && a.ProductId == product.Id && a.Price == discountedPrice)
                        ?? insertProductPriceLogs.FirstOrDefault(a => a.IsDiscountedPrice && a.ProductId == product.Id && a.Price == discountedPrice);
                    if (discountedProductPriceLog != null)
                    {
                        discountedProductPriceLog.UpdatedOnUtc = DateTime.UtcNow;
                        updateProductPriceLogs.Add(discountedProductPriceLog);
                    }
                    else
                    {
                        discountedProductPriceLog = new ProductPriceLog
                        {
                            Price = discountedPrice,
                            ProductId = product.Id,
                            IsDiscountedPrice = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            Sku = product.Sku,
                        };
                        insertProductPriceLogs.Add(discountedProductPriceLog);

                        foreach (var discount in appliedDiscounts)
                        {
                            var discountLog = new DiscountLog
                            {
                                DiscountAmount = discount.DiscountAmount,
                                DiscountId = discount.Id,
                                DiscountPercentage = discount.DiscountPercentage,
                                DiscountType = discount.DiscountType,
                                DiscountTypeId = discount.DiscountTypeId,
                                EndDateUtc = discount.EndDateUtc,
                                MaximumDiscountAmount = discount.MaximumDiscountAmount,
                                Name = discount.Name,
                                StartDateUtc = discount.StartDateUtc,
                                UsePercentage = discount.UsePercentage,
                                CreatedOnUtc = DateTime.UtcNow
                            };

                            insertDiscountLogs.Add(discountLog);
                            discountPriceLogDictionary.Add(discountedProductPriceLog, discountLog);
                        }
                    }
                }
            }

            await _productPriceHistoryService.InsertProductPriceLogsAsync(insertProductPriceLogs);
            await _productPriceHistoryService.UpdateProductPriceLogsAsync(updateProductPriceLogs);

            await _discountLogService.InsertDiscountLogsAsync(insertDiscountLogs);

            var insertPriceLogDiscountLogs = new List<PriceLogDiscountLog>();
            foreach (var kvp in discountPriceLogDictionary)
            {
                var priceLog = kvp.Key;
                var discountLog = kvp.Value;

                var priceLogDiscountLogMapping = new PriceLogDiscountLog
                {
                    DiscountLogId = discountLog.Id,
                    EntityName = nameof(ProductPriceLog),
                    EntityId = priceLog.Id
                };

                insertPriceLogDiscountLogs.Add(priceLogDiscountLogMapping);
            }
            await _discountLogService.InsertPriceLogDiscountLogsAsync(insertPriceLogDiscountLogs);
        }

        public async Task LogProductAttributeCombinationPricesAsync(IList<Product> allProducts,
            IList<ProductAttributeCombination> allProductAttributeCombinations,
            IList<ProductAttributeCombinationPriceLog> allProductAttributeCombinationPriceLogs)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var updateProductAttributeCombinationPriceLogs = new List<ProductAttributeCombinationPriceLog>();
            var insertProductAttributeCombinationPriceLogs = new List<ProductAttributeCombinationPriceLog>();
            var insertDiscountLogs = new List<DiscountLog>();
            var discountPriceLogDictionary = new Dictionary<ProductAttributeCombinationPriceLog, DiscountLog>();
            foreach (var productAttributeCombination in allProductAttributeCombinations)
            {
                var product = allProducts.First(a => a.Id == productAttributeCombination.ProductId);
                if (product == null)
                {
                    await _logger.InsertLogAsync(LogLevel.Error, $"Product (Id: {productAttributeCombination.ProductId} does not exist for product attribute combination (Id: {productAttributeCombination.Id})");
                    continue;
                }

                var (discountedPrice, discountAmount, appliedDiscounts) = await _shoppingCartService.GetUnitPriceAsync(product, currentCustomer, ShoppingCartType.ShoppingCart, 1, productAttributeCombination.AttributesXml, 0, null, null, true);

                var unitPrice = discountedPrice + discountAmount;
                var productAttributeCombinationPriceLog = allProductAttributeCombinationPriceLogs
                    .FirstOrDefault(a => !a.IsDiscountedPrice && a.Price == unitPrice && a.ProductAttributeCombinationId == productAttributeCombination.Id);
                if (productAttributeCombinationPriceLog != null)
                {
                    productAttributeCombinationPriceLog.UpdatedOnUtc = DateTime.UtcNow;
                    updateProductAttributeCombinationPriceLogs.Add(productAttributeCombinationPriceLog);
                }
                else
                {
                    productAttributeCombinationPriceLog = new ProductAttributeCombinationPriceLog
                    {
                        Price = unitPrice,
                        ProductAttributeCombinationId = productAttributeCombination.Id,
                        IsDiscountedPrice = false,
                        CreatedOnUtc = DateTime.UtcNow,
                        Sku = productAttributeCombination.Sku,
                    };
                    insertProductAttributeCombinationPriceLogs.Add(productAttributeCombinationPriceLog);
                }

                if (appliedDiscounts.Count > 0 && discountAmount != 0)
                {
                    var productAttributeCombinationDiscountedPriceLog = allProductAttributeCombinationPriceLogs
                        .FirstOrDefault(a => a.IsDiscountedPrice && a.Price == discountedPrice && a.ProductAttributeCombinationId == productAttributeCombination.Id);
                    if (productAttributeCombinationDiscountedPriceLog != null)
                    {
                        productAttributeCombinationDiscountedPriceLog.UpdatedOnUtc = DateTime.UtcNow;
                        updateProductAttributeCombinationPriceLogs.Add(productAttributeCombinationDiscountedPriceLog);
                    }
                    else
                    {
                        productAttributeCombinationDiscountedPriceLog = new ProductAttributeCombinationPriceLog
                        {
                            Price = discountedPrice,
                            ProductAttributeCombinationId = productAttributeCombination.Id,
                            IsDiscountedPrice = true,
                            CreatedOnUtc = DateTime.UtcNow,
                            Sku = productAttributeCombination.Sku,
                        };
                        insertProductAttributeCombinationPriceLogs.Add(productAttributeCombinationDiscountedPriceLog);

                        foreach (var discount in appliedDiscounts)
                        {
                            var discountLog = new DiscountLog
                            {
                                DiscountAmount = discount.DiscountAmount,
                                DiscountId = discount.Id,
                                DiscountPercentage = discount.DiscountPercentage,
                                DiscountType = discount.DiscountType,
                                DiscountTypeId = discount.DiscountTypeId,
                                EndDateUtc = discount.EndDateUtc,
                                MaximumDiscountAmount = discount.MaximumDiscountAmount,
                                Name = discount.Name,
                                StartDateUtc = discount.StartDateUtc,
                                UsePercentage = discount.UsePercentage,
                                CreatedOnUtc = DateTime.UtcNow
                            };

                            insertDiscountLogs.Add(discountLog);
                            discountPriceLogDictionary.Add(productAttributeCombinationDiscountedPriceLog, discountLog);
                        }
                    }
                }
            }

            await _productAttributeCombinationPriceHistoryService.UpdateProductAttributeCombinationPriceLogsAsync(updateProductAttributeCombinationPriceLogs);
            await _productAttributeCombinationPriceHistoryService.InsertProductAttributeCombinationPriceLogsAsync(insertProductAttributeCombinationPriceLogs);

            await _discountLogService.InsertDiscountLogsAsync(insertDiscountLogs);

            var insertPriceLogDiscountLogs = new List<PriceLogDiscountLog>();
            foreach (var kvp in discountPriceLogDictionary)
            {
                var priceLog = kvp.Key;
                var discountLog = kvp.Value;

                var priceLogDiscountLogMapping = new PriceLogDiscountLog
                {
                    DiscountLogId = discountLog.Id,
                    EntityName = nameof(ProductAttributeCombinationPriceLog),
                    EntityId = priceLog.Id
                };

                insertPriceLogDiscountLogs.Add(priceLogDiscountLogMapping);
            }
            await _discountLogService.InsertPriceLogDiscountLogsAsync(insertPriceLogDiscountLogs);
        }
    }
}
