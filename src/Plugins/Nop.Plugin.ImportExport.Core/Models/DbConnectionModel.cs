using System.ComponentModel.DataAnnotations;
using Nop.Data;

namespace Nop.Plugin.ImportExport.Core.Models
{
    public class DbConnectionModel : INopConnectionStringInfo
    {
        public string ConnectionString { get; set; }
        public bool ConnectionStringRaw { get; set; }
        public DataProviderType DataProvider { get; set; }
        //SQL Server properties
        public string SqlConnectionInfo { get; set; }

        public string ServerName { get; set; }
        public string DatabaseName { get; set; }
        public string Username { get; set; }
        [DataType(DataType.Password)]
        public string Password { get; set; }
        public bool IntegratedSecurity { get; set; }
    }
}
