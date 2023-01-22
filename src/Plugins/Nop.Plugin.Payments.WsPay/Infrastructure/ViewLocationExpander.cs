using Microsoft.AspNetCore.Mvc.Razor;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Payments.WsPay.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";


        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var themeApplied = context.Values.TryGetValue(THEME_KEY, out string theme);

            if(themeApplied && theme != "DefaultClean")
            {
                if (context.AreaName == null && context.ControllerName.ToLower().Contains("checkout"))
                {
                    viewLocations = new string[] {
                            $"~/Themes/{theme}/Views/Checkout/{{0}}.cshtml",
                    }.Concat(viewLocations);
                }
            }
            else
            {
                if (context.AreaName == null && context.ControllerName.ToLower().Contains("checkout"))
                {
                    viewLocations = new string[] {
                            $"~/Views/Checkout/{{0}}.cshtml",
                    }.Concat(viewLocations);
                }
            }

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {


        }
    }
}
