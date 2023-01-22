using System.Threading.Tasks;
using Nop.Plugin.Misc.LowestPrice.Services;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.LowestPrice.Tasks
{
    public class CreatePriceHistoryLogsTask : IScheduleTask
    {
        #region Fields

        private readonly ILowestPriceService _lowestPriceService;

        #endregion

        #region Ctor

        public CreatePriceHistoryLogsTask(ILowestPriceService lowestPriceService)
        {
            _lowestPriceService = lowestPriceService;
        }

        #endregion

        public async Task ExecuteAsync()
        {
            await _lowestPriceService.LogPricesAndDiscountsAsync();
        }
    }
}
