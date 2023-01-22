using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Photography.CMS.Areas.Admin.Models.Gallery
{
    /// <summary>
    /// Represents a gallery search model
    /// </summary>
    public partial record GallerySearchModel : BaseSearchModel
    {
        #region Ctor

        public GallerySearchModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.ContentManagement.Galleries.List.SearchStore")]
        public int SearchStoreId { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }

        public string SearchSystemName { get; set; }
        public string SearchTitle { get; set; }

        public bool HideStoresList { get; set; }

        #endregion
    }
}
