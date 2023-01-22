using System;
using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.LowestPrice
{
    public class PriceHistorySettings : ISettings
    {
        public TimeSpan TimeSpan { get; set; }
        public bool DisplayForDiscounts { get; set; }
        public bool DisplayForOldPrice { get; set; }
    }
}
