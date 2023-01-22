using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.WsPay.Infrastructure
{
    public class RouteProvider : IRouteProvider
    {
        public int Priority => int.MaxValue;

        public void RegisterRoutes(IEndpointRouteBuilder routeBuilder)
        {
            //checkout
            routeBuilder.MapControllerRoute("Plugin.Payments.WsPay.Checkout.OpcSavePaymentInfo", "checkout/OpcSavePaymentInfo/",
                new { controller = "CustomCheckout", action = "OpcSavePaymentInfo" });

            routeBuilder.MapControllerRoute("Plugin.Payments.WsPay.Checkout.OpcConfirmOrder", "checkout/OpcConfirmOrder/",
                new { controller = "CustomCheckout", action = "OpcConfirmOrder" });
        }
    }
}
