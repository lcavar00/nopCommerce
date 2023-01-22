using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.MorePrices.Areas.Admin.Models
{
    public class PriceSearchModel : BaseSearchModel
    {
        #region Ctor

        public PriceSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableManufacturers = new List<SelectListItem>();
            AvailableStores = new List<SelectListItem>();
            AvailableWarehouses = new List<SelectListItem>();
            AvailableVendors = new List<SelectListItem>();
            AvailableProductTypes = new List<SelectListItem>();
            AvailablePublishedOptions = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Catalog.Prices.List.SearchPriceName")]
        public string SearchProductName { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Prices.List.SearchCategory")]
        public int SearchCategoryId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Prices.List.SearchIncludeSubCategories")]
        public bool SearchIncludeSubCategories { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Prices.List.SearchManufacturer")]
        public int SearchManufacturerId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Prices.List.SearchStore")]
        public int SearchStoreId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Prices.List.SearchVendor")]
        public int SearchVendorId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Prices.List.SearchWarehouse")]
        public int SearchWarehouseId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Prices.List.SearchPriceType")]
        public int SearchProductTypeId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Prices.List.SearchPublished")]
        public int SearchPublishedId { get; set; }


        public bool IsLoggedInAsVendor { get; set; }

        public bool AllowVendorsToImportProducts { get; set; }

        public bool HideStoresList { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }

        public IList<SelectListItem> AvailableManufacturers { get; set; }

        public IList<SelectListItem> AvailableStores { get; set; }

        public IList<SelectListItem> AvailableWarehouses { get; set; }

        public IList<SelectListItem> AvailableVendors { get; set; }

        public IList<SelectListItem> AvailableProductTypes { get; set; }

        public IList<SelectListItem> AvailablePublishedOptions { get; set; }

        #endregion
    }
}
