using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Shipping.Core.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => int.MaxValue;

        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapControllerRoute("Plugin.Shipping.Core.Warehouse.Edit", "Admin/Shipping/EditWarehouse/{id}",
                 new { controller = "CustomShipping", action = "EditWarehouse", area = "Admin" },
                 new { id = @"\d+" });

            routeBuilder.MapControllerRoute("Plugin.Shipping.Core.Warehouse.Create", "Admin/Shipping/CreateWarehouse",
                new { controller = "CustomShipping", action = "CreateWarehouse", area = "Admin" });

            routeBuilder.MapControllerRoute("Plugin.Shipping.Core.Admin.Setting.Shipping", "Admin/Setting/Shipping",
                 new { controller = "CustomSetting", action = "Shipping", area = "Admin" });

            routeBuilder.MapControllerRoute("Plugin.Shipping.Core.Admin.Order.Edit", "Admin/Order/Edit/{id}",
                 new { controller = "CustomOrder", action = "Edit", area = "Admin" },
                 new { id = @"\d+" });
        }
    }
}
