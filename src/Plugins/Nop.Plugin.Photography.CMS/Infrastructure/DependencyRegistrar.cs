using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Photography.CMS.Factories;

namespace Nop.Plugin.Photography.CMS.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => int.MaxValue;

        public void Register(IServiceCollection services, ITypeFinder typeFinder, AppSettings appSettings)
        {
            services.AddScoped<IGalleryModelFactory, GalleryModelFactory>();
        }
    }
}
