using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Models
{
    public record PickupAddressSettingsModel : BaseNopModel, ISettingsModel
    {
        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FirstNameRequired")]
        public bool FirstNameRequired { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.LastNameRequired")]
        public bool LastNameRequired { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.EmailRequired")]
        public bool EmailRequired { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CompanyRequired")]
        public bool CompanyRequired { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddressRequired")]
        public bool StreetAddressRequired { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StreetAddress2Required")]
        public bool StreetAddress2Required { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.ZipPostalCodeRequired")]
        public bool ZipPostalCodeRequired { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CityRequired")]
        public bool CityRequired { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountyRequired")]
        public bool CountyRequired { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.CountryRequired")]
        public bool CountryRequired { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.StateProvinceRequired")]
        public bool StateProvinceEnabled { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.PhoneRequired")]
        public bool PhoneRequired { get; set; }


        [NopResourceDisplayName("Admin.Configuration.Settings.CustomerUser.AddressFormFields.FaxRequired")]
        public bool FaxRequired { get; set; }

        #endregion
    }
}
