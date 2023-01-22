using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Photography.CMS.Domain;
using Nop.Plugin.Photography.CMS.Models;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Photography.CMS.Factories
{
    public interface IGalleryModelFactory
    {
        Task<GalleryModel> PrepareGalleryModelAsync(string systemName);
        Task<Dictionary<int, PictureModel>> PrepareGalleryPictureModels(IList<PictureGallery> pictureGalleries);
    }
}