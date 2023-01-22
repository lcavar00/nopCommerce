using System.Linq;
using Nop.Services.Common;
using Nop.Services.Plugins;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.ImportExport.Core
{
    public class ImportExportPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin
    {

        #region Ctor

        public ImportExportPlugin()
        {

        }

        #endregion

        public override async System.Threading.Tasks.Task InstallAsync()
        {
            await base.InstallAsync();
        }

        public override async System.Threading.Tasks.Task UninstallAsync()
        {
            await base.UninstallAsync();
        }


        public async System.Threading.Tasks.Task ManageSiteMapAsync(SiteMapNode rootNode)
        {
            var pluginRootNode = rootNode.ChildNodes.FirstOrDefault(a => a.SystemName == ImportExportDefaults.PluginSiteMapNodeSystemName);

            if(pluginRootNode == null)
            {
                pluginRootNode = new SiteMapNode
                {
                    Title = ImportExportDefaults.PluginSiteMapNodeName,
                    SystemName = ImportExportDefaults.PluginSiteMapNodeSystemName,
                    Visible = true,
                    IconClass = "nav-icon far fa-dot-circle",
                };

                rootNode.ChildNodes.Add(pluginRootNode);
            }

            var dbConnectionNode = new SiteMapNode
            {
                Title = ImportExportDefaults.PluginDbConnectionSiteMapNodeName,
                SystemName = ImportExportDefaults.PluginDbConnectionSiteMapNodeSystemName,
                Visible = true,
                ControllerName = "DbConnection",
                ActionName = "Index",
                IconClass = "nav-icon far fa-dot-circle",
            };

            pluginRootNode.ChildNodes.Add(dbConnectionNode);
        }
    }
}
