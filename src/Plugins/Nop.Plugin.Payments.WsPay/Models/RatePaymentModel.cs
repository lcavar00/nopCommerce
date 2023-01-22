using System.Collections.Generic;

namespace Nop.Plugin.Payments.WsPay.Models
{
    public class RatePaymentModel
    {
        #region Ctor

        public RatePaymentModel()
        {
            Cards = new List<CardSelectionModel>();
        }

        #endregion

        public bool AllowRatePayment { get; set; }
        public bool UseRatePayment { get; set; }
        public string OrderTotal { get; set; }
        public decimal OrderTotalValue { get; set; }
        public List<CardSelectionModel> Cards { get; set; }
    }
}
