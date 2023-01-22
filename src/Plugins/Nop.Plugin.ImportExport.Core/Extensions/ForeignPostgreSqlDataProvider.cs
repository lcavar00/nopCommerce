using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Common;
using LinqToDB.Data;
using LinqToDB.DataProvider;
using LinqToDB.SqlQuery;
using Nop.Core;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Data.DataProviders;
using Nop.Data.DataProviders.LinqToDB;
using Nop.Data.Migrations;
using Npgsql;

namespace Nop.Plugin.ImportExport.Core.Extensions
{
    public class ForeignPostgreSqlDataProvider : BaseDataProvider, INopDataProvider
    {
        public int SupportedLengthOfBinaryHash => throw new NotImplementedException();

        public bool BackupSupported => throw new NotImplementedException();

        protected override IDataProvider LinqToDbDataProvider => throw new NotImplementedException();

        public Task BackupDatabaseAsync(string fileName)
        {
            throw new NotImplementedException();
        }

        public string BuildConnectionString(INopConnectionStringInfo nopConnectionString)
        {
            throw new NotImplementedException();
        }

        public void CreateDatabase(string collation, int triesToConnect = 10)
        {
            throw new NotImplementedException();
        }

        public string CreateForeignKeyName(string foreignTable, string foreignColumn, string primaryTable, string primaryColumn)
        {
            throw new NotImplementedException();
        }

        public bool DatabaseExists()
        {
            throw new NotImplementedException();
        }

        public Task<bool> DatabaseExistsAsync()
        {
            throw new NotImplementedException();
        }

        public string GetIndexName(string targetTable, string targetColumn)
        {
            throw new NotImplementedException();
        }

        public Task<int?> GetTableIdentAsync<TEntity>() where TEntity : BaseEntity
        {
            throw new NotImplementedException();
        }

        public Task ReIndexTablesAsync()
        {
            throw new NotImplementedException();
        }

        public Task RestoreDatabaseAsync(string backupFileName)
        {
            throw new NotImplementedException();
        }

        public Task SetTableIdentAsync<TEntity>(int ident) where TEntity : BaseEntity
        {
            throw new NotImplementedException();
        }

        protected override DbConnection GetInternalDbConnection(string connectionString)
        {
            throw new NotImplementedException();
        }
    }
}