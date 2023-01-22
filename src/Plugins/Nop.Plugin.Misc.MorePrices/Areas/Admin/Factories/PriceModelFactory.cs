using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Misc.MorePrices.Areas.Admin.Extensions;
using Nop.Plugin.Misc.MorePrices.Areas.Admin.Models;
using Nop.Plugin.Misc.MorePrices.Domain;
using Nop.Plugin.Misc.MorePrices.Extensions;
using Nop.Plugin.Misc.MorePrices.Services;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Factories;
using Nop.Web.Framework.Models.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Misc.MorePrices.Areas.Admin.Factories
{
    public class PriceModelFactory : IPriceModelFactory
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly IAclSupportedModelFactory _aclSupportedModelFactory;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICategoryServiceExtension _categoryService;
        private readonly ICurrencyService _currencyService;
        private readonly ILocalizationService _localizationService;
        private readonly ILocalizedModelFactory _localizedModelFactory;
        private readonly IManufacturerServiceExtension _manufacturerService;
        private readonly IPictureService _pictureService;
        private readonly IPriceService _priceService;
        private readonly ISettingModelFactoryExtension _settingModelFactory;
        private readonly IStoreMappingSupportedModelFactory _storeMappingSupportedModelFactory;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public PriceModelFactory(CurrencySettings currencySettings,
            IAclSupportedModelFactory aclSupportedModelFactory,
            IBaseAdminModelFactory baseAdminModelFactory,
            ICategoryServiceExtension categoryService,
            ICurrencyService currencyService,
            ILocalizationService localizationService,
            ILocalizedModelFactory localizedModelFactory,
            IManufacturerServiceExtension manufacturerService,
            IPictureService pictureService,
            IPriceService priceService,
            ISettingModelFactoryExtension settingModelFactory,
            IStoreMappingSupportedModelFactory storeMappingSupportedModelFactory,
            IWorkContext workContext)
        {
            _currencySettings = currencySettings;
            _aclSupportedModelFactory = aclSupportedModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _categoryService = categoryService;
            _currencyService = currencyService;
            _localizationService = localizationService;
            _localizedModelFactory = localizedModelFactory;
            _manufacturerService = manufacturerService;
            _pictureService = pictureService;
            _priceService = priceService;
            _settingModelFactory = settingModelFactory;
            _storeMappingSupportedModelFactory = storeMappingSupportedModelFactory;
            _workContext = workContext;
        }

        #endregion

        public PriceSearchModel PreparePriceSearchModel(PriceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //a vendor should have access only to his prices
            searchModel.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //prepare available categories
            _baseAdminModelFactory.PrepareCategories(searchModel.AvailableCategories);

            //prepare available manufacturers
            _baseAdminModelFactory.PrepareManufacturers(searchModel.AvailableManufacturers);

            //prepare available stores
            _baseAdminModelFactory.PrepareStores(searchModel.AvailableStores);

            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(searchModel.AvailableVendors);

            //prepare available warehouses
            _baseAdminModelFactory.PrepareWarehouses(searchModel.AvailableWarehouses);

            //prepare "published" filter (0 - all; 1 - published only; 2 - unpublished only)
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "0",
                Text = _localizationService.GetResource("Admin.Catalog.Prices.List.SearchPublished.All")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "1",
                Text = _localizationService.GetResource("Admin.Catalog.Prices.List.SearchPublished.PublishedOnly")
            });
            searchModel.AvailablePublishedOptions.Add(new SelectListItem
            {
                Value = "2",
                Text = _localizationService.GetResource("Admin.Catalog.Prices.List.SearchPublished.UnpublishedOnly")
            });

            //prepare grid
            searchModel.SetGridPageSize();

            return searchModel;
        }

        public PriceListModel PreparePriceListModel(PriceSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get parameters to filter comments
            var overridePublished = searchModel.SearchPublishedId == 0 ? null : (bool?)(searchModel.SearchPublishedId == 1);
            if (_workContext.CurrentVendor != null)
                searchModel.SearchVendorId = _workContext.CurrentVendor.Id;
            var categoryIds = new List<int> { searchModel.SearchCategoryId };
            if (searchModel.SearchIncludeSubCategories && searchModel.SearchCategoryId > 0)
            {
                var childCategoryIds = _categoryService.GetChildCategoryIds(parentCategoryId: searchModel.SearchCategoryId, showHidden: true);
                categoryIds.AddRange(childCategoryIds);
            }

            //get prices
            var prices = _priceService.SearchPrices(categoryIds: categoryIds,
                manufacturerId: searchModel.SearchManufacturerId,
                storeId: searchModel.SearchStoreId,
                vendorId: searchModel.SearchVendorId,
                warehouseId: searchModel.SearchWarehouseId,
                //priceType: searchModel.SearchPriceTypeId > 0 ? (PriceType?)searchModel.SearchPriceTypeId : null,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            //prepare list model
            var model = new PriceListModel().PrepareToGrid(searchModel, prices, () =>
            {
                return prices.Select(price =>
                {
                    //fill in model values from the entity
                    var priceModel = price.ToModel<PriceModel>();

                    //fill in additional values (not existing in the entity)
                    var defaultPricePicture = _pictureService.GetPicturesByProductId(price.Id, 1).FirstOrDefault();
                    priceModel.ProductPictureThumbnailUrl = _pictureService.GetPictureUrl(ref defaultPricePicture, 75);
                    //priceModel.PriceTypeName = _localizationService.GetLocalizedEnum(price.PriceType);

                    return priceModel;
                });
            });

            return model;
        }

        public PriceModel PreparePriceModel(PriceModel model, Price price, bool excludeProperties = false)
        {
            Action<PriceLocalizedModel, int> localizedModelConfiguration = null;

            if (price != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = price.ToModel<PriceModel>();
                }

                if (!excludeProperties)
                {
                    model.SelectedCategoryIds = _categoryService.GetPriceCategoriesByPriceId(price.Id)
                        .Select(priceCategory => priceCategory.CategoryId).ToList();
                    model.SelectedManufacturerIds = _manufacturerService.GetPriceManufacturersByPriceId(price.Id)
                        .Select(priceManufacturer => priceManufacturer.ManufacturerId).ToList();
                }

                ////prepare copy price model
                //PrepareCopyPriceModel(model.CopyPriceModel, price);

                ////prepare nested search model
                //PreparePricePictureSearchModel(model.PricePictureSearchModel, price);
                //PreparePriceOrderSearchModel(model.PriceOrderSearchModel, price);
                //PrepareTierPriceSearchModel(model.TierPriceSearchModel, price);

                //define localized model configuration action
                localizedModelConfiguration = (locale, languageId) =>
                {
                    locale.Name = _localizationService.GetLocalized(price, entity => entity.Name, languageId, false, false);
                };
            }

            //set default values for the new model
            if (price == null)
            {
                model.Published = true;
            }

            model.PrimaryStoreCurrencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode;
            model.IsLoggedInAsVendor = _workContext.CurrentVendor != null;

            //prepare localized models
            if (!excludeProperties)
                model.Locales = _localizedModelFactory.PrepareLocalizedModels(localizedModelConfiguration);

            //prepare editor settings
            model.PriceEditorSettingsModel = _settingModelFactory.PreparePriceEditorSettingsModel();

            //prepare available vendors
            _baseAdminModelFactory.PrepareVendors(model.AvailableVendors,
                defaultItemText: _localizationService.GetResource("Admin.Catalog.Prices.Fields.Vendor.None"));

            //prepare model categories
            _baseAdminModelFactory.PrepareCategories(model.AvailableCategories, false);
            foreach (var categoryItem in model.AvailableCategories)
            {
                categoryItem.Selected = int.TryParse(categoryItem.Value, out var categoryId)
                    && model.SelectedCategoryIds.Contains(categoryId);
            }

            //prepare model manufacturers
            _baseAdminModelFactory.PrepareManufacturers(model.AvailableManufacturers, false);
            foreach (var manufacturerItem in model.AvailableManufacturers)
            {
                manufacturerItem.Selected = int.TryParse(manufacturerItem.Value, out var manufacturerId)
                    && model.SelectedManufacturerIds.Contains(manufacturerId);
            }

            //prepare model customer roles
            _aclSupportedModelFactory.PrepareModelCustomerRoles(model, price, excludeProperties);

            //prepare model stores
            _storeMappingSupportedModelFactory.PrepareModelStores(model, price, excludeProperties);

            return model;
        }

        private void PrepareTierPriceSearchModel(object tierPriceSearchModel, Price price)
        {
            throw new NotImplementedException();
        }

        private void PreparePricePictureSearchModel(object pricePictureSearchModel, Price price)
        {
            throw new NotImplementedException();
        }

        private void PreparePriceOrderSearchModel(object priceOrderSearchModel, Price price)
        {
            throw new NotImplementedException();
        }

        private void PrepareCopyPriceModel(object copyPriceModel, Price price)
        {
            throw new NotImplementedException();
        }
    }
}
