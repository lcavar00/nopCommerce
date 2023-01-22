using Nop.Plugin.Payments.WsPay.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.WsPay.Factories
{
    public interface IRatePaymentModelFactory
    {
        Task<RatePaymentModel> PrepareRatePaymentModelAsync();
        Task<List<CardSelectionModel>> GetCardsForOrderTotalAsync(decimal orderTotal);
    }
}
