using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Misc.MorePrices.Extensions;
using Nop.Services.Catalog;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.MorePrices.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => int.MaxValue;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<PriceFormatterExtension>().As<IPriceFormatter>().InstancePerLifetimeScope();
            builder.RegisterType<StandardPermissionProviderExtension>().As<IPermissionProvider>().InstancePerLifetimeScope();
        }
    }
}
