using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Photography.CMS.Domain;
using Nop.Plugin.Photography.CMS.Models;
using Nop.Plugin.Photography.CMS.Services;
using Nop.Services.Media;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Photography.CMS.Factories
{
    public class GalleryModelFactory : IGalleryModelFactory
    {
        private IGalleryService _galleryService;
        private IPictureService _pictureService;

        #region Ctor

        public GalleryModelFactory(IGalleryService galleryService,
            IPictureService pictureService)
        {
            _galleryService = galleryService;
            _pictureService = pictureService;
        }

        #endregion

        public virtual async Task<GalleryModel> PrepareGalleryModelAsync(string systemName)
        {
            var gallery = await _galleryService.GetGalleryBySystemNameAsync(systemName);
            var pictureGalleries = await _galleryService.GetPictureGalleriesByGalleryIdAsync(gallery.Id);

            var model = new GalleryModel
            {
                Pictures = await PrepareGalleryPictureModels(pictureGalleries),
                Id = gallery.Id,
                SystemName = gallery.SystemName
            };

            return model;
        }

        public virtual async Task<Dictionary<int, PictureModel>> PrepareGalleryPictureModels(IList<PictureGallery> pictureGalleries)
        {
            //var pictures = await _pictureService.GetPicturesByIdsAsync(pictureGalleries.Select(a => a.PictureId));

            var pictureModels = new Dictionary<int, PictureModel>();

            string fullSizeImageUrl, imageUrl, thumbImageUrl;
            for (var i = 0; i < pictureGalleries.Count; i++)
            {
                var picture = await _pictureService.GetPictureByIdAsync(pictureGalleries[i].PictureId);
                (imageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (fullSizeImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);
                (thumbImageUrl, picture) = await _pictureService.GetPictureUrlAsync(picture);

                var pictureModel = new PictureModel
                {
                    ImageUrl = imageUrl,
                    ThumbImageUrl = thumbImageUrl,
                    FullSizeImageUrl = fullSizeImageUrl,
                    Title = "SORT THIS OUT",
                    AlternateText = "SORT THIS OUT AS WELL",
                };

                pictureModels.Add(pictureGalleries[i].DisplayOrder, pictureModel);
            }

            return pictureModels;
        }
    }
}
