using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Photography.CMS.Domain;

namespace Nop.Plugin.Photography.CMS.Services
{
    public class GalleryService : IGalleryService
    {
        #region Fields

        private readonly IRepository<Gallery> _galleryRepository;
        private readonly IRepository<PictureGallery> _pictureGalleryRepository;

        #endregion

        #region Ctor

        public GalleryService(IRepository<Gallery> galleryRepository,
            IRepository<PictureGallery> pictureGalleryRepository)
        {
            _galleryRepository = galleryRepository;
            _pictureGalleryRepository = pictureGalleryRepository;
        }

        #endregion

        public virtual async Task DeleteGalleryAsync(Gallery gallery)
        {
            await _galleryRepository.DeleteAsync(gallery);
        }

        public virtual async Task DeleteGalleryAsync(IEnumerable<Gallery> galleries)
        {
            await _galleryRepository.DeleteAsync(galleries.ToList());
        }

        public virtual async Task<IPagedList<Gallery>> GetAllGalleriesAsync(int pageIndex = 0, int pageSize = int.MaxValue,
            string title = null, string systemName = null)
        {
            return await _galleryRepository.GetAllPagedAsync(async query =>
            {
                if (!string.IsNullOrEmpty(title))
                {
                    query = query.Where(g => g.Title.Contains(title));
                }

                if (!string.IsNullOrEmpty(systemName))
                {
                    query = query.Where(g => g.SystemName.Contains(systemName));
                }

                query = query.OrderByDescending(b => b.CreatedOnUtc);

                return query;
            }, pageIndex, pageSize);
        }

        public virtual async Task<Gallery> GetGalleryByIdAsync(int id)
        {
            return await _galleryRepository.Table.FirstOrDefaultAsync(a => a.Id == id);
        }

        public virtual async Task<Gallery> GetGalleryBySystemNameAsync(string systemName)
        {
            return await _galleryRepository.Table.FirstOrDefaultAsync(a => a.SystemName == systemName);
        }

        public virtual async Task<IList<PictureGallery>> GetPictureGalleriesByGalleryIdAsync(int id)
        {
            return await _pictureGalleryRepository.Table.Where(a => a.GalleryId == id).ToListAsync();
        }

        public virtual async Task InsertGalleryAsync(Gallery gallery)
        {
            await _galleryRepository.InsertAsync(gallery);
        }

        public virtual async Task InsertGalleryAsync(IEnumerable<Gallery> galleries)
        {
            await _galleryRepository.InsertAsync(galleries.ToList());
        }

        public virtual async Task UpdateGalleryAsync(Gallery gallery)
        {
            await _galleryRepository.UpdateAsync(gallery);
        }

        public virtual async Task UpdateGalleryAsync(IEnumerable<Gallery> galleries)
        {
            await _galleryRepository.UpdateAsync(galleries.ToList());
        }
    }
}
