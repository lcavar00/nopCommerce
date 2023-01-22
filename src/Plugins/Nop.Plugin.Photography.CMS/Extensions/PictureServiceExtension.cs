using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Media;
using Nop.Services.Media;

namespace Nop.Plugin.Photography.CMS.Extensions
{
    public static class PictureServiceExtension
    {
        public static async Task<IList<Picture>> GetPicturesByIdsAsync(this IPictureService pictureService, IEnumerable<int> ids)
        {
            var pictures = new List<Picture>();

            foreach(var id in ids)
            {
                var picture = await pictureService.GetPictureByIdAsync(id);
                pictures.Add(picture);
            }

            return pictures;
        }
    }
}
