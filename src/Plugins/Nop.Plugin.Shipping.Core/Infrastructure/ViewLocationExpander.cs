using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Razor;

namespace Nop.Plugin.Shipping.Core.Infrastructure
{
    public class ViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "nop.themename";

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.AreaName == "Admin" && (context.ControllerName == "Order" || context.ControllerName == "CustomOrder"))
            {
                viewLocations = new string[]
                {
                        $"Areas/Admin/Views/Order/{{0}}.cshtml",
                }.Concat(viewLocations);
            }

            if (context.AreaName == "Admin" && (context.ControllerName == "Customer" || context.ControllerName == "CustomCustomer") && context.ViewName == "_CreateOrUpdate.Info")
            {
                viewLocations = new string[]
                {
                        $"/Plugins/Shipping.Core/Areas/Admin/Views/CustomCustomer/_CreateOrUpdate.Info.cshtml",
                }.Concat(viewLocations);
            }


            viewLocations = new string[] {
                            $"/Plugins/Shipping.Core/Areas/Admin/Views/CustomShipping/{{0}}.cshtml",
                            $"/Plugins/Shipping.Core/Areas/Admin/Views/ShippingManagement/{{0}}.cshtml",
                }.Concat(viewLocations);

            return viewLocations;
        }

        public void PopulateValues(ViewLocationExpanderContext context)
        {
            
        }
    }
}
