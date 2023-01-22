using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.WsPay
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            routeBuilder.MapControllerRoute("Plugin.Payments.WsPay.SuccessHandler",
                "Plugins/WsPay/SuccessHandler",
                new { controller = "WsPay", action = "SuccessHandler" }
            );

            //Cancel
            routeBuilder.MapControllerRoute("Plugin.Payments.WsPay.CancelHandler",
                 "Plugins/WsPay/CancelHandler",
                 new { controller = "WsPay", action = "CancelHandler" }
            );

            //error
            routeBuilder.MapControllerRoute("Plugin.Payments.WsPay.ErrorHandler",
                "Plugins/WsPay/ErrorHandler",
                new { controller = "WsPay", action = "ErrorHandler" }
           );
        }
        public int Priority => -1;
    }
}
