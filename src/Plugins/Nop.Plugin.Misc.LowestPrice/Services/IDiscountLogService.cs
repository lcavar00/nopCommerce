using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Plugin.Misc.LowestPrice.Domain;

namespace Nop.Plugin.Misc.LowestPrice.Services
{
    public interface IDiscountLogService
    {
        Task<IList<DiscountLog>> GetAllDiscountLogsAsync();
        Task<DiscountLog> GetDiscountLogByIdAsync(int discountLogId);
        Task<IList<DiscountLog>> GetDiscountLogsByDiscountIdAsync(int discountId);
        Task<IList<DiscountLog>> GetDiscountLogsByDiscountIdsAsync(IEnumerable<int> discountIds, string entityName = null);
        Task<IList<DiscountLog>> GetDiscountLogsByDiscountNameAsync(string discountName);
        Task InsertDiscountLogAsync(DiscountLog discountLog);
        Task InsertDiscountLogsAsync(IEnumerable<DiscountLog> discountLogs);
        Task UpdateDiscountLogAsync(DiscountLog discountLog);
        Task UpdateDiscountLogsAsync(IEnumerable<DiscountLog> discountLogs);
        Task DeleteDiscountLogAsync(DiscountLog discountLog);
        Task DeleteDiscountLogsAsync(IEnumerable<DiscountLog> discountLogs);
        Task<IList<PriceLogDiscountLog>> GetAllPriceLogDiscountLogsAsync();
        Task<IList<PriceLogDiscountLog>> GetAllPriceLogDiscountLogsByEntityNameAsync(string entityName);
        Task<PriceLogDiscountLog> GetPriceLogDiscountLogByIdAsync(int id);
        Task<IList<PriceLogDiscountLog>> GetPriceLogDiscountLogsByPriceLogIdAsync(int entityId, string entityName);
        Task<IList<PriceLogDiscountLog>> GetPriceLogDiscountLogsByDiscountLogIdAsync(int discountLogId);
        Task<IList<PriceLogDiscountLog>> GetPriceLogDiscountLogsByDiscountLogIdsAsync(IEnumerable<int> discountLogIds);
        Task InsertPriceLogDiscountLogAsync(PriceLogDiscountLog priceLogDiscountLogMapping);
        Task InsertPriceLogDiscountLogsAsync(IEnumerable<PriceLogDiscountLog> priceLogDiscountLogMappings);
        Task UpdatePriceLogDiscountLogAsync(PriceLogDiscountLog priceLogDiscountLogMapping);
        Task UpdatePriceLogDiscountLogAsync(IEnumerable<PriceLogDiscountLog> priceLogDiscountLogMappings);
        Task DeletePriceLogDiscountLogAsync(PriceLogDiscountLog priceLogDiscountLogMapping);
        Task DeletePriceLogDiscountLogsAsync(IEnumerable<PriceLogDiscountLog> priceLogDiscountLogMappings);
    }
}