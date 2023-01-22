using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.ReCaching.Services;

namespace Nop.Plugin.Misc.ReCaching.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        public int Order => int.MaxValue;

        public void Configure(IApplicationBuilder application)
        {
            
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(ICacheClearingService<Product>), typeof(ProductCacheClearingService));
        }
    }
}
