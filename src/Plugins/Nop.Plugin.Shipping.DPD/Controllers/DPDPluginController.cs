using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Plugin.Shipping.Core;
using Nop.Plugin.Shipping.Core.Controllers;
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
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.DPD.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class DPDPluginController : BaseShippingMethodPluginController
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly IGenericAttributeService _genericAttributeService;
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

        public DPDPluginController(CurrencySettings currencySettings,
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
            MeasureSettings measureSettings) : base(currencySettings, countryService, currencyService, genericAttributeService, localizationService, logger, measureService, permissionService, settingService, shippingByLocationByTotalByWeightService, shippingService, stateProvinceService, storeService, workContext, measureSettings)
        {
            _currencySettings = currencySettings;
            _countryService = countryService;
            _currencyService = currencyService;
            _genericAttributeService = genericAttributeService;
            _measureService = measureService;
            _permissionService = permissionService;
            _settingService = settingService;
            _shippingByLocationByTotalByWeightService = shippingByLocationByTotalByWeightService;
            _shippingService = shippingService;
            _stateProvinceService = stateProvinceService;
            _storeService = storeService;
            _workContext = workContext;
            _measureSettings = measureSettings;
        }

        #endregion

        public async Task<IActionResult> ApiConfiguration()
        {
            var dpdSettings = await _settingService.LoadSettingAsync<DPDSettings>() ?? new DPDSettings();

            return View("~/Plugins/Shipping.DPD/Views/ApiConfiguration.cshtml", dpdSettings);
        }

        public override async Task<IActionResult> Configure(bool showtour = false)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var dpdSettings = await _settingService.LoadSettingAsync<DPDSettings>() ?? new DPDSettings();

            var model = new Models.ConfigurationModel
            {
                ShippingByTotalByLocationEnabled = dpdSettings.ShippingByLocationByTotalByWeightEnabled,
                LimitMethodsToCreated = dpdSettings.LimitMethodsToCreated
            };
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var store in await _storeService.GetAllStoresAsync())
                model.AvailableStores.Add(new SelectListItem { Text = store.Name, Value = store.Id.ToString() });
            //warehouses
            model.AvailableWarehouses.Add(new SelectListItem { Text = "*", Value = "0" });
            foreach (var warehouses in await _shippingService.GetAllWarehousesAsync())
                model.AvailableWarehouses.Add(new SelectListItem { Text = warehouses.Name, Value = warehouses.Id.ToString() });

            //shipping methods
            var shippingMethods = (await _shippingService.GetAllShippingMethodsAsync()).Where(a => a.Name.ToLower().Contains("dpd")).ToList();
            foreach (var sm in shippingMethods)
            {
                model.AvailableShippingMethods.Add(new SelectListItem { Text = sm.Name, Value = sm.Id.ToString() });
            }
            //countries
            model.AvailableCountries.Add(new SelectListItem { Text = "*", Value = "0" });
            var countries = await _countryService.GetAllCountriesAsync();
            foreach (var c in countries)
                model.AvailableCountries.Add(new SelectListItem { Text = c.Name, Value = c.Id.ToString() });
            //states
            model.AvailableStates.Add(new SelectListItem { Text = "*", Value = "0" });

            model.SetGridPageSize();

            //show configuration tour
            if (showtour)
            {
                var hideCard = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.HideConfigurationStepsAttribute);

                var closeCard = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), NopCustomerDefaults.CloseConfigurationStepsAttribute);

                if (!hideCard && !closeCard)
                    ViewBag.ShowTour = true;
            }

            return View("~/Plugins/Shipping.DPD/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public override async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return Content("Access denied");

            var dpdSettings = await _settingService.LoadSettingAsync<DPDSettings>() ?? new DPDSettings();

            //save settings
            dpdSettings.LimitMethodsToCreated = model.LimitMethodsToCreated;
            await _settingService.SaveSettingAsync(dpdSettings);

            return Json(new { Result = true });
        }

        [HttpPost]
        public async Task<IActionResult> SaveSetting(DPDSettings model)
        {
            if (ModelState.IsValid)
                await _settingService.SaveSettingAsync(model);

            return RedirectToAction("Configure");
        }

        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> SaveMode(bool value)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return Content("Access denied");

            var dpdSettings = await _settingService.LoadSettingAsync<DPDSettings>() ?? new DPDSettings();

            //save settings
            dpdSettings.ShippingByLocationByTotalByWeightEnabled = value;
            await _settingService.SaveSettingAsync(dpdSettings);

            return Json(new { Result = true });
        }

        #region Fixed Rate

        public override async Task<IActionResult> FixedShippingRateList(ConfigurationModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageShippingSettings))
                return await AccessDeniedDataTablesJson();

            var shippingMethods = (await _shippingService.GetAllShippingMethodsAsync()).Where(a => a.Name.ToLower().Contains("dpd")).ToList().ToPagedList(searchModel);

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

        public override async Task<IActionResult> UpdateFixedShippingRate(FixedRateModel model)
        {
            return await base.UpdateFixedShippingRate(model);
        }

        #endregion

        #region Rate by location by total by weight

        public override async Task<IActionResult> RateByLocationByTotalByWeightList(ConfigurationModel searchModel, ConfigurationModel filter)
        {
            return await base.RateByLocationByTotalByWeightList(searchModel, filter);
        }

        public override async Task<IActionResult> AddRateByLocationByTotalByWeightPopup()
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

            var shippingMethods = (await _shippingService.GetAllShippingMethodsAsync()).Where(a => a.Name.ToLower().Contains("dpd")).ToList();
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

        public override async Task<IActionResult> EditRateByLocationByTotalByWeightPopup(int id)
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

            var shippingMethods = (await _shippingService.GetAllShippingMethodsAsync()).Where(a => a.Name.ToLower().Contains("dpd")).ToList();
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

        #endregion
    }
}
