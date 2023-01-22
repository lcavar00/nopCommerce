using System.Threading.Tasks;
using Nop.Plugin.Photography.CMS.Areas.Admin.Models.Gallery;

namespace Nop.Plugin.Photography.CMS.Areas.Admin.Factories
{
    public interface IGalleryModelFactory
    {
        Task<GalleryContentModel> PrepareGalleryContentModelAsync(GalleryContentModel galleryContentModel, int? filterByGalleryId);
        Task<GalleryModel> PrepareGalleryModelAsync(GalleryModel model, Domain.Gallery gallery, bool excludeProperties = false);
        Task<GalleryListModel> PrepareGalleryListModelAsync(GallerySearchModel searchModel);
    }
}