using System.Collections.Generic;

namespace Nop.Plugin.Payments.WsPay.Models
{
    public class CardSelectionModel
    {
        #region Ctor

        public CardSelectionModel()
        {
            BankMaxRates = new Dictionary<string, int>();
        }

        #endregion

        public string CardName { get; set; }
        public Dictionary<string, int> BankMaxRates { get; set; }
    }
}
