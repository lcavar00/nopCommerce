using Nop.Core.Configuration;

namespace Nop.Plugin.Payments.WsPay
{
    public class WsPayPaymentSettings : ISettings
    {
        public bool UseSandbox { get; set; }
        public string BusinessEmail { get; set; }

        public string PaymentUrl { get; set; }
        public string PaymentUrlTest { get; set; }
        public string SecretKey { get; set; }
        public string SecretKeyTest { get; set; }

        public string ShopId { get; set; }
        public string ShopIdTest { get; set; }

        public bool PassProductNamesAndTotals { get; set; }
        public bool ReturnFromWsPayWithoutPaymentRedirectsToOrderDetailsPage { get; set; }
    }
}
