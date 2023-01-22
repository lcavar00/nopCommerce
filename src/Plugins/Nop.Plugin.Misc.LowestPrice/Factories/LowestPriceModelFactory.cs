using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.LowestPrice.Models;
using Nop.Plugin.Misc.LowestPrice.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.LowestPrice.Factories
{
    public class LowestPriceModelFactory : ILowestPriceModelFactory
    {
        #region Fields

        private readonly ILowestPriceService _lowestPriceService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public LowestPriceModelFactory(ILowestPriceService lowestPriceService, 
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductService productService,
            ISettingService settingService,
            IWorkContext workContext)
        {
            _lowestPriceService = lowestPriceService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _settingService = settingService;
            _workContext = workContext;
        }

        #endregion

        public async Task<LowestPriceModel> PrepareLowestProductPriceModelAsync(int productId)
        {
            var priceHistorySettings = await _settingService.LoadSettingAsync<PriceHistorySettings>();
            var product = await _productService.GetProductByIdAsync(productId);
            var customer = await _workContext.GetCurrentCustomerAsync();
            var (_, _, _, appliedDiscount) = await _priceCalculationService.GetFinalPriceAsync(product, customer, additionalCharge: 0, quantity: 1, includeDiscounts: true);

            var prepareModel = (product.OldPrice != 0 && priceHistorySettings.DisplayForOldPrice)
                || (appliedDiscount.Any() && priceHistorySettings.DisplayForDiscounts);

            if (!prepareModel)
            {
                return null;
            }

            var lowestPrice = await _lowestPriceService.GetLowestPriceAsync(product.Sku);
            var lowestDiscountedPrice = await _lowestPriceService.GetLowestDiscountedPriceAsync(product.Sku);

            var currentCurrency = await _workContext.GetWorkingCurrencyAsync();

            var model = new LowestPriceModel
            {
                ProductId = product.Id,
                PriceValue = lowestPrice,
                DiscountedPriceValue = lowestDiscountedPrice,
                Sku = product.Sku,
                Price = await _priceFormatter.FormatPriceAsync(lowestPrice, true, currentCurrency),
                DiscountedPrice = lowestDiscountedPrice.HasValue ?
                    await _priceFormatter.FormatPriceAsync(lowestDiscountedPrice.Value, true, currentCurrency)
                    : string.Empty,
            };

            return model;
        }
    }
}
