using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.Core
{
    /// <summary>
    /// Shipping provider settings
    /// </summary>
    public class ShippingRateComputationSettings : ISettings
    {
               /// <summary>
        /// Gets or sets a value indicating whether the "shipping by total by location" method type is selected
        /// </summary>
        public bool ShippingByLocationByTotalByWeightEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether only created shipping methods will be shown
        /// </summary>
        public bool LimitMethodsToCreated { get; set; }
    }
}
