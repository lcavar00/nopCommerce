using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.Core.Models
{
    public record FixedRateModel : BaseNopModel
    {
        public int ShippingMethodId { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.FixedByWeightByTotal.Fields.ShippingMethod")]
        public string ShippingMethodName { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.FixedByWeightByTotal.Fields.Rate")]
        public decimal Rate { get; set; }
    }
}
