using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Shipping.Core.Domain;
using Nop.Plugin.Shipping.Core.Models;
using Nop.Plugin.Shipping.Core.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.Core.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    [AutoValidateAntiforgeryToken]
    public class BaseShippingMethodPluginController : BasePluginController
    {
		#region Fields
		
        private readonly CurrencySettings _currencySettings;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IMeasureService _measureService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IShippingByLocationByTotalByWeightService _shippingByLocationByTotalByWeightService;
        private readonly IShippingService _shippingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly MeasureSettings _measureSettings;

        #endregion

        #region Ctor

        public BaseShippingMethodPluginController(CurrencySettings currencySettings,
            ICountryService countryService,
            ICurrencyService currencyService,
            IGenericAttributeService genericAttributeService,
            ILocalizationService localizationService,
            ILogger logger,
            IMeasureService measureService,
            IPermissionService permissionService,
            ISettingService settingService,
            IShippingByLocationByTotalByWeightService shippingByLocationByTotalByWeightService,
            IShippingService shippingService,
            IStateProvinceService stateProvinceService,
            IStoreService storeService,
            IWorkContext workContext,
            MeasureSettings measureSettings)
        {
            _currencySettings = currencySettings;
            _countryService = countryService;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _logger = logger;
            _measureService = measureService;
            _permissionService = permissionService;
            _settingService = settingService;
            _shippingByLocationByTotalByWeightService = shippingByLocationByTotalByWeightService;
            _shippingService = shippingService;
            _stateProvinceService = stateProvinceService;
            _storeService = storeService;
            _measureSettings = measureSettings;
            _workContext = workContext;
        }

        #endregion

        public virtual async Task<IActionResult> Configure(bool showtour = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            //show configuration tour
            if (showtour)
            {
                var hideCard = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.HideConfigurationStepsAttribute);

                var closeCard = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CloseConfigurationStepsAttribute);

                if (!hideCard && !closeCard)
                    ViewBag.ShowTour = true;
            }

            return View("~/Plugins/Shipping.Core/Views/Configure.cshtml");
        }

        [HttpPost]
        public virtual async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return Content("Access denied");

            var shippingRateComputationSettings = await _settingService.LoadSettingAsync<ShippingRateComputationSettings>() ?? new ShippingRateComputationSettings();

            //save settings
            shippingRateComputationSettings.LimitMethodsToCreated = model.LimitMethodsToCreated;
            await _settingService.SaveSettingAsync(shippingRateComputationSettings);

            return Json(new { Result = true });
        }

        #region Fixed rate

        [HttpPost]
        public virtual async Task<IActionResult> FixedShippingRateList(ConfigurationModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return await AccessDeniedDataTablesJson();

            var shippingMethods = (await _shippingService.GetAllShippingMethodsAsync()).ToPagedList(searchModel);

            var gridModel = await new FixedRateListModel().PrepareToGridAsync(searchModel, shippingMethods, () =>
            {
                return shippingMethods.SelectAwait(async shippingMethod => new FixedRateModel
                {
                    ShippingMethodId = shippingMethod.Id,
                    ShippingMethodName = shippingMethod.Name,
                    Rate = await _settingService.GetSettingByKeyAsync<decimal>(
                        string.Format(ShippingCoreDefaults.FIXED_RATE_SETTINGS_KEY, shippingMethod.Id))
                });
            });

            return Json(gridModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> UpdateFixedShippingRate(FixedRateModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return Content("Access denied");

            await _settingService.SetSettingAsync(string.Format(ShippingCoreDefaults.FIXED_RATE_SETTINGS_KEY, model.ShippingMethodId), model.Rate);

            return new NullJsonResult();
        }

        #endregion

        #region Rate by total by location

        public virtual async Task<IActionResult> RateByLocationByTotalByWeightList(ConfigurationModel searchModel, ConfigurationModel filter)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return await AccessDeniedDataTablesJson();

            //var records = _shippingByWeightService.GetAll(command.Page - 1, command.PageSize);
            var records = await _shippingByLocationByTotalByWeightService.FindRecordsAsync(
              pageIndex: searchModel.Page - 1,
              pageSize: searchModel.PageSize,
              storeId: filter.SearchStoreId,
              warehouseId: filter.SearchWarehouseId,
              countryId: filter.SearchCountryId,
              stateProvinceId: filter.SearchStateProvinceId,
              zip: filter.SearchZip,
              shippingMethodId: filter.SearchShippingMethodId,
              weight: null,
              orderSubtotal: null
              );

            var gridModel = await new ShippingByTotalByLocationListModel().PrepareToGridAsync(searchModel, records, () =>
            {
                return records.SelectAwait(async record =>
                {
                    var model = new ShippingByLocationByTotalByWeightModel
                    {
                        Id = record.Id,
                        StoreId = record.StoreId,
                        StoreName = (await _storeService.GetStoreByIdAsync(record.StoreId))?.Name ?? "*",
                        WarehouseId = record.WarehouseId,
                        WarehouseName = (await _shippingService.GetWarehouseByIdAsync(record.WarehouseId))?.Name ?? "*",
                        ShippingMethodId = record.ShippingMethodId,
                        ShippingMethodName = (await _shippingService.GetShippingMethodByIdAsync(record.ShippingMethodId))?.Name ??
                                             "Unavailable",
                        CountryId = record.CountryId,
                        CountryName = (await _countryService.GetCountryByIdAsync(record.CountryId))?.Name ?? "*",
                        StateProvinceId = record.StateProvinceId,
                        StateProvinceName =
                            (await _stateProvinceService.GetStateProvinceByIdAsync(record.StateProvinceId))?.Name ?? "*",
                        WeightFrom = record.WeightFrom,
                        WeightTo = record.WeightTo,
                        OrderSubtotalFrom = record.OrderSubtotalFrom,
                        OrderSubtotalTo = record.OrderSubtotalTo,
                        AdditionalFixedCost = record.AdditionalFixedCost,
                        PercentageRateOfSubtotal = record.PercentageRateOfSubtotal,
                        RatePerWeightUnit = record.RatePerWeightUnit,
                        LowerWeightLimit = record.LowerWeightLimit,
                        Zip = !string.IsNullOrEmpty(record.Zip) ? record.Zip : "*"
                    };

                    var htmlSb = new StringBuilder("<div>");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync("Plugins.Shipping.Core.Fields.WeightFrom"),
                        model.WeightFrom);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync("Plugins.Shipping.Core.Fields.WeightTo"),
                        model.WeightTo);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync(
                            "Plugins.Shipping.Core.Fields.OrderSubtotalFrom"), model.OrderSubtotalFrom);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync(
                            "Plugins.Shipping.Core.Fields.OrderSubtotalTo"), model.OrderSubtotalTo);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync(
                            "Plugins.Shipping.Core.Fields.AdditionalFixedCost"),
                        model.AdditionalFixedCost);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync(
                            "Plugins.Shipping.Core.Fields.RatePerWeightUnit"), model.RatePerWeightUnit);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync(
                            "Plugins.Shipping.Core.Fields.LowerWeightLimit"), model.LowerWeightLimit);
                    htmlSb.Append("<br />");
                    htmlSb.AppendFormat("{0}: {1}",
                        await _localizationService.GetResourceAsync(
                            "Plugins.Shipping.Core.Fields.PercentageRateOfSubtotal"),
                        model.PercentageRateOfSubtotal);

                    htmlSb.Append("</div>");
                    model.DataHtml = htmlSb.ToString();

                    return model;
                });
            });

            return Json(gridModel);
        }

        public virtual async Task<IActionResult> AddRateByLocationByTotalByWeightPopup()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var model = new ShippingByLocationByTotalByWeightModel
            {
                PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode,
                BaseWeightIn = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name,
                WeightTo = 1000000,
                OrderSubtotalFrom = 0,
                OrderSubtotalTo = 100000,
            };

            var shippingMethods = await _shippingService.GetAllShippingMethodsAsync();
            if (!shippingMethods.Any())
                return Content("No shipping methods can be loaded");

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var store in await _storeService.GetAllStoresAsync())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var warehouses in await _shippingService.GetAllWarehousesAsync())
                model.AvailableWarehouses.Add(new SelectListItem { Text = warehouses.Name, Value = warehouses.Id.ToString() });
            //shipping methods
            foreach (var sm in shippingMethods)
                model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString() });
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            var countries = await _countryService.GetAllCountriesAsync(showHidden: true);
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });

            return View("~/Plugins/Shipping.Core/Views/AddRateByLocationByTotalByWeightPopup.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> AddRateByLocationByTotalByWeightPopup(ShippingByLocationByTotalByWeightModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            ViewBag.RefreshPage = true;

            _shippingByLocationByTotalByWeightService.InsertShippingByLocationByTotalByWeightRecordAsync(new ShippingByLocationByTotalByWeightRecord
            {
                StoreId = model.StoreId,
                WarehouseId = model.WarehouseId,
                CountryId = model.CountryId,
                StateProvinceId = model.StateProvinceId,
                Zip = model.Zip == "*" ? null : model.Zip,
                ShippingMethodId = model.ShippingMethodId,
                WeightFrom = model.WeightFrom,
                WeightTo = model.WeightTo,
                OrderSubtotalFrom = model.OrderSubtotalFrom,
                OrderSubtotalTo = model.OrderSubtotalTo,
                AdditionalFixedCost = model.AdditionalFixedCost,
                RatePerWeightUnit = model.RatePerWeightUnit,
                PercentageRateOfSubtotal = model.PercentageRateOfSubtotal,
                LowerWeightLimit = model.LowerWeightLimit
            });

            return View("~/Plugins/Shipping.Core/Views/AddRateByLocationByTotalByWeightPopup.cshtml", model);
        }

        public virtual async Task<IActionResult> EditRateByLocationByTotalByWeightPopup(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var sbw = await _shippingByLocationByTotalByWeightService.GetByIdAsync(id);
            if (sbw == null)
            {
                return RedirectToAction("Configure");
            }

            var model = new ShippingByLocationByTotalByWeightModel
            {
                Id = sbw.Id,
                StoreId = sbw.StoreId,
                WarehouseId = sbw.WarehouseId,
                CountryId = sbw.CountryId,
                StateProvinceId = sbw.StateProvinceId,
                Zip = sbw.Zip,
                ShippingMethodId = sbw.ShippingMethodId,
                WeightFrom = sbw.WeightFrom,
                WeightTo = sbw.WeightTo,
                OrderSubtotalFrom = sbw.OrderSubtotalFrom,
                OrderSubtotalTo = sbw.OrderSubtotalTo,
                AdditionalFixedCost = sbw.AdditionalFixedCost,
                PercentageRateOfSubtotal = sbw.PercentageRateOfSubtotal,
                RatePerWeightUnit = sbw.RatePerWeightUnit,
                LowerWeightLimit = sbw.LowerWeightLimit,
                PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode,
                BaseWeightIn = (await _measureService.GetMeasureWeightByIdAsync(_measureSettings.BaseWeightId))?.Name
            };

            var shippingMethods = await _shippingService.GetAllShippingMethodsAsync();
            if (!shippingMethods.Any())
                return Content("No shipping methods can be loaded");

            var selectedStore = await _storeService.GetStoreByIdAsync(sbw.StoreId);
            var selectedWarehouse = await _shippingService.GetWarehouseByIdAsync(sbw.WarehouseId);
            var selectedShippingMethod = await _shippingService.GetShippingMethodByIdAsync(sbw.ShippingMethodId);
            var selectedCountry = await _countryService.GetCountryByIdAsync(sbw.CountryId);
            var selectedState = await _stateProvinceService.GetStateProvinceByIdAsync(sbw.StateProvinceId);
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var store in await _storeService.GetAllStoresAsync())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString(), Selected = (selectedStore != null && store.Id == selectedStore.Id) });
            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var warehouse in await _shippingService.GetAllWarehousesAsync())
                model.AvailableWarehouses.Add(new SelectListItem { Text = warehouse.Name, Value = warehouse.Id.ToString(), Selected = (selectedWarehouse != null && warehouse.Id == selectedWarehouse.Id) });
            //shipping methods
            foreach (var sm in shippingMethods)
                model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString(), Selected = (selectedShippingMethod != null && sm.Id == selectedShippingMethod.Id) });
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            var countries = await _countryService.GetAllCountriesAsync(showHidden: true);
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString(), Selected = (selectedCountry != null && c.Id == selectedCountry.Id) });
            //states
            var states = selectedCountry != null ? await _stateProvinceService.GetStateProvincesByCountryIdAsync(selectedCountry.Id, showHidden: true) : new List<StateProvince>();
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var s in states)
                model.AvailableStates.Add(new SelectListItem { Text = s.Name, Value = s.Id.ToString(), Selected = (selectedState != null && s.Id == selectedState.Id) });

            return View("~/Plugins/Shipping.Core/Views/EditRateByLocationByTotalByWeightPopup.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> EditRateByLocationByTotalByWeightPopup(ShippingByLocationByTotalByWeightModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var sbt = await _shippingByLocationByTotalByWeightService.GetByIdAsync(model.Id);
            if (sbt == null)
                //no record found with the specified id
                return RedirectToAction("Configure");

            sbt.StoreId = model.StoreId;
            sbt.WarehouseId = model.WarehouseId;
            sbt.CountryId = model.CountryId;
            sbt.StateProvinceId = model.StateProvinceId;
            sbt.Zip = model.Zip == "*" ? null : model.Zip;
            sbt.ShippingMethodId = model.ShippingMethodId;
            sbt.WeightFrom = model.WeightFrom;
            sbt.WeightTo = model.WeightTo;
            sbt.OrderSubtotalFrom = model.OrderSubtotalFrom;
            sbt.OrderSubtotalTo = model.OrderSubtotalTo;
            sbt.AdditionalFixedCost = model.AdditionalFixedCost;
            sbt.RatePerWeightUnit = model.RatePerWeightUnit;
            sbt.PercentageRateOfSubtotal = model.PercentageRateOfSubtotal;
            sbt.LowerWeightLimit = model.LowerWeightLimit;

            _shippingByLocationByTotalByWeightService.UpdateShippingByLocationByTotalByWeightRecordAsync(sbt);

            ViewBag.RefreshPage = true;

            return View("~/Plugins/Shipping.Core/Views/EditRateByLocationByTotalByWeightPopup.cshtml", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteRateByLocationByTotalByWeight(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return Content("Access denied");

            var sbw = await _shippingByLocationByTotalByWeightService.GetByIdAsync(id);
            if (sbw != null)
                _shippingByLocationByTotalByWeightService.DeleteShippingByLocationByTotalByWeightRecordAsync(sbw);

            return new NullJsonResult();
        }

        #endregion
    }
}
