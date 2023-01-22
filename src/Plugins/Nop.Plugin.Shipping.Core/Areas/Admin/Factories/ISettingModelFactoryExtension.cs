using System.Threading.Tasks;
using Nop.Plugin.Shipping.Core.Areas.Admin.Models;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Factories
{
    public interface ISettingModelFactoryExtension : ISettingModelFactory
    {
        Task<ShippingSettingsModelExtension> PrepareShippingSettingsExtensionModelAsync();
    }
}