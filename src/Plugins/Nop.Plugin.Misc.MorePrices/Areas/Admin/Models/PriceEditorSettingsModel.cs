using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.MorePrices.Areas.Admin.Models
{
    /// <summary>
    /// Represents a product editor settings model
    /// </summary>
    public partial class PriceEditorSettingsModel : BaseNopModel, ISettingsModel
    {
        #region Properties

        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.PriceType")]
        public bool PriceType { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.AdminComment")]
        public bool AdminComment { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.Vendor")]
        public bool Vendor { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.Stores")]
        public bool Stores { get; set; }
        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.ACL")]
        public bool ACL { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.PriceCost")]
        public bool PriceCost { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.TierPrices")]
        public bool TierPrices { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.RecurringPrice")]
        public bool RecurringPrice { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.IsRental")]
        public bool IsRental { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.UseMultipleWarehouses")]
        public bool UseMultipleWarehouses { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.Warehouse")]
        public bool Warehouse { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.Published")]
        public bool Published { get; set; }

        [NopResourceDisplayName("Admin.Configuration.Settings.PriceEditor.Manufacturers")]
        public bool Manufacturers { get; set; }

        #endregion
    }
}
