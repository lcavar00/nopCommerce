using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.LowestPrice.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;

namespace Nop.Plugin.Misc.LowestPrice.Controllers
{
    public partial class LowestPriceController : BasePluginController
    {
        #region Fields

        private readonly ILowestPriceService _lowestPriceService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductAttributeParser _productAttributeParser;
        private readonly IProductService _productService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public LowestPriceController(ILowestPriceService lowestPriceService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            ISettingService settingService,
            IWorkContext workContext)
        {
            _lowestPriceService = lowestPriceService;
            _priceCalculationService = priceCalculationService;
            _priceFormatter = priceFormatter;
            _productAttributeParser = productAttributeParser;
            _productService = productService;
            _settingService = settingService;
            _workContext = workContext;
        }

        #endregion

        //handle product attribute selection event. this way we return new price, overridden gtin/sku/mpn
        //currently we use this method on the product details pages
        [HttpPost]
        public virtual async Task<IActionResult> ProductDetails_AttributeChange(int productId, IFormCollection form)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
                return new NullJsonResult();

            var priceHistorySettings = await _settingService.LoadSettingAsync<PriceHistorySettings>();
            var customer = await _workContext.GetCurrentCustomerAsync();
            var (_, _, _, appliedDiscount) = await _priceCalculationService.GetFinalPriceAsync(product, customer, additionalCharge: 0, quantity: 1, includeDiscounts: true);

            var prepareModel = (product.OldPrice != 0 && priceHistorySettings.DisplayForOldPrice)
                || (appliedDiscount.Any() && priceHistorySettings.DisplayForDiscounts);

            if (!prepareModel)
            {
                return null;
            }

            var errors = new List<string>();
            var attributeXml = await _productAttributeParser.ParseProductAttributesAsync(product, form, errors);

            //sku
            var sku = await _productService.FormatSkuAsync(product, attributeXml);

            var priceValue = await _lowestPriceService.GetLowestPriceAsync(sku);
            var priceDiscountedValue = await _lowestPriceService.GetLowestDiscountedPriceAsync(sku);

            return Json(new
            {
                price = await _priceFormatter.FormatPriceAsync(priceValue),
                priceValue = priceValue,
                discountedPrice = priceDiscountedValue.HasValue ?
                    await _priceFormatter.FormatPriceAsync(priceDiscountedValue.Value)
                    : string.Empty,
                discountedPriceValue = priceDiscountedValue,
            });
        }
    }
}
