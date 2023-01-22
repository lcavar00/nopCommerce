using Nop.Core.Domain.Catalog;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.MorePrices.Extensions
{
    public class CatalogSettingsExtenion : CatalogSettings
    {
        public CatalogSettingsExtenion()
        {
            PriceSortingEnumDisabled = new List<int>();
            PriceSortingEnumDisplayOrder = new Dictionary<int, int>();
        }

        /// <summary>
        /// Gets or sets a list of disabled values of ProductSortingEnum
        /// </summary>
        public List<int> PriceSortingEnumDisabled { get; set; }

        /// <summary>
        /// Gets or sets a display order of ProductSortingEnum values 
        /// </summary>
        public Dictionary<int, int> PriceSortingEnumDisplayOrder { get; set; }
    }
}
