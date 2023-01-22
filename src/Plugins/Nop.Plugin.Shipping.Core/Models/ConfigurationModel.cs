using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nop.Plugin.Shipping.Core.Models
{
    public record ConfigurationModel : BaseSearchModel
    {
        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.LimitMethodsToCreated")]
        public bool LimitMethodsToCreated { get; set; }

        public bool ShippingByTotalByLocationEnabled { get; set; }

        public ConfigurationModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableShippingMethods = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.Store")]
        public int SearchStoreId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.Warehouse")]
        public int SearchWarehouseId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.Country")]
        public int SearchCountryId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.StateProvince")]
        public int SearchStateProvinceId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.Zip")]
        public string SearchZip { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.ShippingMethod")]
        public int SearchShippingMethodId { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableShippingMethods { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }
    }
}
