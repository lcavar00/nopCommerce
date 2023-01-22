using Newtonsoft.Json;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.Configuration;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.ImportExport.Core.Services
{
    public class DbConnectionService : IDbConnectionService
    {
        private INopFileProvider _fileProvider;

        public DbConnectionService(INopFileProvider fileProvider)
        {
            _fileProvider = fileProvider;
        }

        public async Task<DataConfig> GetForeignDataSettingsAsync()
        {
            var filePath = _fileProvider.MapPath(ImportExportDefaults.ConnectionStringFilePath);

            string text = null;

            if(_fileProvider.FileExists(filePath))
            {
                text = await _fileProvider.ReadAllTextAsync(filePath, Encoding.UTF8);
            }

            if (string.IsNullOrEmpty(text))
                return null;

            var dataSettings = JsonConvert.DeserializeObject<DataConfig>(text);

            return dataSettings;
        }

        /// <summary>
        /// Gets source database connection string.
        /// </summary>
        /// <returns>Source database connection string</returns>
        public async Task<string> GetForeignConnectionStringAsync()
        {
            var dataSettings = await GetForeignDataSettingsAsync();

            if(dataSettings == null)
            {
                return null;
            }

            return dataSettings.ConnectionString;
        }


        /// <summary>
        /// Gets destination database connection string.
        /// </summary>
        /// <returns>Destination database connection string</returns>
        public string GetNopConnectionString()
        {
            var settings = DataSettingsManager.LoadSettings();
            
            return settings.ConnectionString;
        }

        /// <summary>
        /// Returns the name of the source database
        /// </summary>
        /// <returns>Database name as string</returns>
        public string GetNopDbName()
        {
            var connString = GetNopConnectionString();

            if (string.IsNullOrEmpty(connString))
                return string.Empty;

            var first = "Initial Catalog=";
            var last = ";Integrated Security";
            var pos1 = connString.IndexOf(first) + first.Length;
            var pos2 = connString.IndexOf(last);

            var databaseName = connString[pos1..pos2];

            return databaseName;
        }

        /// <summary>
        /// Returns the name of the destination database
        /// </summary>
        /// <returns>Database name as string</returns>
        public async Task<string> GetForeignDbNameAsync()
        {
            var connString = await GetForeignConnectionStringAsync();

            if (string.IsNullOrEmpty(connString))
                return string.Empty;

            var first = "Initial Catalog=";
            var last = ";Integrated Security";
            var pos1 = connString.IndexOf(first) + first.Length;
            var pos2 = connString.IndexOf(last);

            var databaseName = connString[pos1..pos2];

            return databaseName;
        }

        public async Task<string> GetForeignServerNameAsync()
        {
            var connString = await GetForeignConnectionStringAsync();

            if (string.IsNullOrEmpty(connString))
                return string.Empty;

            var first = "Data Source=";
            var last = ";Initial Catalog";
            var pos1 = connString.IndexOf(first) + first.Length;
            var pos2 = connString.IndexOf(last);

            var serverName = connString[pos1..pos2];

            return serverName;
        }

        public async Task<string> GetAuthentificationType()
        {
            var connString = await GetForeignConnectionStringAsync();

            if (string.IsNullOrEmpty(connString))
                return string.Empty;

            var first = "Integrated Security=";
            var last = ";Persist Security Info";
            var pos1 = connString.IndexOf(first) + first.Length;
            var pos2 = connString.IndexOf(last);

            var authentificationType = connString[pos1..pos2] == "True" ? "windowsauthentication" : "sqlauthentication";

            return authentificationType;
        }

        public async Task<string> GetSqlUsername()
        {
            var connString = (await GetForeignConnectionStringAsync());

            if (string.IsNullOrEmpty(connString))
                return string.Empty;

            var first = "User ID=";
            var last = ";Password";
            var pos1 = connString.IndexOf(first) + first.Length;
            var pos2 = connString.IndexOf(last);

            var userName = connString[pos1..pos2];

            return userName;
        }
    }
}
