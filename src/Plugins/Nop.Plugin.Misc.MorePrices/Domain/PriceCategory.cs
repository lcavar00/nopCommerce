using Nop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.MorePrices.Domain
{
    public class PriceCategory : BaseEntity
    {
        public virtual int PriceId { get; set; }
        public virtual int CategoryId { get; set; }
    }
}
