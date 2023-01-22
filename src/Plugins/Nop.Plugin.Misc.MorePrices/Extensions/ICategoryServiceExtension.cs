using Nop.Plugin.Misc.MorePrices.Domain;
using Nop.Services.Catalog;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.MorePrices.Extensions
{
    public interface ICategoryServiceExtension : ICategoryService
    {
        IEnumerable<PriceCategory> GetPriceCategoriesByPriceId(int id);
    }
}