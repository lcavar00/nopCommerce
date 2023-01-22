using Nop.Core;

namespace Nop.Plugin.Photography.CMS.Domain
{
    public class PictureGallery : BaseEntity
    {
        public int PictureId { get; set; }
        public int GalleryId { get; set; }

        public int DisplayOrder { get; set; }
    }
}
