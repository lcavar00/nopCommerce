using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Models
{
    public record ShippingPickupAddressSettingsModel : BaseNopModel, ISettingsModel
    {
        #region Ctor

        public ShippingPickupAddressSettingsModel()
        {
            PickupAddressSettingsModel = new PickupAddressSettingsModel();
            AddressAttributeSearchModel = new AddressAttributeSearchModel();
        }

        #endregion

        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }
        public PickupAddressSettingsModel PickupAddressSettingsModel { get; set; }
        public AddressAttributeSearchModel AddressAttributeSearchModel { get; set; }

        #endregion
    }
}
