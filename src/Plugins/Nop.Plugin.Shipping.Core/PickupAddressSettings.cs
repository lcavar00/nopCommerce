using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.Core
{
    public class PickupAddressSettings : ISettings
    {
        public bool FirstNameRequired { get; set; }
        public bool LastNameRequired { get; set; }
        public bool EmailRequired { get; set; }
        public bool PhoneRequired { get; set; }
        public bool CompanyRequired { get; set; }
        public bool FaxRequired { get; set; }
        public bool CountryRequired { get; set; }
        public bool StateProvinceEnabled { get; set; }
        public bool CountyRequired { get; set; }
        public bool CityRequired { get; set; }
        public bool StreetAddressRequired { get; set; }
        public bool StreetAddress2Required { get; set; }
        public bool ZipPostalCodeRequired { get; set; }
    }
}
