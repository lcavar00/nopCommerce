using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;

namespace Nop.Plugin.Misc.MorePrices.Extensions
{
    public class PriceFormatterExtension : PriceFormatter
    {
        public PriceFormatterExtension(CurrencySettings currencySettings,
            ICurrencyService currencyService, 
            ILocalizationService localizationService, 
            IMeasureService measureService, 
            IWorkContext workContext, 
            TaxSettings taxSettings) : base(currencySettings, currencyService, localizationService, measureService, workContext, taxSettings)
        {

        }
    }
}
