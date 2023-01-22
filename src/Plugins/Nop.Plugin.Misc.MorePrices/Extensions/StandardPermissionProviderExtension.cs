using Nop.Core.Domain.Security;
using Nop.Services.Security;

namespace Nop.Plugin.Misc.MorePrices.Extensions
{
    public partial class StandardPermissionProviderExtension : StandardPermissionProvider
    {
        public static readonly PermissionRecord ManagePrices = new PermissionRecord { Name = "Admin area. Manage Prices", SystemName = "ManagePrices", Category = "Catalog" };

    }
}
