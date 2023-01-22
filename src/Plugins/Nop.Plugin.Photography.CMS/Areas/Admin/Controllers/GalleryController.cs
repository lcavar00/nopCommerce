using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Photography.CMS.Areas.Admin.Factories;
using Nop.Plugin.Photography.CMS.Areas.Admin.Models.Gallery;
using Nop.Plugin.Photography.CMS.Domain;
using Nop.Plugin.Photography.CMS.Services;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Photography.CMS.Areas.Admin.Controllers
{
    public class GalleryController : BasePluginController
    {
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGalleryModelFactory _galleryModelFactory;
        private readonly IGalleryService _galleryService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;

        #region Ctor

        public GalleryController(ICustomerActivityService customerActivityService,
            IGalleryModelFactory galleryModelFactory,
            IGalleryService galleryService,
            ILocalizationService localizationService,
            INotificationService notificationService)
        {
            _customerActivityService = customerActivityService;
            _galleryModelFactory = galleryModelFactory;
            _galleryService = galleryService;
            _localizationService = localizationService;
            _notificationService = notificationService;
        }

        #endregion

        public virtual IActionResult Index()
        {
            return RedirectToAction("Galleries");
        }


        public virtual async Task<IActionResult> Galleries(int? filterByGalleryId)
        {
            //prepare model
            var model = _galleryModelFactory.PrepareGalleryContentModelAsync(new GalleryContentModel(), filterByGalleryId);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(GallerySearchModel searchModel)
        {
            var model = await _galleryModelFactory.PrepareGalleryListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> GalleryCreate()
        {
            var model = await _galleryModelFactory.PrepareGalleryModelAsync(new GalleryModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> GalleryCreate(GalleryModel model, bool continueEditing)
        {
            if (ModelState.IsValid)
            {
                var gallery = model.ToEntity<Gallery>();
                gallery.CreatedOnUtc = DateTime.UtcNow;
                await _galleryService.InsertGalleryAsync(gallery);

                await _customerActivityService.InsertActivityAsync("AddNewGallery",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.AddNewGallery"), gallery.Id), gallery);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ContentManagement.Gallery.Added"));

                if (!continueEditing)
                    return RedirectToAction("Galleries");

                return RedirectToAction("GalleryEdit", new { id = gallery.Id });
            }

            //prepare model
            model = await _galleryModelFactory.PrepareGalleryModelAsync(model, null, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public async Task<IActionResult> GalleryEdit(GalleryModel model, bool continueEditing)
        {
            var gallery = await _galleryService.GetGalleryByIdAsync(model.Id);
            if (gallery == null)
                return RedirectToAction("Galleries");

            if (ModelState.IsValid)
            {
                gallery = model.ToEntity(gallery);
                await _galleryService.UpdateGalleryAsync(gallery);

                await _customerActivityService.InsertActivityAsync("EditGallery",
                    string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditGallery"), gallery.Id), gallery);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ContentManagement.Gallery.Updated"));

                if (!continueEditing)
                    return RedirectToAction("Galleries");

                return RedirectToAction("GalleryEdit", new { id = gallery.Id });
            }

            //prepare model
            model = await _galleryModelFactory.PrepareGalleryModelAsync(model, gallery, true);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Delete(int id)
        {
            var gallery = await _galleryService.GetGalleryByIdAsync(id);
            if (gallery == null)
                return RedirectToAction("Galleries");

            await _galleryService.DeleteGalleryAsync(gallery);

            await _customerActivityService.InsertActivityAsync("DeleteGallery",
                   string.Format(await _localizationService.GetResourceAsync("ActivityLog.DeleteGallery"), gallery.Id), gallery);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.ContentManagement.Gallery.Deleted"));

            return RedirectToAction("Galleries");
        }
    }
}
