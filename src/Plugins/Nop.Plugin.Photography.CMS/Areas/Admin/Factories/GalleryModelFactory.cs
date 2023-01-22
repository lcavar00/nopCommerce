using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Photography.CMS.Areas.Admin.Models.Gallery;
using Nop.Plugin.Photography.CMS.Services;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Photography.CMS.Areas.Admin.Factories
{
    public class GalleryModelFactory : IGalleryModelFactory
    {
        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IGalleryService _galleryService;

        #region Ctor

        public GalleryModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            IGalleryService galleryService)
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _galleryService = galleryService;
        }

        #endregion

        public virtual async Task<GalleryContentModel> PrepareGalleryContentModelAsync(GalleryContentModel galleryContentModel, int? filterByGalleryId)
        {
            if (galleryContentModel == null)
                throw new ArgumentNullException(nameof(galleryContentModel));

            //prepare nested search models
            await PrepareGalleryListModelAsync(galleryContentModel.Galleries);
            var gallery = await _galleryService.GetGalleryByIdAsync(filterByGalleryId ?? 0);

            return galleryContentModel;
        }

        public virtual async Task<GalleryModel> PrepareGalleryModelAsync(GalleryModel model, Domain.Gallery gallery, bool excludeProperties = false)
        {
            if(gallery != null)
            {
                if(model == null)
                {
                    model = gallery.ToModel<GalleryModel>();
                }
            }

            await _baseAdminModelFactory.PrepareLanguagesAsync(model.AvailableLanguages, false);

            return model;
        }

        public virtual async Task<GalleryListModel> PrepareGalleryListModelAsync(GallerySearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var galleries = await _galleryService.GetAllGalleriesAsync(pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize, systemName: searchModel.SearchSystemName, title: searchModel.SearchTitle);

            var model = await new GalleryListModel().PrepareToGridAsync(searchModel, galleries, () =>
            {
                return galleries.SelectAwait(async gallery =>
                {
                    //fill in model values from the entity
                    var galleryModel = gallery.ToModel<GalleryModel>();

                    return galleryModel;
                });
            });

            return model;
        }
    }
}
