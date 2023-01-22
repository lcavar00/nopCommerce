using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Core.Events;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Plugin.Shipping.Core.Areas.Admin.Factories;
using Nop.Plugin.Shipping.Core.Areas.Admin.Models;
using Nop.Services.Authentication.MultiFactor;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Gdpr;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Media.RoxyFileman;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Services.Stores;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Settings;
using System;
using System.Threading.Tasks;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Controllers
{
    public partial class CustomSettingController : SettingController
    {
        #region Fields

        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressService _addressService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingModelFactoryExtension _settingModelFactory;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public CustomSettingController(AppSettings appSettings,
            IAddressAttributeParser addressAttributeParser,
            IAddressService addressService, 
            ICustomerActivityService customerActivityService, 
            ICustomerService customerService, 
            INopDataProvider dataProvider, 
            IEncryptionService encryptionService, 
            IEventPublisher eventPublisher, 
            IGenericAttributeService genericAttributeService, 
            IGdprService gdprService, 
            ILocalizedEntityService localizedEntityService, 
            ILocalizationService localizationService, 
            IMultiFactorAuthenticationPluginManager multiFactorAuthenticationPluginManager, 
            INopFileProvider fileProvider, 
            INotificationService notificationService, 
            IOrderService orderService, 
            IPermissionService permissionService, 
            IPictureService pictureService, 
            IRoxyFilemanService roxyFilemanService, 
            IServiceScopeFactory serviceScopeFactory, 
            ISettingModelFactoryExtension settingModelFactory, 
            ISettingService settingService, 
            IStoreContext storeContext, 
            IStoreService storeService, 
            IWorkContext workContext, 
            IUploadService uploadService) : base(appSettings, addressService, customerActivityService, customerService, dataProvider, encryptionService, eventPublisher, genericAttributeService, gdprService, localizedEntityService, localizationService, multiFactorAuthenticationPluginManager, fileProvider, notificationService, orderService, permissionService, pictureService, roxyFilemanService, serviceScopeFactory, settingModelFactory, settingService, storeContext, storeService, workContext, uploadService)
        {
            _addressAttributeParser = addressAttributeParser;
            _addressService = addressService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingModelFactory = settingModelFactory;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        public override async Task<IActionResult> Shipping()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareShippingSettingsExtensionModelAsync();

            return View($"~/Plugins/Shipping.Core/Areas/Admin/Views/CustomSettings/Shipping.cshtml", model);
        }

        public async Task<IActionResult> CustomShipping()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //prepare model
            var model = await _settingModelFactory.PrepareShippingSettingsExtensionModelAsync();

            return View($"~/Plugins/Shipping.Core/Areas/Admin/Views/CustomSettings/Shipping.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> CustomShipping(ShippingSettingsModelExtension model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSettings))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var shippingSettings = await _settingService.LoadSettingAsync<ShippingSettings>(storeScope);
            shippingSettings = (model as ShippingSettingsModel).ToSettings(shippingSettings);

            //we do not clear cache after each setting update.
            //this behavior can increase performance because cached settings will not be cleared 
            //and loaded from database after each update
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.ShipToSameAddress, model.ShipToSameAddress_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.AllowPickupInStore, model.AllowPickupInStore_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.DisplayPickupPointsOnMap, model.DisplayPickupPointsOnMap_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.IgnoreAdditionalShippingChargeForPickupInStore, model.IgnoreAdditionalShippingChargeForPickupInStore_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.GoogleMapsApiKey, model.GoogleMapsApiKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.UseWarehouseLocation, model.UseWarehouseLocation_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.NotifyCustomerAboutShippingFromMultipleLocations, model.NotifyCustomerAboutShippingFromMultipleLocations_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.FreeShippingOverXEnabled, model.FreeShippingOverXEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.FreeShippingOverXValue, model.FreeShippingOverXValue_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.FreeShippingOverXIncludingTax, model.FreeShippingOverXIncludingTax_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.EstimateShippingCartPageEnabled, model.EstimateShippingCartPageEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.EstimateShippingProductPageEnabled, model.EstimateShippingProductPageEnabled_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.DisplayShipmentEventsToCustomers, model.DisplayShipmentEventsToCustomers_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.DisplayShipmentEventsToStoreOwner, model.DisplayShipmentEventsToStoreOwner_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.HideShippingTotal, model.HideShippingTotal_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.BypassShippingMethodSelectionIfOnlyOne, model.BypassShippingMethodSelectionIfOnlyOne_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(shippingSettings, x => x.ConsiderAssociatedProductsDimensions, model.ConsiderAssociatedProductsDimensions_OverrideForStore, storeScope, false);

            if (model.ShippingOriginAddress_OverrideForStore || storeScope == 0)
            {
                var pickupAddressSettings = await _settingService.LoadSettingAsync<PickupAddressSettings>(model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.ActiveStoreScopeConfiguration);

                var addressId = await _settingService.SettingExistsAsync(shippingSettings, x => x.ShippingOriginAddressId, storeScope) ?
                    shippingSettings.ShippingOriginAddressId : 0;
                var originAddress = await _addressService.GetAddressByIdAsync(addressId) ??
                    new Address
                    {
                        CreatedOnUtc = DateTime.UtcNow
                    };

                var form = HttpContext.Request.Form;

                var city = form["City"];
                var firstName = form["FirstName"];
                var lastName = form["LastName"];
                var phoneNumber = form["PhoneNumber"];
                var company = form["Company"];
                int.TryParse(form["CountryId"], out int countryId);
                int.TryParse(form["StateProvinceId"], out int stateProvinceId);
                var county = form["County"];
                var email = form["email"];
                var faxNumber = form["FaxNumer"];
                var address1 = form["Address1"];
                var address2 = form["Address2"];
                var zipPostalCode = form["ZipPostalCode"];

                //update address
                originAddress.City = city;
                originAddress.FirstName = firstName;
                originAddress.LastName = lastName;
                originAddress.PhoneNumber = phoneNumber;
                originAddress.Company = company;
                originAddress.CountryId = countryId;
                originAddress.StateProvinceId = stateProvinceId;
                originAddress.County = county;
                originAddress.Email = email;
                originAddress.FaxNumber = faxNumber;
                originAddress.Address1 = address1;
                originAddress.Address2 = address2;
                originAddress.ZipPostalCode = zipPostalCode;

                pickupAddressSettings.CityRequired = form["PickupAddressSettingsModel.CityRequired"] != "false";
                pickupAddressSettings.CompanyRequired = form["PickupAddressSettingsModel.CompanyRequired"] != "false";
                pickupAddressSettings.CountryRequired = form["PickupAddressSettingsModel.CountryRequired"] != "false";
                pickupAddressSettings.CountyRequired = form["PickupAddressSettingsModel.CountyRequired"] != "false";
                pickupAddressSettings.EmailRequired = form["PickupAddressSettingsModel.EmailRequired"] != "false";
                pickupAddressSettings.FaxRequired = form["PickupAddressSettingsModel.FaxRequired"] != "false";
                pickupAddressSettings.FirstNameRequired = form["PickupAddressSettingsModel.FirstNameRequired"] != "false";
                pickupAddressSettings.LastNameRequired = form["PickupAddressSettingsModel.LastNameRequired"] != "false";
                pickupAddressSettings.PhoneRequired = form["PickupAddressSettingsModel.PhoneRequired"] != "false";
                pickupAddressSettings.StateProvinceEnabled = form["PickupAddressSettingsModel.StateProvinceEnabled"] != "false";
                pickupAddressSettings.StreetAddress2Required = form["PickupAddressSettingsModel.StreetAddress2Required"] != "false";
                pickupAddressSettings.StreetAddressRequired = form["PickupAddressSettingsModel.StreetAddressRequired"] != "false";
                pickupAddressSettings.ZipPostalCodeRequired = form["PickupAddressSettingsModel.ZipPostalCodeRequired"] != "false";

                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.CityRequired = form["PickupAddressSettingsModel.CityRequired"] != "false";
                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.CompanyRequired = form["PickupAddressSettingsModel.CompanyRequired"] != "false";
                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.CountryRequired = form["PickupAddressSettingsModel.CountryRequired"] != "false";
                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.CountyRequired = form["PickupAddressSettingsModel.CountyRequired"] != "false";
                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.EmailRequired = form["PickupAddressSettingsModel.EmailRequired"] != "false";
                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.FaxRequired = form["PickupAddressSettingsModel.FaxRequired"] != "false";
                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.FirstNameRequired = form["PickupAddressSettingsModel.FirstNameRequired"] != "false";
                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.LastNameRequired = form["PickupAddressSettingsModel.LastNameRequired"] != "false";
                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.PhoneRequired = form["PickupAddressSettingsModel.PhoneRequired"] != "false";
                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.StateProvinceEnabled = form["PickupAddressSettingsModel.StateProvinceEnabled"] != "false";
                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.StreetAddress2Required = form["PickupAddressSettingsModel.StreetAddress2Required"] != "false";
                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.StreetAddressRequired = form["PickupAddressSettingsModel.StreetAddressRequired"] != "false";
                model.ShippingPickupAddressSettingsModel.PickupAddressSettingsModel.ZipPostalCodeRequired = form["PickupAddressSettingsModel.ZipPostalCodeRequired"] != "false";

                //check if required fields are empty
                if (string.IsNullOrEmpty(originAddress.Address1) && pickupAddressSettings.StreetAddressRequired)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.Address1.Required", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }
                if(string.IsNullOrEmpty(originAddress.Address2) && pickupAddressSettings.StreetAddress2Required)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.Address2.Required", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }
                if (string.IsNullOrEmpty(originAddress.City) && pickupAddressSettings.CityRequired)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.City.Required", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }
                if (string.IsNullOrEmpty(originAddress.Company) && pickupAddressSettings.CompanyRequired)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.Company.Required", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }
                if ((originAddress.CountryId == 0) && pickupAddressSettings.CountryRequired)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.Country.Required", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }
                if (string.IsNullOrEmpty(originAddress.County) && pickupAddressSettings.CountyRequired)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.County.Required", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }
                if (string.IsNullOrEmpty(originAddress.Email) && pickupAddressSettings.EmailRequired)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.Email.Required", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }
                if (string.IsNullOrEmpty(originAddress.FaxNumber) && pickupAddressSettings.FaxRequired)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.FaxNumber.Required", _workContext.GetWorkingLanguageAsync().Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }
                if (string.IsNullOrEmpty(originAddress.FirstName) && pickupAddressSettings.FirstNameRequired)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.FirstName.Required", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }
                if (string.IsNullOrEmpty(originAddress.LastName) && pickupAddressSettings.LastNameRequired)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.LastName.Required", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }
                if (string.IsNullOrEmpty(originAddress.PhoneNumber) && pickupAddressSettings.PhoneRequired)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.PhoneNumber.Required", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }
                if ((originAddress.StateProvinceId == 0) && pickupAddressSettings.StateProvinceEnabled)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.StateProvinceRequired.Required", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }
                if (string.IsNullOrEmpty(originAddress.ZipPostalCode) && pickupAddressSettings.ZipPostalCodeRequired)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Address.Fields.ZipPostalCode.Required", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    string.Empty;
                    ModelState.AddModelError(string.Empty, error);
                }

                var customAttributes = await _addressAttributeParser.ParseCustomAddressAttributesAsync(form);
                var customAttributeWarnings = await _addressAttributeParser.GetAttributeWarningsAsync(customAttributes);
                foreach (var error in customAttributeWarnings)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
                originAddress.CustomAttributes = customAttributes;

                if (ModelState.ErrorCount > 0)
                {
                    var error = (await _localizationService.GetLocaleStringResourceByNameAsync("Admin.Configuration.Settings.Shipping.ShippingOriginAddress.Error", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ??
                                    "Origin address fields required.";

                    _notificationService.ErrorNotification(error);

                    //model = await _settingModelFactory.PrepareShippingSettingsExtensionModelAsync();

                    return View($"~/Plugins/Shipping.Core/Areas/Admin/Views/CustomSettings/Shipping.cshtml", model);
                }

                //update ID manually (in case we're in multi-store configuration mode it'll be set to the shared one)
                if (originAddress.Id > 0)
                    await _addressService.UpdateAddressAsync(originAddress);
                else
                    await _addressService.InsertAddressAsync(originAddress);
                shippingSettings.ShippingOriginAddressId = originAddress.Id;

                await _settingService.SaveSettingAsync(shippingSettings, x => x.ShippingOriginAddressId, storeScope, false);
                await _settingService.SaveSettingAsync(pickupAddressSettings);

            }
            else if (storeScope > 0)
                await _settingService.DeleteSettingAsync(shippingSettings, x => x.ShippingOriginAddressId, storeScope);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            //activity log
            await _customerActivityService.InsertActivityAsync("EditSettings", await _localizationService.GetResourceAsync("ActivityLog.EditSettings"));

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Configuration.Updated"));

            return RedirectToAction("Shipping");
        }

    }
}
