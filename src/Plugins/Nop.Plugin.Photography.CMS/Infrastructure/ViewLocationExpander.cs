using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Photography.CMS.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if(context.ControllerName == "Home" && context.ViewName == "Index")
            {
                viewLocations = new string[] {
                    "~/Plugins/Photography.CMS/Views/Home/Index.cshtml",
                }.Concat(viewLocations);
            }

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            
        }
    }
}
