using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Photography.CMS.Areas.Admin.Models.Gallery
{
    public record GalleryContentModel : BaseNopModel
    {
        public GalleryContentModel()
        {
            Galleries = new GallerySearchModel();
            SearchTitle = new GallerySearchModel().SearchTitle;
            SearchSystemName = new GallerySearchModel().SearchSystemName;
        }

        [NopResourceDisplayName("Admin.ContentManagement.Blog.BlogPosts.List.SearchTitle")]
        public string SearchTitle { get; set; }
        public string SearchSystemName { get; set; }
        public GallerySearchModel Galleries { get; set; }
    }
}
