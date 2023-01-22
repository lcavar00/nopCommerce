using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.WsPay.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WsPay.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }
        public bool UseSandbox_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WsPay.Fields.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WsPay.Fields.SecretKeyTest")]
        public string SecretKeyTest { get; set; }
        public bool SecretKeyTest_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WsPay.Fields.PaymentUrl")]
        public string PaymentUrl { get; set; }
        public bool PaymentUrl_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WsPay.Fields.PaymentUrlTest")]
        public string PaymentUrlTest { get; set; }
        public bool PaymentUrlTest_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WsPay.Fields.ShopId")]
        public string ShopId { get; set; }
        public bool ShopId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WsPay.Fields.ShopIdTest")]
        public string ShopIdTest { get; set; }
        public bool ShopIdTest_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WsPay.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WsPay.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WsPay.Fields.PassProductNamesAndTotals")]
        public bool PassProductNamesAndTotals { get; set; }
        public bool PassProductNamesAndTotals_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WsPay.Fields.ReturnFromWsPayWithoutPaymentRedirectsToOrderDetailsPage")]
        public bool ReturnFromWsPayWithoutPaymentRedirectsToOrderDetailsPage { get; set; }
        public bool ReturnFromWsPayWithoutPaymentRedirectsToOrderDetailsPage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.WsPay.Fields.CallbackUrl")]
        public string CallbackUrl { get; set; }
    }
}
