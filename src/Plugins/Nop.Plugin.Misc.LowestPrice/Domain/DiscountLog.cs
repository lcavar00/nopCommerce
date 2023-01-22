using System;
using Nop.Core;
using Nop.Core.Domain.Discounts;

namespace Nop.Plugin.Misc.LowestPrice.Domain
{
    public class DiscountLog : BaseEntity
    {
        /// <summary>
        /// Gets or sets the Discount identifier
        /// </summary>
        public virtual int DiscountId { get; set; }

        /// <summary>
        /// Gets or sets the name
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the discount type identifier
        /// </summary>
        public virtual int DiscountTypeId { get; set; }

        /// <summary>
        /// Gets or sets the discount type
        /// </summary>
        public virtual DiscountType DiscountType
        {
            get => (DiscountType)DiscountTypeId;
            set => DiscountTypeId = (int)value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to use percentage
        /// </summary>
        public virtual bool UsePercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount percentage
        /// </summary>
        public virtual decimal DiscountPercentage { get; set; }

        /// <summary>
        /// Gets or sets the discount amount
        /// </summary>
        public virtual decimal DiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the maximum discount amount (used with "UsePercentage")
        /// </summary>
        public virtual decimal? MaximumDiscountAmount { get; set; }

        /// <summary>
        /// Gets or sets the discount start date and time
        /// </summary>
        public virtual DateTime? StartDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the discount end date and time
        /// </summary>
        public virtual DateTime? EndDateUtc { get; set; }

        /// <summary>
        /// Gets or sets the discount log creation date and time
        /// </summary>
        public virtual DateTime CreatedOnUtc { get; set; }

    }
}
