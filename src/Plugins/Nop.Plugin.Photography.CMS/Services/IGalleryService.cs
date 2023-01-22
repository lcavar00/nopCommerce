using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Photography.CMS.Domain;

namespace Nop.Plugin.Photography.CMS.Services
{
    public interface IGalleryService
    {
        Task DeleteGalleryAsync(Gallery gallery);
        Task DeleteGalleryAsync(IEnumerable<Gallery> galleries);
        Task<IPagedList<Gallery>> GetAllGalleriesAsync(int pageIndex = 0, int pageSize = int.MaxValue, string title = null, string systemName = null);
        Task<Gallery> GetGalleryByIdAsync(int id);
        Task<Gallery> GetGalleryBySystemNameAsync(string systemName);
        Task<IList<PictureGallery>> GetPictureGalleriesByGalleryIdAsync(int id);
        Task InsertGalleryAsync(Gallery gallery);
        Task InsertGalleryAsync(IEnumerable<Gallery> galleries);
        Task UpdateGalleryAsync(Gallery gallery);
        Task UpdateGalleryAsync(IEnumerable<Gallery> galleries);
    }
}