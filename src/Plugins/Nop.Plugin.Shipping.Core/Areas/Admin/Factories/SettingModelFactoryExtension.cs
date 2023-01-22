using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Configuration;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Plugin.Shipping.Core.Areas.Admin.Models;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Gdpr;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Stores;
using Nop.Services.Themes;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Settings;
using Nop.Web.Framework.Factories;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Factories
{
    public class SettingModelFactoryExtension : SettingModelFactory, ISettingModelFactoryExtension
    {
        #region Fields

        private readonly CurrencySettings _currencySettings;
        private readonly IAddressAttributeModelFactory _addressAttributeModelFactory;
        private readonly IAddressService _addressService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ICurrencyService _currencyService;
        private readonly ISettingService _settingService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly IStoreContext _storeContext;

        #endregion

        #region Ctor

        public SettingModelFactoryExtension(AppSettings appSettings,
            CurrencySettings currencySettings, 
            IAddressModelFactory addressModelFactory, 
            IAddressAttributeModelFactory addressAttributeModelFactory, 
            IAddressService addressService, 
            IBaseAdminModelFactory baseAdminModelFactory, 
            ICurrencyService currencyService, 
            ICustomerAttributeModelFactory customerAttributeModelFactory, 
            INopDataProvider dataProvider, 
            IDateTimeHelper dateTimeHelper, 
            IGdprService gdprService, 
            ILocalizedModelFactory localizedModelFactory, 
            IGenericAttributeService genericAttributeService, 
            ILocalizationService localizationService, 
            IPictureService pictureService, 
            IReturnRequestModelFactory returnRequestModelFactory, 
            ISettingService settingService, 
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext, 
            IStoreService storeService, 
            IThemeProvider themeProvider, 
            IVendorAttributeModelFactory vendorAttributeModelFactory, 
            IReviewTypeModelFactory reviewTypeModelFactory,
            IWorkContext workContext) : base(appSettings, currencySettings, addressModelFactory, addressAttributeModelFactory, addressService, baseAdminModelFactory, currencyService, customerAttributeModelFactory, dataProvider, dateTimeHelper, gdprService, localizedModelFactory, genericAttributeService, localizationService, pictureService, returnRequestModelFactory, settingService, storeContext, storeService, themeProvider, vendorAttributeModelFactory, reviewTypeModelFactory, workContext)
        {
            _currencySettings = currencySettings;
            _addressAttributeModelFactory = addressAttributeModelFactory;
            _addressService = addressService;
            _baseAdminModelFactory = baseAdminModelFactory;
            _currencyService = currencyService;
            _settingService = settingService;
            _stateProvinceService = stateProvinceService;
            _storeContext = storeContext;
        }

        #endregion

        public async Task<ShippingSettingsModelExtension> PrepareShippingSettingsExtensionModelAsync()
        {
            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var shippingSettings = await _settingService.LoadSettingAsync<ShippingSettings>(storeId);

            //fill in model values from the entity
            var shippingSettingsModel = shippingSettings.ToSettingsModel<ShippingSettingsModel>();
            

            //fill in additional values (not existing in the entity)
            shippingSettingsModel.ActiveStoreScopeConfiguration = storeId;
            shippingSettingsModel.PrimaryStoreCurrencyCode = (await _currencyService.GetCurrencyByIdAsync(_currencySettings.PrimaryStoreCurrencyId))?.CurrencyCode;

            //fill in overridden values
            if (storeId > 0)
            {
                shippingSettingsModel.ShipToSameAddress_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.ShipToSameAddress, storeId);
                shippingSettingsModel.AllowPickupInStore_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.AllowPickupInStore, storeId);
                shippingSettingsModel.DisplayPickupPointsOnMap_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.DisplayPickupPointsOnMap, storeId);
                shippingSettingsModel.IgnoreAdditionalShippingChargeForPickupInStore_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.IgnoreAdditionalShippingChargeForPickupInStore, storeId);
                shippingSettingsModel.GoogleMapsApiKey_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.GoogleMapsApiKey, storeId);
                shippingSettingsModel.UseWarehouseLocation_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.UseWarehouseLocation, storeId);
                shippingSettingsModel.NotifyCustomerAboutShippingFromMultipleLocations_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.NotifyCustomerAboutShippingFromMultipleLocations, storeId);
                shippingSettingsModel.FreeShippingOverXEnabled_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.FreeShippingOverXEnabled, storeId);
                shippingSettingsModel.FreeShippingOverXValue_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.FreeShippingOverXValue, storeId);
                shippingSettingsModel.FreeShippingOverXIncludingTax_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.FreeShippingOverXIncludingTax, storeId);
                shippingSettingsModel.EstimateShippingCartPageEnabled_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.EstimateShippingCartPageEnabled, storeId);
                shippingSettingsModel.EstimateShippingProductPageEnabled_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.EstimateShippingProductPageEnabled, storeId);
                shippingSettingsModel.DisplayShipmentEventsToCustomers_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.DisplayShipmentEventsToCustomers, storeId);
                shippingSettingsModel.DisplayShipmentEventsToStoreOwner_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.DisplayShipmentEventsToStoreOwner, storeId);
                shippingSettingsModel.HideShippingTotal_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.HideShippingTotal, storeId);
                shippingSettingsModel.BypassShippingMethodSelectionIfOnlyOne_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.BypassShippingMethodSelectionIfOnlyOne, storeId);
                shippingSettingsModel.ConsiderAssociatedProductsDimensions_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.ConsiderAssociatedProductsDimensions, storeId);
                shippingSettingsModel.ShippingOriginAddress_OverrideForStore = await _settingService.SettingExistsAsync(shippingSettings, x => x.ShippingOriginAddressId, storeId);
            }

            var pickupAddressSettings = await _settingService.LoadSettingAsync<PickupAddressSettings>(storeId);

            var pickupAddressSettingsModel = new PickupAddressSettingsModel
            {
                //set some of address fields as enabled and required
                FirstNameRequired = pickupAddressSettings.FirstNameRequired,
                LastNameRequired = pickupAddressSettings.LastNameRequired,
                EmailRequired = pickupAddressSettings.EmailRequired,
                CompanyRequired = pickupAddressSettings.CompanyRequired,
                CityRequired = pickupAddressSettings.CityRequired,
                CountryRequired = pickupAddressSettings.CountryRequired,
                StateProvinceEnabled = pickupAddressSettings.StateProvinceEnabled,
                CountyRequired = pickupAddressSettings.CountyRequired,
                StreetAddressRequired = pickupAddressSettings.StreetAddressRequired,
                StreetAddress2Required = pickupAddressSettings.StreetAddress2Required,
                ZipPostalCodeRequired = pickupAddressSettings.ZipPostalCodeRequired,
                PhoneRequired = pickupAddressSettings.PhoneRequired,
                FaxRequired = pickupAddressSettings.FaxRequired
            };
            var model = new ShippingSettingsModelExtension(shippingSettingsModel);

            var shippingPickupAddressSettingsModel = new ShippingPickupAddressSettingsModel
            {
                ActiveStoreScopeConfiguration = storeId,
                PickupAddressSettingsModel = pickupAddressSettingsModel
            };
            await _addressAttributeModelFactory.PrepareAddressAttributeSearchModelAsync(shippingPickupAddressSettingsModel.AddressAttributeSearchModel);

            model.ShippingPickupAddressSettingsModel = shippingPickupAddressSettingsModel;

            //prepare shipping origin address
            var originAddress = await _addressService.GetAddressByIdAsync(shippingSettings.ShippingOriginAddressId);
            if (originAddress != null)
                shippingSettingsModel.ShippingOriginAddress = originAddress.ToModel(shippingSettingsModel.ShippingOriginAddress);
            await PrepareAddressModelAsync(originAddress, shippingSettingsModel.ShippingOriginAddress);

            return model;
        }

        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address</param>
        public virtual async Task<AddressModel> PrepareAddressModelAsync(Address address, AddressModel model = null)
        {
            address ??= new Address();

            if (model == null)
                model = new AddressModel();

            //load settings for a chosen store scope
            var storeId = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var pickupAddressSettings = await _settingService.LoadSettingAsync<PickupAddressSettings>(storeId);

            //set some of address fields as enabled and required
            model.FirstNameRequired = pickupAddressSettings.FirstNameRequired;
            model.LastNameRequired = pickupAddressSettings.LastNameRequired;
            model.EmailRequired = pickupAddressSettings.EmailRequired;
            model.CompanyRequired = pickupAddressSettings.CompanyRequired;
            model.CityRequired = pickupAddressSettings.CityRequired;
            model.CountyRequired = pickupAddressSettings.CountyRequired;
            model.CountryRequired = pickupAddressSettings.CountryRequired;
            model.StreetAddressRequired = pickupAddressSettings.StreetAddressRequired;
            model.StreetAddress2Required = pickupAddressSettings.StreetAddress2Required;
            model.ZipPostalCodeRequired = pickupAddressSettings.ZipPostalCodeRequired;
            model.PhoneRequired = pickupAddressSettings.PhoneRequired;
            model.FaxRequired = pickupAddressSettings.FaxRequired;

            //field values
            model.Id = address.Id;
            model.FirstName = address.FirstName;
            model.LastName = address.LastName;
            model.Email = address.Email;
            model.Company = address.Company;
            model.CountryId = address.CountryId;
            model.StateProvinceId = address.StateProvinceId;
            model.StateProvinceName = (await _stateProvinceService.GetStateProvinceByAddressAsync(address))?.Name ?? string.Empty;
            model.City = address.City;
            model.County = address.County;
            model.Address1 = address.Address1;
            model.Address2 = address.Address2;
            model.ZipPostalCode = address.ZipPostalCode;
            model.PhoneNumber = address.PhoneNumber;
            model.FaxNumber = address.FaxNumber;

            //prepare available countries
            await _baseAdminModelFactory.PrepareCountriesAsync(model.AvailableCountries);

            //prepare available states
            await _baseAdminModelFactory.PrepareStatesAndProvincesAsync(model.AvailableStates, model.CountryId);

            //prepare custom address attributes
            await _addressAttributeModelFactory.PrepareCustomAddressAttributesAsync(model.CustomAddressAttributes, address);

            return model;
        }
    }
}
