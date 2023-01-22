using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.LowestPrice.Factories;
using Nop.Web.Framework.Components;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.LowestPrice.Components
{
    public class LowestPriceViewComponent : NopViewComponent
    {
        private readonly ILowestPriceModelFactory _lowestPriceModelFactory;

        public LowestPriceViewComponent(ILowestPriceModelFactory lowestPriceModelFactory)
        {
            _lowestPriceModelFactory = lowestPriceModelFactory;
        }

        public async Task<IViewComponentResult> InvokeAsync(ProductDetailsModel additionalData)
        {
            var model = await _lowestPriceModelFactory.PrepareLowestProductPriceModelAsync(additionalData.Id);

            return View(model);
        }
    }
}
