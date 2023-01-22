using System.Threading.Tasks;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.ReCaching.Services;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.ReCaching.Tasks
{
    public class ClearProductCacheTask : IScheduleTask
    {
        private readonly ICacheClearingService<Product> _productCacheClearingService;

        public ClearProductCacheTask(ICacheClearingService<Product> productCacheClearingService)
        {
            _productCacheClearingService = productCacheClearingService;
        }

        public async Task ExecuteAsync()
        {
            await _productCacheClearingService.ClearCacheAsync();
        }
    }
}
