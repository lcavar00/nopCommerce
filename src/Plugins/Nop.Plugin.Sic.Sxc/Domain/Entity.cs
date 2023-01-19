using System;

namespace Nop.Plugin.ToSic.Sxc.Domain
{
    public partial class Entity
    {
        public int EntityId { get; set; }
        public int EntityTypeId { get; set; }
        public int ParentId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime PublishedOn { get; set; }
        public string PublishedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
}
