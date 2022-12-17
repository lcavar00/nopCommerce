using System.Threading.Tasks;
using Nop.Services.Plugins;

namespace Nop.Plugin.Sic.Sxc
{
    public class SxciPlugin : BasePlugin
    {
        public SxciPlugin() 
        { 
        
        }

        public override Task InstallAsync()
        {
            return base.InstallAsync();
        }

        public override Task UninstallAsync()
        {
            return base.UninstallAsync();
        }
    }
}
