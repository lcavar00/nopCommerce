using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Stores;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.MorePrices.Domain
{
    public class Price : BaseEntity, IAclSupported, ILocalizedEntity, IStoreMappingSupported
    {
        public virtual string Name { get; set; }
        public virtual bool Deleted { get; set; }
        public virtual decimal? FixedPrice { get; set; }
        public virtual decimal? ReducedByPercentage { get; set; }
        public virtual int VendorId { get; set; }
        public virtual DateTime CreatedOnUtc { get; set; }
        public virtual DateTime UpdatedOnUtc { get; set; }
        public bool SubjectToAcl { get; set; }
        public bool LimitedToStores { get; set; }

        public virtual IEnumerable<PriceManufacturer> PriceManufacturers { get; set; }
        public virtual IEnumerable<PriceCategory> PriceCategories { get; set; }

    }
}
