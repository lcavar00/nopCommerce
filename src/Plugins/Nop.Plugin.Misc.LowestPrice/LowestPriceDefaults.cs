using Nop.Plugin.Misc.LowestPrice.Tasks;

namespace Nop.Plugin.Misc.LowestPrice
{
    public static class LowestPriceDefaults
    {
        public static string UpdateLowestPricesTaskName = "Create price logs";
        public static string UpdateLowestPricesTaskType = typeof(CreatePriceHistoryLogsTask).FullName;

        public static string DeletePriceHistoryLogsTaskName = "Delete price history logs";
        public static string DeletePriceHistoryLogsTaskType = typeof(DeletePriceHistoryLogsTask).FullName;
    }
}
