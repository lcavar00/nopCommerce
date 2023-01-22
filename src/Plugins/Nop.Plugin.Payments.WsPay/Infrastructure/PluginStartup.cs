using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.WsPay.Factories;
using Nop.Plugin.Payments.WsPay.Services;

namespace Nop.Plugin.Payments.WsPay.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        public int Order => int.MaxValue - 1;

        public void Configure(IApplicationBuilder application)
        {

        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });

            services.AddScoped<IRatePaymentModelFactory, RatePaymentModelFactory>();
            services.AddScoped<IRatePaymentService, RatePaymentService>();
        }

    }
}
