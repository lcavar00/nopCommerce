using Nop.Plugin.Misc.MorePrices.Areas.Admin.Models;
using Nop.Plugin.Misc.MorePrices.Domain;

namespace Nop.Plugin.Misc.MorePrices.Areas.Admin.Factories
{
    public interface IPriceModelFactory
    {
        /// <summary>
        /// Prepare price search model
        /// </summary>
        /// <param name="searchModel">Price search model</param>
        /// <returns>Price search model</returns>
        PriceSearchModel PreparePriceSearchModel(PriceSearchModel searchModel);

        /// <summary>
        /// Prepare paged product list model
        /// </summary>
        /// <param name="searchModel">Product search model</param>
        /// <returns>Product list model</returns>
        PriceListModel PreparePriceListModel(PriceSearchModel searchModel);

        /// <summary>
        /// Prepare price model
        /// </summary>
        /// <param name="model">price model</param>
        /// <param name="product">Price</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Price model</returns>
        PriceModel PreparePriceModel(PriceModel model, Price product, bool excludeProperties = false);
    }
}