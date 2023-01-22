using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.MorePrices.Areas.Admin.Models
{
    public partial class PriceModel : BaseNopEntityModel,
        IAclSupportedModel, ILocalizedModel<PriceLocalizedModel>, IStoreMappingSupportedModel
    {
        #region Ctor

        public PriceModel()
        {
            SelectedCategoryIds = new List<int>();
            AvailableCategories = new List<SelectListItem>();
            SelectedCustomerRoleIds = new List<int>();
            AvailableCustomerRoles = new List<SelectListItem>();
            SelectedManufacturerIds = new List<int>();
            AvailableManufacturers = new List<SelectListItem>();

            PriceEditorSettingsModel = new PriceEditorSettingsModel();
        }

        #endregion

        [NopResourceDisplayName("Admin.Catalog.Prices.Fields.Name")]
        public string Name { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Prices.Fields.Published")]
        public bool Published { get; set; }

        public IList<int> SelectedStoreIds { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IList<SelectListItem> AvailableStores { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IList<PriceLocalizedModel> Locales { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        //product thumbnail
        [NopResourceDisplayName("Admin.Catalog.Prices.Fields.ProductSku")]
        public string ProductSku { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Prices.Fields.ProductAttributeCombinationSku")]
        public string ProductAttributeCombinationSku { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Prices.Fields.ProductName")]
        public string ProductName { get; set; }
        [NopResourceDisplayName("Admin.Catalog.Prices.Fields.ProductPictureThumbnailUrl")]
        public string ProductPictureThumbnailUrl { get; set; }

        //categories
        public List<int> SelectedCategoryIds { get; set; }
        public List<SelectListItem> AvailableCategories { get; set; }

        //currency
        public string PrimaryStoreCurrencyCode { get; set; }

        //customers
        public List<int> SelectedCustomerRoleIds { get; set; }
        public List<SelectListItem> AvailableCustomerRoles { get; set; }

        //manufacturers
        public List<int> SelectedManufacturerIds { get; set; }
        public List<SelectListItem> AvailableManufacturers { get; set; }

        //vendor
        public bool IsLoggedInAsVendor { get; set; }

        //vendors
        [NopResourceDisplayName("Admin.Catalog.Prices.Fields.Vendor")]
        public int VendorId { get; set; }
        public IList<SelectListItem> AvailableVendors { get; set; }

        //editor settings
        public PriceEditorSettingsModel PriceEditorSettingsModel { get; set; }
        IList<int> IAclSupportedModel.SelectedCustomerRoleIds { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        IList<SelectListItem> IAclSupportedModel.AvailableCustomerRoles { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }

    public partial class PriceLocalizedModel : ILocalizedLocaleModel
    {
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.Catalog.Prices.Fields.Name")]
        public string Name { get; set; }
    }
}
