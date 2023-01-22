using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Shipping.Core.Areas.Admin.Factories;
using Nop.Plugin.Shipping.Core.Factories;
using Nop.Plugin.Shipping.Core.Services; 

namespace Nop.Plugin.Shipping.Core.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        public int Order => 200000;

        public void Configure(IApplicationBuilder application)
        {
            
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            services.AddScoped<IOrderModelFactoryExtension, OrderModelFactoryExtension>();
            services.AddScoped<ISettingModelFactoryExtension, SettingModelFactoryExtension>();
            services.AddScoped<IShipmentRequestModelFactory, ShipmentRequestModelFactory>();
            services.AddScoped<IShipmentRequestService, ShipmentRequestService>();
            services.AddScoped<IShippingAddressModelFactory, ShippingAddressModelFactory>();
            services.AddScoped<IShippingByLocationByTotalByWeightService, ShippingByLocationByTotalByWeightService>();
            services.AddScoped<IShippingMethodPluginManager, ShippingMethodPluginManager>();
            services.AddScoped<IShippingModelFactoryExtension, ShippingModelFactoryExtension>();
        }
    }
}
