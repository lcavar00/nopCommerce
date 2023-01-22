using Nop.Plugin.Payments.WsPay.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.WsPay.Services
{
    public interface IRatePaymentService
    {
        /// <summary>
        /// Returns a Rate payment instance using its identifier.
        /// </summary>
        /// <param name="id">Rate payment identifier</param>
        /// <returns>Rate payment instance</returns>
        Task<RatePayment> GetRatePaymentByIdAsync(int id);

        /// <summary>
        /// Returns a Rate payment instance using its Order identifier.
        /// </summary>
        /// <param name="id">Order identifier</param>
        /// <returns>Rate payment instance</returns>
        Task<RatePayment> GetRatePaymentByOrderIdAsync(int id);

        /// <summary>
        /// Returns all Rate payment instances.
        /// </summary>
        /// <returns>List of rate payment instances</returns>
        Task<List<RatePayment>> GetAllRatePaymentsAsync();

        /// <summary>
        /// Updates and returns Rate payment instance.
        /// </summary>
        /// <param name="ratePayment">Rate payment instance to be updated</param>
        /// <returns>Rate payment instance</returns>
        Task<RatePayment> UpdateRatePaymentAsync(RatePayment ratePayment);

        /// <summary>
        /// Inserts and returns a new RatePayment instance.
        /// </summary>
        /// <param name="ratePayment">Rate payment instance to be inserted</param>
        /// <returns>Rate payment instance</returns>
        Task<RatePayment> InsertRatePaymentAsync(RatePayment ratePayment);

        /// <summary>
        /// Deletes a Rate payment instance.
        /// </summary>
        /// <param name="ratePayment">Rate payment instance to be deleted</param>
        Task DeleteRatePaymentAsync(RatePayment ratePayment);

        /// <summary>
        /// Deletes a Rate payment using its identifier.
        /// </summary>
        /// <param name="id">Rate payment identifier</param>
        Task DeleteRatePaymentAsync(int id);
    }
}
