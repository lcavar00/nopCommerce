using Nop.Data;
using Nop.Plugin.Payments.WsPay.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.WsPay.Services
{
    public class RatePaymentService : IRatePaymentService
    {
        #region Fields

        private readonly IRepository<RatePayment> _ratePaymentRepository;

        #endregion

        #region Ctor

        public RatePaymentService(IRepository<RatePayment> ratePaymentRepository)
        {
            _ratePaymentRepository = ratePaymentRepository;
        }

        #endregion

        public async Task DeleteRatePaymentAsync(RatePayment ratePayment)
        {
            await _ratePaymentRepository.DeleteAsync(ratePayment);
        }

        public async Task DeleteRatePaymentAsync(int id)
        {
            var ratePayment = await _ratePaymentRepository.GetByIdAsync(id);
            await DeleteRatePaymentAsync(ratePayment);
        }

        public async Task<List<RatePayment>> GetAllRatePaymentsAsync()
        {
            return await _ratePaymentRepository.Table.ToListAsync();
        }

        public async Task<RatePayment> GetRatePaymentByIdAsync(int id)
        {
            return await _ratePaymentRepository.GetByIdAsync(id);
        }

        public async Task<RatePayment> GetRatePaymentByOrderIdAsync(int id)
        {
            return await _ratePaymentRepository.Table.FirstOrDefaultAsync(a => a.OrderId == id);
        }

        public async Task<RatePayment> InsertRatePaymentAsync(RatePayment ratePayment)
        {
            await _ratePaymentRepository.InsertAsync(ratePayment);

            return ratePayment;
        }

        public async Task<RatePayment> UpdateRatePaymentAsync(RatePayment ratePayment)
        {
            await _ratePaymentRepository.UpdateAsync(ratePayment);

            return ratePayment;
        }
    }
}
