using System;
using Nop.Core;

namespace Nop.Plugin.ImportExport.Core.Domain
{
    public class EntityMapping : BaseEntity
    {
        public virtual string SourceEntityName { get; set; }
        public virtual string DestinationEntityName { get; set; }

        public virtual bool Deleted { get; set; }
        public virtual DateTime CreatedAtUtc { get; set; }
        public virtual DateTime? UpdatedAtUtc { get; set; }
    }
}
