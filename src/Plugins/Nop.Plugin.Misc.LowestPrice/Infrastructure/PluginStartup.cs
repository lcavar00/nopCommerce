using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.LowestPrice.Extensions;
using Nop.Plugin.Misc.LowestPrice.Factories;
using Nop.Plugin.Misc.LowestPrice.Services;
using Nop.Services.Catalog;
using Nop.Web.Factories;

namespace Nop.Plugin.Misc.LowestPrice.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        public int Order => 20000;

        public void Configure(IApplicationBuilder application)
        {
            
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            services.AddScoped<IDiscountLogService, DiscountLogService>();
            services.AddScoped<ILowestPriceModelFactory, LowestPriceModelFactory>();
            services.AddScoped<ILowestPriceService, LowestPriceService>();
            services.AddScoped<IProductPriceHistoryService, ProductPriceHistoryService>();
            services.AddScoped<IProductAttributeCombinationPriceHistoryService, ProductAttributeCombinationPriceHistoryService>();

            //extensions
            //services.AddScoped<IProductModelFactory, ProductModelFactoryExtension>();
            //services.AddScoped<IProductService, ProductServiceExtension>();
            //services.AddScoped<IProductAttributeService, ProductAttributeServiceExtension>();
        }
    }
}
