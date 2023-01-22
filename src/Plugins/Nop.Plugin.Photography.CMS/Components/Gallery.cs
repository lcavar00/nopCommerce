using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Photography.CMS.Factories;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Photography.CMS.Components
{
    public class GalleryViewComponent : NopViewComponent
    {
        private readonly IGalleryModelFactory _galleryModelFactory;

        #region Ctor

        public GalleryViewComponent(IGalleryModelFactory galleryModelFactory)
        {
            _galleryModelFactory = galleryModelFactory;
        }

        #endregion

        public async Task<IViewComponentResult> InvokeAsync(string systemName)
        {
            var model = await _galleryModelFactory.PrepareGalleryModelAsync(systemName);

            if (model == null)
                return Content("");

            return View(model);
        }
    }
}
