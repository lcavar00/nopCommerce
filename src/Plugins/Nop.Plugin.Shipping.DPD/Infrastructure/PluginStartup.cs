using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Shipping.DPD.Services;

namespace Nop.Plugin.Shipping.DPD.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        public int Order => 0;

        public void Configure(IApplicationBuilder application)
        {
            
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IDPDApiService, DPDApiService>();
        }
    }
}
