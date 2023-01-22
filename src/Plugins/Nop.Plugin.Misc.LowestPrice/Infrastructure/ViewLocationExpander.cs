using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Misc.LowestPrice.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        public static string THEME_KEY = "nop.themename";

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var themeApplied = context.Values.TryGetValue(THEME_KEY, out var theme);

            viewLocations = new string[]
            {
                $"~/Plugins/Misc.LowestPrice/Views/Shared/{{0}}.cshtml",
            }.Concat(viewLocations);

            if (themeApplied)
            {
                viewLocations = new string[]
                {
                        $"~/Plugins/Misc.LowestPrice/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                }.Concat(viewLocations);
            }
            else
            {
                viewLocations = new string[]
                {
                    $"~/Plugins/Misc.LowestPrice/Views/Shared/{{0}}.cshtml",
                }.Concat(viewLocations);
            }
            

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {

        }
    }
}
