using System;
using Nop.Core;

namespace Nop.Plugin.ImportExport.Core.Domain
{
    public class DataEntryMapping : BaseEntity, IEquatable<DataEntryMapping>
    {
        public virtual string SourceId { get; set; }
        public virtual string DestinationId { get; set; }
        
        public virtual string SourceCode { get; set; }
        public virtual string DestinationCode { get; set; }

        public virtual bool Deleted { get; set; }
        public virtual DateTime CreatedAtUtc { get; set; }
        public virtual DateTime? UpdatedAtUtc { get; set; }

        public virtual int EntityMappingId { get; set; }

        public bool Equals(DataEntryMapping other)
        {
            if (other == null)
                return false;

            return SourceId == other.SourceId && DestinationId == other.DestinationId
                && SourceCode == other.SourceCode && DestinationCode == other.DestinationCode
                && Deleted == other.Deleted && EntityMappingId == other.EntityMappingId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DataEntryMapping);
        }

        public override int GetHashCode()
        {
            return (SourceId, DestinationId, SourceCode, DestinationCode, Deleted, EntityMappingId).GetHashCode();
        }
    }
}
