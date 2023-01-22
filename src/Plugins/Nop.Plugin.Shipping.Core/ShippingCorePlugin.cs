using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Security;
using Nop.Data;
using Nop.Services.Cms;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Shipping.Core
{
    public class ShippingCorePlugin : BasePlugin, IMiscPlugin, IWidgetPlugin
    {
		#region Fields
		
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly IRepository<PermissionRecord> _permissionRecordRepository;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public ShippingCorePlugin(ILocalizationService localizationService,
            IPermissionService permissionService,
            IRepository<PermissionRecord> permissionRecordRepository,
            ISettingService settingService)
        {
            _localizationService = localizationService;
            _permissionService = permissionService;
            _permissionRecordRepository = permissionRecordRepository;
            _settingService = settingService;
        }

        #endregion

        public override async Task InstallAsync()
        {
            await AddSettingsAsync();
            await AddLocalizationAsync();
            await AddPermissionsAsync();
            await UpdateAddressSettingsAsync();
            await UpdateAddressLocalizationAsync();
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await DeleteSettingsAsync();
            await DeleteLocalizationAsync();
            await DeletePermissionsAsync();
            await base.UninstallAsync();
        }

        public bool HideInWidgetList => true;

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "SendMultipleShipmentRequests";
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
                {
                    AdminWidgetZones.OrderListButtons,
                }
            );
        }

        private async Task AddPermissionsAsync()
        {
            var managePacakgePickupLocationsPermission = new PermissionRecord
            {
                Name = "Admin area. Manage Package Pickup Locations",
                SystemName = "ManagePackagePickupLocations",
                Category = "Shipping",
            };

            await _permissionRecordRepository.InsertAsync(managePacakgePickupLocationsPermission);
        }

        private async Task DeletePermissionsAsync()
        {
            var managePackagePickupLocationsPaermission = (await _permissionService.GetAllPermissionRecordsAsync()).FirstOrDefault(a => a.SystemName == "ManagePackagePickupLocations");
            if (managePackagePickupLocationsPaermission != null)
            {
                await _permissionRecordRepository.DeleteAsync(managePackagePickupLocationsPaermission);
            }
        }

        private async Task AddSettingsAsync()
        {
            var shippingPickupAddressSettings = await _settingService.LoadSettingAsync<PickupAddressSettings>();
            if(shippingPickupAddressSettings == null)
            {
                shippingPickupAddressSettings = new PickupAddressSettings
                {
                    CityRequired = true,
                    CountryRequired = true,
                    CountyRequired = false,
                    EmailRequired = true,
                    FaxRequired = false,
                    FirstNameRequired = true,
                    LastNameRequired = true,
                    PhoneRequired = true,
                    StateProvinceEnabled = false,
                    StreetAddressRequired = true,
                    StreetAddress2Required = false,
                    ZipPostalCodeRequired = true,
                };
                
                await _settingService.SaveSettingAsync(shippingPickupAddressSettings);
            }
        }

        private async Task DeleteSettingsAsync()
        {
            var shippingPickupAddressSettings = await _settingService.LoadSettingAsync<PickupAddressSettings>();
            if (shippingPickupAddressSettings == null)
            {
                await _settingService.DeleteSettingAsync<PickupAddressSettings>();
            }

            var shippingProviderSettings = await _settingService.LoadSettingAsync<ShippingRateComputationSettings>();
            if (shippingProviderSettings == null)
            {
                await _settingService.DeleteSettingAsync<ShippingRateComputationSettings>();
            }
        }

        /// <summary>
        /// We're going to make address2 setting enabled and required (and use it as house number)
        /// </summary>
        private async Task UpdateAddressSettingsAsync()
        {
            var addressSetting = await _settingService.LoadSettingAsync<AddressSettings>();
            addressSetting.StreetAddress2Enabled = true;
            addressSetting.StreetAddress2Required = true;
            await _settingService.SaveSettingAsync(addressSetting);
        }

        private async Task AddLocalizationAsync()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.ShippingByTotal", "By total", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.ShippingByTotal", "By total", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.ShippingByTotal", "Po ukupnoj cijeni", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Checkout.ExistingAddresses.NoValidAddresses", "Your addresses don't have a house number value. Please go to your profile and update them, or input a new address.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Checkout.ExistingAddresses.NoValidAddresses", "Your addresses don't have a house number value. Please go to your profile and update them, or input a new address.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Checkout.ExistingAddresses.NoValidAddresses", "Vaše adrese nemaju upisan kućni broj na potrebno predviđeno mjesto. Potrebno je ažurirati vaše adrese unutar vašeg korisničkog profila ili upisati novu.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Checkout.ExistingAddresses.SomeInvalidAddresses", "You have addresses don't have a house number value. Please go to your profile and update them, or input a new address.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Checkout.ExistingAddresses.SomeInvalidAddresses", "You have addresses don't have a house number value. Please go to your profile and update them, or input a new address.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Checkout.ExistingAddresses.SomeInvalidAddresses", "Postoje adrese koje nemaju upisan kućni broj na potrebno predviđeno mjesto, stoga ovdje nisu ponuđene. Možete ih ažurirati u svom korisničkom profilu.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.DefaultPickupAddress", "Default pickup address", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.DefaultPickupAddress", "Default pickup address", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.DefaultPickupAddress", "Zadana adresa prikupa", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.PrintLabels", "Print Labels", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.PrintLabels", "Print Labels", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.PrintLabels", "Ispiši adresnice", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.Cancel", "Cancel", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.Cancel", "Cancel", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.Cancel", "Odustani", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address1HouseNumber", "House number", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address1HouseNumber", "House number", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address1HouseNumber", "Kućni broj", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address1HouseNumber.Hint", "Enter House number", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address1HouseNumber.Hint", "Enter House number", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address1HouseNumber.Hint", "Unesi Kućni broj", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address2HouseNumber", "House number 2", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address2HouseNumber", "House number 2", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address2HouseNumber", "Kućni broj 2", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address2HouseNumber.Hint", "Enter House number 2", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address2HouseNumber.Hint", "Enter House number 2", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address2HouseNumber.Hint", "Unesi Kućni broj 2", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.Title", "Shipping Management", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.Title", "Shipping Management", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.Title", "Upravljanje dostavom", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.BlockTitle.DefaultAddress", "Default package pickup address", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.BlockTitle.DefaultAddress", "Default package pickup address", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.BlockTitle.DefaultAddress", "Zadana adresa prikupa paketa", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.BlockTitle.AddressSettings", "Pickup address fields settings", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.BlockTitle.AddressSettings", "Pickup address fields settings", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.BlockTitle.AddressSettings", "Postavke polja adrese prikupa", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Name", "Shipping core plugin");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddRecord", "Add record", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddRecord", "Add record", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.AddRecord", "Dodaj zapis", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Formula", "Formula to calculate rates:", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Formula", "Formula to calculate rates:", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Formula", "Formula za računanje rata:", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Formula.Value", "[additional fixed cost] + ([order total weight] - [lower weight limit]) * [rate per weight unit] + [order subtotal] * [charge percentage]", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Formula.Value", "[additional fixed cost] + ([order total weight] - [lower weight limit]) * [rate per weight unit] + [order subtotal] * [charge percentage]", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Formula.Value", "[dodatna fiksna cijena] + ([ukupna težina narudžbe] - [niži limit težine]) * [rata po jedinici težine] + [cijena narudžbe] * [postotak naplate]", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.AdditionalFixedCost", "Additional fixed cost", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.AdditionalFixedCost", "Additional fixed cost", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.AdditionalFixedCost", "Dodatna fiksna cijena", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.AdditionalFixedCost.Hint", "Specify an additional fixed cost per shopping cart for this option. Set to 0 if you don't want an additional fixed cost to be applied.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.AdditionalFixedCost.Hint", "Specify an additional fixed cost per shopping cart for this option. Set to 0 if you don't want an additional fixed cost to be applied.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.AdditionalFixedCost.Hint", "Odredi dodatnu fiksnu cijenu po košarici za ovu opciju. Stavi 0 ako se ne primjenjuje dodatna fiksna cijena.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Country", "Country", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Country", "Country", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Country", "Država", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Country.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers, regardless of the country.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Country.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers, regardless of the country.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Country.Hint", "Ako je odabrana zvijezdica, onda će se ova rata aplicirati na sve korisnike, neovisno o državi.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.DataHtml", "Data", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.DataHtml", "Data", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.DataHtml", "Podaci", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.LimitMethodsToCreated", "Limit shipping methods to configured ones", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.LimitMethodsToCreated", "Limit shipping methods to configured ones", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.LimitMethodsToCreated", "Ograniči na konfigurirane metode dostave", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.LimitMethodsToCreated.Hint", "If you check this option, then your customers will be limited to shipping options configured here. Otherwise, they'll be able to choose any existing shipping options even they are not configured here (zero shipping fee in this case).", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.LimitMethodsToCreated.Hint", "If you check this option, then your customers will be limited to shipping options configured here. Otherwise, they'll be able to choose any existing shipping options even they are not configured here (zero shipping fee in this case).", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.LimitMethodsToCreated.Hint", "Ako je odabrana ova opcija, onda će kupci biti ograničeni na opcije dostave konfigurirane ovdje. U suprotnom, moći će odabrati bilo koju postojeću opciju dostave čak ako nisu ovdje konfigurirane (u ovom slučaju bez cijene dostave)", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalFrom", "Order subtotal from", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalFrom", "Order subtotal from", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalFrom", "Od cijene narudžbe", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.WeightFrom", "Package weight from", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.WeightFrom", "Pacakge weight from", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.WeightFrom", "Od težine paketa", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.WeightFrom.Hint", "Order weight from", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.WeightFrom.Hint", "Order weight from", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.WeightFrom.Hint", "Od težine paketa", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.WeightTo", "Order weight to", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.WeightTo", "Order weight to", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.WeightTo", "Do težine paketa", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.WeightTo.Hint", "Order weight to", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.WeightTo.Hint", "Order weight to", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.WeightTo.Hint", "Do težine paketa", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.RatePerWeightUnit", "Rate per weight unit", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.RatePerWeightUnit", "Rate per weight unit", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.RatePerWeightUnit", "Rata po jedinici težine", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.LowerWeightLimit", "Lower weight limit", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.LowerWeightLimit", "Lower weight limit", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.LowerWeightLimit", "Donja granica težine", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.LowerWeightLimit.Hint", "Lower weight limit. This field can be used for \"per extra weight unit\" scenarios.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.LowerWeightLimit.Hint", "Lower weight limit. This field can be used for \"per extra weight unit\" scenarios.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.LowerWeightLimit.Hint", "Donja granica težine. Ovo polje se može koristiti u slučajevima \"po dodatnoj jedinici težine\"", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalFrom.Hint", "Order subtotal from.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalFrom.Hint", "Order subtotal from.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalFrom.Hint", "Od cijene narudžbe.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalTo", "Order subtotal to", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalTo", "Order subtotal to", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalTo", "Do cijene narudžbe", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalTo.Hint", "Order subtotal to.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalTo.Hint", "Order subtotal to.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalTo.Hint", "Do cijene narudžbe.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.PercentageRateOfSubtotal", "Charge percentage (of subtotal)", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.PercentageRateOfSubtotal", "Charge percentage (of subtotal)", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.PercentageRateOfSubtotal", "Postotak naplate (od cijene)", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.PercentageRateOfSubtotal.Hint", "Charge percentage (of subtotal).", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.PercentageRateOfSubtotal.Hint", "Charge percentage (of subtotal).", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.PercentageRateOfSubtotal.Hint", "Postotak naplate (od cijene).", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Rate", "Rate", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Rate", "Rate", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Rate", "Rata", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.ShippingMethod", "Shipping method", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.ShippingMethod", "Shipping method", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.ShippingMethod", "Metoda dostave", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.ShippingMethod.Hint", "Choose shipping method.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.ShippingMethod.Hint", "Choose shipping method.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.ShippingMethod.Hint", "Odaberi metodu dostave.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.StateProvince", "State / province", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.StateProvince", "State / province", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.StateProvince", "Županija / provincija", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.StateProvince.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers from the given country, regardless of the state.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.StateProvince.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers from the given country, regardless of the state.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.StateProvince.Hint", "Ako je odabrana zvijezdica, onda će ova rata dostave biti aplicirana na sve provincije.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Store", "Store", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Store", "Store", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Store", "Trgovina", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Store.Hint", "If an asterisk is selected, then this shipping rate will apply to all stores.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Store.Hint", "If an asterisk is selected, then this shipping rate will apply to all stores.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Store.Hint", "Ako je odabrana zvijezdica, onda će ova rata dostave biti aplicirana na sve trgovine.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Warehouse", "Warehouse", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Warehouse", "Warehouse", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Warehouse", "Skladište", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Warehouse.Hint", "If an asterisk is selected, then this shipping rate will apply to all warehouses.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Warehouse.Hint", "If an asterisk is selected, then this shipping rate will apply to all warehouses.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Warehouse.Hint", "Ako je zvijezdica odabrana, onda će ova rata dostave biti aplicirana na sva skladišta.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Zip", "Zip", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Zip", "Zip", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Zip", "Poštanski broj", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Zip.Hint", "Zip / postal code. If zip is empty, then this shipping rate will apply to all customers from the given country or state, regardless of the zip code.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Zip.Hint", "Zip / postal code. If zip is empty, then this shipping rate will apply to all customers from the given country or state, regardless of the zip code.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fields.Zip.Hint", "Zip / poštanski broj. Ako je poštanski broj prazan, onda će se ova rata dostave aplicirati na sve kupce iz navedene države, neovisno o poštanskom broju.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fixed", "Fixed Rate", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fixed", "Fixed Rate", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Fixed", "Fiksna Rata", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FirstNameRequired", "'First Name' required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FirstNameRequired", "'First Name' required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FirstNameRequired", "'Ime' potrebno", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FirstNameRequired.Hint", "Set if 'First Name' is required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FirstNameRequired.Hint", "Set if 'First Name' is required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FirstNameRequired.Hint", "Podesi je li 'Ime' potrebno", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.LastNameRequired", "'Last Name' required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.LastNameRequired", "'Last Name' required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.LastNameRequired", "'Prezime' potrebno", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.LastNameRequired.Hint", "Set if 'Last Name' is required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.LastNameRequired.Hint", "Set if 'Last Name' is required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.LastNameRequired.Hint", "Podesi je li 'Prezime' potrebno", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.EmailRequired", "'Email' required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.EmailRequired", "'Email' required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.EmailRequired", "'Email' potreban", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.EmailRequired.Hint", "Set if 'Email' is required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.EmailRequired.Hint", "Set if 'Email' is required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.EmailRequired.Hint", "Podesi je li 'Email' potreban", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CityRequired", "'Grad' potreban", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CityRequired.Hint", "Podesi je li 'Grad' potreban", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryRequired", "'Country' required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryRequired", "'Country' required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryRequired.Hint", "Set if 'Country' is required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryRequired.Hint", "Set if 'Country' is required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryRequired", "'Država' potrebna", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryRequired.Hint", "Podesi je li 'Država' potrebna", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddressRequired", "'Adresa 1' potrebna", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddressRequired.Hint", "Podesi je li 'Adresa 1' potrebna", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CompanyRequired", "'Tvrtka' potrebna", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CompanyRequired.Hint", "Podesi je li 'Tvrtka", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required", "'Adresa 2' potrebna", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required.Hint", "Podesi je li 'Adresa 2' potrebna", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.ZipPostalCodeRequired", "'Poštanski broj' potreban", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.ZipPostalCodeRequired.Hint", "Podesi je li 'Poštanski broj' potreban", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FaxRequired", "'Faks' potreban", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FaxRequired.Hint", "Podesi je li 'Faks' potreban", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountyRequired", "'Županija' potrebna", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountyRequired.Hint", "Podesi je li 'Županija' potrebna", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.Country", "Country", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.Country", "Country", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.Country", "Država", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.Country.Hint", "Select Country.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.Country.Hint", "Select Country.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.Country.Hint", "Odaberi Državu.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.StateProvince", "State / province", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.StateProvince", "State / province", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.StateProvince", "Provincija", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.StateProvince.Hint", "Select State / province.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.StateProvince.Hint", "Select State / province.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.StateProvince.Hint", "Odaberi Provinciju.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.SendShippingRequest", "Send shipping request", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.SendShippingRequest", "Send shipping request", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.SendShippingRequest", "Pošalji zahtijev za dostavu", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.CancelShipmentRequest", "Cancel shipment request", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.CancelShipmentRequest", "Cancel shipment request", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.CancelShipmentRequest", "Otkaži zahtijev za dostavu", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.GetShipmentStatus", "Get shipment status", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.GetShipmentStatus", "Get shipment status", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.GetShipmentStatus", "Dohvati status pošiljke", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceRequired", "'State / province' required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceRequired", "'State / province' required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceRequired", "'Provincija' potrebna", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceRequired.Hint", "Set if 'State / province' is required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceRequired.Hint", "Set if 'State / province' is required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceRequired.Hint", "Podesi je li 'Provincija' potrebna", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Shipping.ShippingOriginAddress.Error", "Shipping origin address error", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Shipping.ShippingOriginAddress.Error", "Shipping origin address error", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.Shipping.ShippingOriginAddress.Error", "Greška adrese podrijetla", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Warning.NoShippingMethods", "No shipping method configured", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Warning.NoShippingMethods", "No shipping method configured", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.Warning.NoShippingMethods", "Nema konfigurirane metode", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.ConfigureShippingMethods", "Please configure shipping methods for this shipping provider", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.ConfigureShippingMethods", "Please configure shipping methods for this shipping provider", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.Core.ConfigureShippingMethods", "Konfiguriraj metode za ovu dostavljačku službu", "hr-HR");
        }

        /// <summary>
        /// We're going to use Address 2 as house number field
        /// </summary>
        private async Task UpdateAddressLocalizationAsync()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.StreetAddress2", "House number", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.StreetAddress2", "House number", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.StreetAddress2", "Kućni broj", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.StreetAddress2.Required", "House number is required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.StreetAddress2.Required", "House number is required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.StreetAddress2.Required", "Kućni broj je obavezan", "hr-HR");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Address.Fields.Address2", "House number", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Address.Fields.Address2", "House number", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Address.Fields.Address2", "Kućni broj", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Address.Fields.Address2.Required", "House number is required.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Address.Fields.Address2.Required", "House number is required.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Address.Fields.Address2.Required", "Kućni broj je obavezan.", "hr-HR");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Address2", "House number", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Address2", "House number", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Address2", "Kućni broj", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Address2.Required", "House number is required.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Address2.Required", "House number is required.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Account.Fields.Address2.Required", "Kućni broj je obavezan.", "hr-HR");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Address.Fields.Address2", "House number", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Address.Fields.Address2", "House number", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Address.Fields.Address2", "Kućni broj", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Address.Fields.Address2.Hint", "Enter house number.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Address.Fields.Address2.Hint", "Enter house number.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Address.Fields.Address2.Hint", "Unesite kućni broj.", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Address.Fields.Address2.Required", "House number is required.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Address.Fields.Address2.Required", "House number is required.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Address.Fields.Address2.Required", "Kućni broj je obavezan.", "hr-HR");

            await _localizationService.AddOrUpdateLocaleResourceAsync("PDFInvoice.Address2", "House number: {0}", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("PDFInvoice.Address2", "House number: {0}", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("PDFInvoice.Address2", "Kućni broj: {0}", "hr-HR");

            await _localizationService.AddOrUpdateLocaleResourceAsync("PDFPackagingSlip.Address2", "House number: {0}", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("PDFPackagingSlip.Address2", "House number: {0}", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("PDFPackagingSlip.Address2", "Kućni broj: {0}", "hr-HR");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Customers.Fields.StreetAddress2", "House number", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Customers.Fields.StreetAddress2", "House number", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Customers.Fields.StreetAddress2", "Kućni broj", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Customers.Fields.StreetAddress2.Hint", "Enter house number", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Customers.Fields.StreetAddress2.Hint", "Enter house number", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Customers.Fields.StreetAddress2.Hint", "Unesite kućni broj", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Customers.Fields.StreetAddress2.Required", "House number is required.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Customers.Fields.StreetAddress2.Required", "House number is required.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Customers.Customers.Fields.StreetAddress2.Required", "Kućni broj je obavezan.", "hr-HR");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Orders.Address.Address2", "House number", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Orders.Address.Address2", "House number", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Orders.Address.Address2", "Kućni broj", "hr-HR");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Enabled", "House number is enabled", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Enabled", "House number is enabled", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Enabled", "Kućni broj je omogućen", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Enabled.Hint", "Toggle whether 'House number' is enabled.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Enabled.Hint", "Toggle whether 'House number' is enabled.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Enabled.Hint", "Postavite ako je 'Kućni broj' omogućen.", "hr-HR");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required", "House number is required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required", "House number is required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required", "Kućni broj je obvezan", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required.Hint", "Toggle whether 'House number' is required.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required.Hint", "Toggle whether 'House number' is required.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required.Hint", "Postavite ako je 'Kućni broj' obvezan.", "hr-HR");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.StreetAddress2Enabled", "House number is enabled", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.StreetAddress2Enabled", "House number is enabled", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.StreetAddress2Enabled", "Kućni broj je omogućen", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.StreetAddress2Enabled.Hint", "Toggle whether 'House number' is enabled.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.StreetAddress2Enabled.Hint", "Toggle whether 'House number' is enabled.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.StreetAddress2Enabled.Hint", "Postavite ako je 'Kućni broj' omogućen.", "hr-HR");

            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.StreetAddress2Required", "House number is required", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.StreetAddress2Required", "House number is required", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.StreetAddress2Required", "Kućni broj je obvezan", "hr-HR");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.StreetAddress2Required.Hint", "Toggle whether 'House number' is required.", "en-US");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.StreetAddress2Required.Hint", "Toggle whether 'House number' is required.", "en-GB");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.StreetAddress2Required.Hint", "Postavite ako je 'Kućni broj' obvezan.", "hr-HR");
        }

        private async Task DeleteLocalizationAsync()
        {
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.ShippingByTotal");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Admin.DefaultPickupAddress");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Admin.PrintLabels");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Admin.Cancel");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address1HouseNumber");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address1HouseNumber.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address2HouseNumber");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.AddressAttribute.Address2HouseNumber.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugin.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.BlockTitle.DefaultAddress");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.BlockTitle.AddressSettings");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.Title");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Admin.Configuration.Settings.ShippingManagement.Title");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.AddRecord");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.AdditionalFixedCost");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.AdditionalFixedCost.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.Country");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.Country.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.DataHtml");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.LimitMethodsToCreated");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.LimitMethodsToCreated.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalFrom");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalFrom.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalTo");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.OrderSubtotalTo.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.PercentageRateOfSubtotal");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.PercentageRateOfSubtotal.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.Rate");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.ShippingMethod");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.ShippingMethod.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.StateProvince");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.StateProvince.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.Store");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.Store.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.Warehouse");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.Warehouse.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.Zip");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Fields.Zip.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.Country");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.Country.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.StateProvince");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.DefaultAddress.AddressFormFields.StateProvince.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.SendShippingRequest");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.CancelShipmentRequest");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.GetShipmentStatus");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceRequired");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceRequired.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Admin.Configuration.Settings.Shipping.ShippingOriginAddress.Error");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.Warning.NoShippingMethods");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.Core.ConfigureShippingMethods");
        }
    }
}
