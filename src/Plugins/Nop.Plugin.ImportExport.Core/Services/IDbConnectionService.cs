using System.Threading.Tasks;
using Nop.Data.Configuration;

namespace Nop.Plugin.ImportExport.Core.Services
{
    public interface IDbConnectionService
    {
        Task<DataConfig> GetForeignDataSettingsAsync();
        Task<string> GetForeignConnectionStringAsync();
        Task<string> GetForeignDbNameAsync();
        Task<string> GetForeignServerNameAsync();
        string GetNopDbName();
        string GetNopConnectionString();
        Task<string> GetAuthentificationType();
        Task<string> GetSqlUsername();
    }
}
