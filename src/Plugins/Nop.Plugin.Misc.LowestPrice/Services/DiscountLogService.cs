using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Data;
using Nop.Plugin.Misc.LowestPrice.Domain;
namespace Nop.Plugin.Misc.LowestPrice.Services
{
    public partial class DiscountLogService : IDiscountLogService
    {
        #region Fields

        private readonly IRepository<DiscountLog> _discountLogRepository;
        private readonly IRepository<PriceLogDiscountLog> _priceLogDiscountLogRepository;

        #endregion

        #region Ctor

        public DiscountLogService(IRepository<DiscountLog> discountLogRepository,
            IRepository<PriceLogDiscountLog> priceLogDiscountLogRepository)
        {
            _discountLogRepository = discountLogRepository;
            _priceLogDiscountLogRepository = priceLogDiscountLogRepository;
        }

        #endregion

        public async Task DeleteDiscountLogAsync(DiscountLog discountLog)
        {
            await _discountLogRepository.DeleteAsync(discountLog);
        }

        public async Task DeleteDiscountLogsAsync(IEnumerable<DiscountLog> discountLogs)
        {
            await _discountLogRepository.DeleteAsync(discountLogs.ToArray());
        }

        public async Task DeletePriceLogDiscountLogAsync(PriceLogDiscountLog priceLogDiscountLog)
        {
            await _priceLogDiscountLogRepository.DeleteAsync(priceLogDiscountLog);
        }

        public async Task DeletePriceLogDiscountLogsAsync(IEnumerable<PriceLogDiscountLog> priceLogDiscountLogs)
        {
            await _priceLogDiscountLogRepository.DeleteAsync(priceLogDiscountLogs.ToArray());
        }

        public async Task<IList<DiscountLog>> GetAllDiscountLogsAsync()
        {
            return await _discountLogRepository.Table.ToListAsync();
        }

        public async Task<IList<PriceLogDiscountLog>> GetAllPriceLogDiscountLogsAsync()
        {
            return await _priceLogDiscountLogRepository.Table.ToListAsync();
        }

        public async Task<IList<PriceLogDiscountLog>> GetAllPriceLogDiscountLogsByEntityNameAsync(string entityName)
        {
            return await _priceLogDiscountLogRepository.Table.Where(a => a.EntityName == entityName).ToListAsync();
        }

        public async Task<DiscountLog> GetDiscountLogByIdAsync(int discountLogId)
        {
            return await _discountLogRepository.GetByIdAsync(discountLogId);
        }

        public async Task<IList<DiscountLog>> GetDiscountLogsByDiscountIdAsync(int discountId)
        {
            return await _discountLogRepository.Table.Where(a => a.DiscountId == discountId).ToListAsync();
        }

        public async Task<IList<DiscountLog>> GetDiscountLogsByDiscountIdsAsync(IEnumerable<int> discountIds, string entityName = null)
        {
            var priceLogDiscountLogsQuery = _priceLogDiscountLogRepository.Table.Where(a => a.EntityName == entityName);
            var query = _discountLogRepository.Table.Where(a => discountIds.Contains(a.DiscountId));

            query = !string.IsNullOrEmpty(entityName) ? query.Where(a => priceLogDiscountLogsQuery.Any(b => b.DiscountLogId == a.Id)) : query;
            return await query.ToListAsync();
        }

        public async Task<IList<DiscountLog>> GetDiscountLogsByDiscountNameAsync(string discountName)
        {
            return await _discountLogRepository.Table.Where(a => a.Name == discountName).ToListAsync();
        }

        public async Task<PriceLogDiscountLog> GetPriceLogDiscountLogByIdAsync(int id)
        {
            return await _priceLogDiscountLogRepository.GetByIdAsync(id);
        }

        public async Task<IList<PriceLogDiscountLog>> GetPriceLogDiscountLogsByDiscountLogIdAsync(int discountLogId)
        {
            return await _priceLogDiscountLogRepository.Table.Where(a => a.DiscountLogId == discountLogId).ToListAsync();
        }

        public async Task<IList<PriceLogDiscountLog>> GetPriceLogDiscountLogsByDiscountLogIdsAsync(IEnumerable<int> discountLogIds)
        {
            return await _priceLogDiscountLogRepository.Table.Where(a => discountLogIds.Contains(a.Id)).ToListAsync();
        }

        public async Task<IList<PriceLogDiscountLog>> GetPriceLogDiscountLogsByPriceLogIdAsync(int entityId, string entityName)
        {
            return await _priceLogDiscountLogRepository.Table.Where(a => a.EntityName == entityName && a.EntityId == entityId).ToListAsync();
        }

        public async Task InsertDiscountLogAsync(DiscountLog discountLog)
        {
            await _discountLogRepository.InsertAsync(discountLog);
        }

        public async Task InsertDiscountLogsAsync(IEnumerable<DiscountLog> discountLogs)
        {
            await _discountLogRepository.InsertAsync(discountLogs.ToArray());
        }

        public async Task InsertPriceLogDiscountLogAsync(PriceLogDiscountLog priceLogDiscountLog)
        {
            await _priceLogDiscountLogRepository.InsertAsync(priceLogDiscountLog);
        }

        public async Task InsertPriceLogDiscountLogsAsync(IEnumerable<PriceLogDiscountLog> priceLogDiscountLogs)
        {
            await _priceLogDiscountLogRepository.InsertAsync(priceLogDiscountLogs.ToArray());
        }

        public async Task UpdateDiscountLogAsync(DiscountLog discountLog)
        {
            await _discountLogRepository.UpdateAsync(discountLog);
        }

        public async Task UpdateDiscountLogsAsync(IEnumerable<DiscountLog> discountLogs)
        {
            await _discountLogRepository.UpdateAsync(discountLogs.ToArray());
        }

        public async Task UpdatePriceLogDiscountLogAsync(PriceLogDiscountLog priceLogDiscountLog)
        {
            await _priceLogDiscountLogRepository.UpdateAsync(priceLogDiscountLog);
        }

        public async Task UpdatePriceLogDiscountLogAsync(IEnumerable<PriceLogDiscountLog> priceLogDiscountLogs)
        {
            await _priceLogDiscountLogRepository.UpdateAsync(priceLogDiscountLogs.ToArray());
        }
    }
}
