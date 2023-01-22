using Nop.Core;

namespace Nop.Plugin.Payments.WsPay.Domain
{
    public class RatePayment : BaseEntity
    {
        public virtual int OrderId { get; set; }

        public virtual int RatesNumber { get; set; }
        public virtual string CardProvider { get; set; }
        public virtual bool IsDeleted { get; set; }
    }
}
