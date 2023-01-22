using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Payments.WsPay.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.WsPay.Components
{
    [ViewComponent(Name = "WsPay")]
    public class WsPayViewComponent : NopViewComponent
    {
        #region Fields

        private readonly IRatePaymentModelFactory _ratePaymentModelFactory;

        #endregion

        #region Ctor

        public WsPayViewComponent(IRatePaymentModelFactory ratePaymentModelFactory)
        {
            _ratePaymentModelFactory = ratePaymentModelFactory;
        }

        #endregion

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = await _ratePaymentModelFactory.PrepareRatePaymentModelAsync();

            return View("~/Plugins/Payments.WsPay/Views/PaymentInfo.cshtml", model);
        }
    }
}
