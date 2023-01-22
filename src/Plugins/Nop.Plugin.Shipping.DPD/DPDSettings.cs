using Nop.Plugin.Shipping.Core;

namespace Nop.Plugin.Shipping.DPD
{
    public class DPDSettings : ShippingRateComputationSettings
    {
        public bool UseSandbox { get; set; }
        public string WebServiceUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string TestWebServiceUrl { get; set; }
        public string TestUsername { get; set; }
        public string TestPassword { get; set; }
    }
}