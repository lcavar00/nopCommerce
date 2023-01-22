using Nop.Plugin.Misc.MorePrices.Areas.Admin.Models;
using Nop.Web.Areas.Admin.Factories;

namespace Nop.Plugin.Misc.MorePrices.Areas.Admin.Extensions
{
    public interface ISettingModelFactoryExtension : ISettingModelFactory
    {
        PriceEditorSettingsModel PreparePriceEditorSettingsModel();
    }
}