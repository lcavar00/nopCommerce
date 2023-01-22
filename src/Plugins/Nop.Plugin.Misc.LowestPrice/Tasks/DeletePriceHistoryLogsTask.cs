using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.Misc.LowestPrice.Services;
using Nop.Services.Configuration;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.LowestPrice.Tasks
{
    public partial class DeletePriceHistoryLogsTask : IScheduleTask
    {
        #region Fields

        private readonly IProductAttributeCombinationPriceHistoryService _productAttributeCombinationPriceHistory;
        private readonly IProductPriceHistoryService _productPriceHistoryService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public DeletePriceHistoryLogsTask(IProductAttributeCombinationPriceHistoryService productAttributeCombinationPriceHistory,
            IProductPriceHistoryService productPriceHistoryService,
            ISettingService settingService)
        {
            _productAttributeCombinationPriceHistory = productAttributeCombinationPriceHistory;
            _productPriceHistoryService = productPriceHistoryService;
            _settingService = settingService;
        }

        #endregion

        public async Task ExecuteAsync()
        {
            var priceHistorySettings = await _settingService.LoadSettingAsync<PriceHistorySettings>();

            var productPriceLogs = await _productPriceHistoryService.GetAllProductPriceLogsAsync();
            var productPriceLogsForDeletion = await productPriceLogs.Where(a => (a.UpdatedOnUtc ?? a.CreatedOnUtc) <= DateTime.UtcNow - priceHistorySettings.TimeSpan).ToListAsync();

            await _productPriceHistoryService.DeleteProductPriceLogsAsync(productPriceLogsForDeletion);

            var productAttributeCombinationLogs = await _productAttributeCombinationPriceHistory.GetAllProductAttributeCombintaionPriceLogsAsync();
            var productAttributeCombinationLogsForDeletion = await productAttributeCombinationLogs.Where(a => (a.UpdatedOnUtc ?? a.CreatedOnUtc) <= DateTime.UtcNow - priceHistorySettings.TimeSpan).ToListAsync();

            await _productAttributeCombinationPriceHistory.DeleteProductAttributeCombinationPriceLogsAsync(productAttributeCombinationLogsForDeletion);

        }
    }
}
