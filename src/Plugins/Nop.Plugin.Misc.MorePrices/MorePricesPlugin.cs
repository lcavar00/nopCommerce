using Nop.Core;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;
using System.Linq;

namespace Nop.Plugin.Misc.MorePrices
{
    public class MorePricesPlugin : BasePlugin, IAdminMenuPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        public MorePricesPlugin(ILocalizationService localizationService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _localizationService = localizationService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var pricesNode = new SiteMapNode
            {
                ActionName = "Index",
                ControllerName = "Prices",
                SystemName = "Prices",
                Visible = true,
                Title = "Prices",
            };

            var nodeTitle = _localizationService.GetLocaleStringResourceByName("Nop.Admin.Catalog", _workContext.WorkingLanguage.Id);

            var catalogNode = rootNode.ChildNodes.FirstOrDefault(a => a.Title == (nodeTitle?.ResourceValue ?? "Catalog"));
            catalogNode.ChildNodes.Add(pricesNode);
        }

        public override void Install()
        {
            base.Install();
        }

        public override void Uninstall()
        {
            base.Uninstall();
        }
    }
}
