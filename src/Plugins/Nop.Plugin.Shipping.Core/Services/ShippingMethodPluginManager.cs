using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Plugins;
using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.Core.Services
{
    public class ShippingMethodPluginManager : PluginManager<IShippingProviderPlugin>, IShippingMethodPluginManager
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly ShippingSettings _shippingSettings;

        #endregion

        #region Ctor

        public ShippingMethodPluginManager(ICustomerService customerService,
            IPluginService pluginService,
            ISettingService settingService,
            IShippingPluginManager shippingPluginManager,
            ShippingSettings shippingSettings) : base(customerService, pluginService)
        {
            _settingService = settingService;
            _shippingPluginManager = shippingPluginManager;
            _shippingSettings = shippingSettings;
        }

        #endregion

        #region Methods

        public async Task<IList<IShippingProviderPlugin>> LoadActivePluginsAsync(Customer customer = null, int storeId = 0, string systemName = null)
        {
            var shippingRateComputationMethods = await _shippingPluginManager.LoadActivePluginsAsync(_shippingSettings.ActiveShippingRateComputationMethodSystemNames, customer, storeId);

            await ComplementShippingProviderMethodSystemNamesAsync(shippingRateComputationMethods);
            var shippingMethodProviders = await LoadActivePluginsAsync(_shippingSettings.ActiveShippingRateComputationMethodSystemNames, customer, storeId);

            //filter by passed system name
            if (!string.IsNullOrEmpty(systemName))
            {
                shippingMethodProviders = shippingMethodProviders
                    .Where(provider => provider.PluginDescriptor.SystemName.Equals(systemName, StringComparison.InvariantCultureIgnoreCase))
                    .ToList();
            }

            return shippingMethodProviders;
        }

        public bool IsPluginActive(IShippingProviderPlugin shippingProvider)
        {
            return IsPluginActive(shippingProvider, _shippingSettings.ActiveShippingRateComputationMethodSystemNames);
        }

        public async Task<bool> IsPluginActive(string systemName, Customer customer = null, int storeId = 0)
        {
            var shippingProvider = await LoadPluginBySystemNameAsync(systemName, customer, storeId);
            return IsPluginActive(shippingProvider);
        }

        #endregion

        #region Utilities

        private async Task<IList<string>> ComplementShippingProviderMethodSystemNamesAsync(IList<IShippingRateComputationMethod> shippingRateComputationMethods)
        {
            shippingRateComputationMethods = shippingRateComputationMethods ?? await _shippingPluginManager.LoadActivePluginsAsync(_shippingSettings.ActiveShippingRateComputationMethodSystemNames);

            foreach (var shippingRateComputationMethod in shippingRateComputationMethods)
            {
                if (shippingRateComputationMethod is IShippingProviderPlugin && 
                    !_shippingSettings.ActiveShippingRateComputationMethodSystemNames.Contains(shippingRateComputationMethod.PluginDescriptor.SystemName))
                    _shippingSettings.ActiveShippingRateComputationMethodSystemNames.Add(shippingRateComputationMethod.PluginDescriptor.SystemName);
            }

            await _settingService.SaveSettingAsync(_shippingSettings);

            return _shippingSettings.ActiveShippingRateComputationMethodSystemNames;
        }

        #endregion

    }
}
