using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Media;

namespace Nop.Plugin.Photography.CMS.Models
{
    public partial record GalleryModel : BaseNopEntityModel
    {
        public string SystemName { get; set; }
        public Dictionary<int, PictureModel> Pictures { get; set; }
    }
}
