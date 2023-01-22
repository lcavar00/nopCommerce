using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Photography.CMS.Areas.Admin.Models.Gallery
{
    public partial record GalleryModel : BaseNopEntityModel
    {
        public GalleryModel()
        {
            AvailableLanguages = new List<SelectListItem>();
            SelectedStoreIds = new List<int>();
            AvailableStores = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Admin.ContentManagement.Galleries.Fields.Language")]
        public int LanguageId { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Galleries.Fields.IncludeInSitemap")]
        public bool IncludeInSitemap { get; set; }

        public IList<SelectListItem> AvailableLanguages { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Galleries.Fields.Language")]
        public string LanguageName { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Galleries.Fields.SystemName")]
        public string SystemName { get; set; }

        [NopResourceDisplayName("Admin.ContentManagement.Galleries.Fields.Title")]
        public string Title { get; set; }

        public DateTime? CreatedOnUtc { get; set; }

        //store mapping
        [NopResourceDisplayName("Admin.ContentManagement.Galleries.Fields.LimitedToStores")]
        public IList<int> SelectedStoreIds { get; set; }
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}
