using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nop.Services.Plugins;

namespace Nop.Plugin.Photography.CMS
{
    public class CMSPlugin : BasePlugin
    {
        public CMSPlugin()
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
