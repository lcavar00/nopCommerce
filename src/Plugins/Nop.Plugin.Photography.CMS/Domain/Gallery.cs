using System;
using Nop.Core;

namespace Nop.Plugin.Photography.CMS.Domain
{
    public partial class Gallery : BaseEntity
    {
        public string SystemName { get; set; }
        public string Title { get; set; }
        public DateTime CreatedOnUtc { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }
    }
}
