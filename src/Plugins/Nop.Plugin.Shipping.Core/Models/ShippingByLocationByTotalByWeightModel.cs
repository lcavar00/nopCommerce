using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.Core.Models
{
    public record ShippingByLocationByTotalByWeightModel : BaseNopEntityModel
    {
        public ShippingByLocationByTotalByWeightModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailableShippingMethods = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.Store")]
        public int StoreId { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.Store")]
        public string StoreName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.Warehouse")]
        public int WarehouseId { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.Warehouse")]
        public string WarehouseName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.Country")]
        public int CountryId { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.Country")]
        public string CountryName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.StateProvince")]
        public int StateProvinceId { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.StateProvince")]
        public string StateProvinceName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.Zip")]
        public string Zip { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.ShippingMethod")]
        public int ShippingMethodId { get; set; }
        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.ShippingMethod")]
        public string ShippingMethodName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.WeightFrom")]
        public decimal WeightFrom { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.WeightTo")]
        public decimal WeightTo { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.OrderSubtotalFrom")]
        public decimal OrderSubtotalFrom { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.OrderSubtotalTo")]
        public decimal OrderSubtotalTo { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.AdditionalFixedCost")]
        public decimal AdditionalFixedCost { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.PercentageRateOfSubtotal")]
        public decimal PercentageRateOfSubtotal { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.RatePerWeightUnit")]
        public decimal RatePerWeightUnit { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.LowerWeightLimit")]
        public decimal LowerWeightLimit { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.Core.Fields.DataHtml")]
        public string DataHtml { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
        public string BaseWeightIn { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }
        public IList<SelectListItem> AvailableStates { get; set; }
        public IList<SelectListItem> AvailableShippingMethods { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableWarehouses { get; set; }
    }
}
