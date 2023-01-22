using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.ImportExport.Core.Extensions;
using Nop.Plugin.ImportExport.Core.Services;

namespace Nop.Plugin.ImportExport.Core.Infrastructure
{
    /// <summary>
    /// Represents object for the configuring services on application startup
    /// </summary>
    public class NopStartup : INopStartup
    {
        /// <summary>
        /// Gets order of this startup configuration implementation
        /// </summary>
        public int Order => 100000;

        /// <summary>
        /// Add and configure any of the middleware
        /// </summary>
        /// <param name="services">Collection of service descriptors</param>
        /// <param name="configuration">Configuration of the application</param>
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IForeignDataProviderManager, ForeignDataProviderManager>();
            services.AddTransient(serviceProvider =>
                serviceProvider.GetRequiredService<IForeignDataProviderManager>().DataProvider);

            services.AddScoped(typeof(IForeignRepository<>), typeof(ForeignEntityRepository<>));

            services.AddScoped<IDbConnectionService, DbConnectionService>();
            services.AddScoped<IDataEntryMappingService, DataEntryMappingService>();
            services.AddScoped<IEntityMappingService, EntityMappingService>();
            services.AddScoped<INopDataProviderExtension, MsSqlNopDataProviderExtension>();
            services.AddScoped<ISyncronisationMetaDataService, SyncronisationMetaDataService>();
        }

        /// <summary>
        /// Configure the using of added middleware
        /// </summary>
        /// <param name="application">Builder for configuring an application's request pipeline</param>
        public void Configure(IApplicationBuilder application)
        {
            
        }
    }
}
