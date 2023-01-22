using System.Threading.Tasks;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.ReCaching
{
    public class ReCachingPlugin : BasePlugin
    {
        private readonly IScheduleTaskService _scheduleTaskService;

        public ReCachingPlugin(IScheduleTaskService scheduleTaskService) 
        {
            _scheduleTaskService = scheduleTaskService;
        }

        public override async Task InstallAsync()
        {
            await CreateScheduleTasksAsync();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await DeleteScheduleTasksAsync();

            await base.UninstallAsync();
        }

        private async Task CreateScheduleTasksAsync()
        {
            var clearProductCacheTask = await _scheduleTaskService.GetTaskByTypeAsync(ReCachingDefaults.ClearProductCacheTaskType);
            if(clearProductCacheTask is null)
            {
                clearProductCacheTask = new ScheduleTask
                {
                    Enabled = false,
                    Name = ReCachingDefaults.ClearProductCacheTaskName,
                    Seconds = 3600,
                    Type = ReCachingDefaults.ClearProductCacheTaskType
                };

                await _scheduleTaskService.InsertTaskAsync(clearProductCacheTask);
            }
        }

        private async Task DeleteScheduleTasksAsync()
        {
            var clearProductCacheTask = await _scheduleTaskService.GetTaskByTypeAsync(ReCachingDefaults.ClearProductCacheTaskType);
            if (clearProductCacheTask is not null)
            {
                await _scheduleTaskService.DeleteTaskAsync(clearProductCacheTask);
            }
        }
    }
}