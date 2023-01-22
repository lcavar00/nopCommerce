using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.Photography.CMS.Infrastructure
{
    public class PluginStartup : INopStartup
    {
        public int Order => int.MaxValue;

        public void Configure(IApplicationBuilder application)
        {
            throw new NotImplementedException();
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ViewLocationExpander());
            });
        }
    }
}
