using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Misc.LowestPrice
{
    public class LowestPricePlugin : BasePlugin, IWidgetPlugin
	{
        #region Fields

        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public LowestPricePlugin(IScheduleTaskService scheduleTaskService,
            ISettingService settingService)
        {
            _scheduleTaskService = scheduleTaskService;
            _settingService = settingService;
        }

        #endregion

        public bool HideInWidgetList => true;

        public string GetWidgetViewComponentName(string widgetZone)
        {
            return "LowestPrice";
        }

        public async Task<IList<string>> GetWidgetZonesAsync()
        {
            return new List<string>
            {
                PublicWidgetZones.ProductDetailsInsideOverviewButtonsBefore,
            };
        }

        public override async Task InstallAsync()
        {
            await CreateSettingsAsync();
            await CreateScheduleTasksAsync();
            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await DeleteSettingsAsync();
            await DeleteScheduleTaskAsync();
            await base.UninstallAsync();
        }

        private async Task CreateSettingsAsync()
        {
            var lowestPriceSettings = await _settingService.LoadSettingAsync<PriceHistorySettings>();
             lowestPriceSettings = new PriceHistorySettings
            {
                DisplayForDiscounts = true,
                DisplayForOldPrice = true,
                TimeSpan = TimeSpan.FromDays(31),
            };
            await _settingService.SaveSettingAsync(lowestPriceSettings);
        }

        private async Task DeleteSettingsAsync()
        {
            var lowestPriceSettings = await _settingService.LoadSettingAsync<PriceHistorySettings>();
            if (lowestPriceSettings != null)
            {
                await _settingService.DeleteSettingAsync<PriceHistorySettings>();
            }
        }

        private async Task CreateScheduleTasksAsync()
        {
            var updateLowestPricesTask = await _scheduleTaskService.GetTaskByTypeAsync(LowestPriceDefaults.UpdateLowestPricesTaskType);
            if(updateLowestPricesTask == null)
            {
                updateLowestPricesTask = new ScheduleTask
                {
                    Enabled = true,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = LowestPriceDefaults.UpdateLowestPricesTaskName,
                    Type = LowestPriceDefaults.UpdateLowestPricesTaskType,
                    Seconds = 14400,
                    StopOnError = true
                };

                await _scheduleTaskService.InsertTaskAsync(updateLowestPricesTask);
            }

            var deletePriceHistoryLogsTask = await _scheduleTaskService.GetTaskByTypeAsync(LowestPriceDefaults.DeletePriceHistoryLogsTaskType);
            if(deletePriceHistoryLogsTask == null)
            {
                deletePriceHistoryLogsTask = new ScheduleTask
                {
                    Enabled = true,
                    LastEnabledUtc = DateTime.UtcNow,
                    Name = LowestPriceDefaults.DeletePriceHistoryLogsTaskName,
                    Type = LowestPriceDefaults.DeletePriceHistoryLogsTaskType,
                    Seconds = 14400,
                    StopOnError = true
                };

                await _scheduleTaskService.InsertTaskAsync(deletePriceHistoryLogsTask);
            }
        }

        private async Task DeleteScheduleTaskAsync()
        {
            var updatePriceHistoryScheduleTask = await _scheduleTaskService.GetTaskByTypeAsync(LowestPriceDefaults.UpdateLowestPricesTaskType);
            if(updatePriceHistoryScheduleTask != null)
            {
                await _scheduleTaskService.DeleteTaskAsync(updatePriceHistoryScheduleTask);
            }

            var deletePriceHistoryLogsTask = await _scheduleTaskService.GetTaskByTypeAsync(LowestPriceDefaults.DeletePriceHistoryLogsTaskType);
            if (deletePriceHistoryLogsTask != null)
            {
                await _scheduleTaskService.DeleteTaskAsync(deletePriceHistoryLogsTask);
            }
        }
    }
}