using System;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Date;
using Nop.Services.Shipping.Pickup;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Shipping;
using Nop.Web.Framework.Factories;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Factories
{
    public class ShippingModelFactoryExtension : ShippingModelFactory, IShippingModelFactoryExtension
    {
        #region Fields

        private readonly IAddressAttributeModelFactory _addressAttributeModelFactory;
        private readonly IAddressModelFactory _addressModelFactory;
        private readonly IAddressService _addressService;
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly ISettingService _settingService;

        public ShippingModelFactoryExtension(IAddressAttributeModelFactory addressAttributeModelFactory,
            IAddressModelFactory addressModelFactory,
            IAddressService addressService, 
            IBaseAdminModelFactory baseAdminModelFactory,
            ICountryService countryService, 
            IDateRangeService dateRangeService, 
            ILocalizationService localizationService, 
            ILocalizedModelFactory localizedModelFactory, 
            IPickupPluginManager pickupPluginManager, 
            ISettingService settingService,
            IShippingPluginManager shippingPluginManager, 
            IShippingService shippingService, 
            IStateProvinceService stateProvinceService) : base(addressModelFactory, addressService, countryService, dateRangeService, localizationService, localizedModelFactory, pickupPluginManager, shippingPluginManager, shippingService, stateProvinceService)
        {
            _addressAttributeModelFactory = addressAttributeModelFactory;
            _addressModelFactory = addressModelFactory;
            _addressService = addressService;
            _addressModelFactory = addressModelFactory;
            _baseAdminModelFactory = baseAdminModelFactory;
            _settingService = settingService;
        }

        #endregion

        #region Ctor



        #endregion

        /// <summary>
        /// Prepare warehouse model
        /// </summary>
        /// <param name="model">Warehouse model</param>
        /// <param name="warehouse">Warehouse</param>
        /// <param name="excludeProperties">Whether to exclude populating of some properties of model</param>
        /// <returns>Warehouse model</returns>
        public override async Task<WarehouseModel> PrepareWarehouseModelAsync(WarehouseModel model, Warehouse warehouse, bool excludeProperties = false)
        {
            if (warehouse != null)
            {
                //fill in model values from the entity
                if (model == null)
                {
                    model = warehouse.ToModel<WarehouseModel>();
                }
            }

            var settings = await _settingService.LoadSettingAsync<PickupAddressSettings>();

            //prepare address model
            var address = await _addressService.GetAddressByIdAsync(warehouse?.AddressId ?? 0);

            if (!excludeProperties && address != null)
            {
                var addressModel = new AddressModel();
                await _addressModelFactory.PrepareAddressModelAsync(addressModel, address);

                //model.Address = address.ToModel(model.Address);
                model.Address = new AddressModel
                {
                    CityRequired = settings.CityRequired,
                    CompanyRequired = settings.CompanyRequired,
                    CountryRequired = settings.CountryRequired,
                    CountyRequired = settings.CountyRequired,
                    EmailRequired = settings.EmailRequired,
                    FaxRequired = settings.FaxRequired,
                    FirstNameRequired = settings.FirstNameRequired,
                    LastNameRequired = settings.LastNameRequired,
                    PhoneRequired = settings.PhoneRequired,
                    StreetAddress2Required = settings.StreetAddress2Required,
                    StreetAddressRequired = settings.StreetAddressRequired,
                    ZipPostalCodeRequired = settings.ZipPostalCodeRequired,
                    Address1 = addressModel.Address1,
                    Address2 = addressModel.Address2,
                    AvailableCountries = addressModel.AvailableCountries,
                    AvailableStates = addressModel.AvailableStates,
                    City = addressModel.City,
                    Company = addressModel.Company,
                    CountryId = addressModel.CountryId,
                    CountryName = addressModel.CountryName,
                    County = addressModel.County,
                    CustomProperties = addressModel.CustomProperties,
                    FirstName = addressModel.FirstName,
                    Email = addressModel.Email,
                    FaxNumber = addressModel.FaxNumber,
                    FormattedCustomAddressAttributes = addressModel.FormattedCustomAddressAttributes,
                    Id = addressModel.Id,
                    LastName = addressModel.LastName,
                    PhoneNumber = addressModel.PhoneNumber,
                    StateProvinceId = addressModel.StateProvinceId,
                    StateProvinceName = addressModel.StateProvinceName,
                    ZipPostalCode = addressModel.ZipPostalCode,
                };
            }

            PrepareWarehouseAddressModelAsync(model.Address, address);

            return model;
        }

        //public override PickupPointProviderModel 

        /// <summary>
        /// Prepare address model
        /// </summary>
        /// <param name="model">Address model</param>
        /// <param name="address">Address</param>
        private async void  PrepareWarehouseAddressModelAsync(AddressModel model, Address address)
        {
            var settings = await _settingService.LoadSettingAsync<PickupAddressSettings>();

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            //set some of address fields as enabled and required
            model.FirstNameRequired = settings.FirstNameRequired;
            model.LastNameRequired = settings.LastNameRequired;
            model.PhoneRequired = settings.PhoneRequired;
            model.EmailRequired = settings.EmailRequired;
            model.CountryRequired = settings.CountryRequired;
            model.CityRequired = settings.CityRequired;
            model.StreetAddressRequired = settings.StreetAddressRequired;
            model.StreetAddress2Required = settings.StreetAddress2Required;
            model.ZipPostalCodeRequired = settings.ZipPostalCodeRequired;

            //prepare available countries
            await _baseAdminModelFactory.PrepareCountriesAsync(model.AvailableCountries);

            //prepare available states
            await _baseAdminModelFactory.PrepareStatesAndProvincesAsync(model.AvailableStates, model.CountryId);

            //prepare custom address attributes
            await _addressAttributeModelFactory.PrepareCustomAddressAttributesAsync(model.CustomAddressAttributes, address);
        }
    }
}
