using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Components
{
    [ViewComponent(Name = "SendMultipleShipmentRequests")]
    public class SendMultipleShipmentRequestsViewComponent : NopViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View("~/Plugins/Shipping.Core/Areas/Admin/Views/Shared/Components/SendMultipleShipmentRequests/Default.cshtml");
        }
    }
}
