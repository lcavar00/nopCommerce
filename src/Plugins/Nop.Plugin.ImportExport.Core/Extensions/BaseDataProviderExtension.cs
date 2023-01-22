using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using LinqToDB;
using LinqToDB.Data;
using LinqToDB.Mapping;
using LinqToDB.Tools;
using Nop.Core.Infrastructure;
using Nop.Data.DataProviders;
using Nop.Data.Mapping;
using StackExchange.Profiling.Data;

namespace Nop.Plugin.ImportExport.Core.Extensions
{
    public abstract class BaseDataProviderExtension : BaseDataProvider
    {
        /// <summary>
        /// Gets an additional mapping schema
        /// </summary>
        private MappingSchema GetMappingSchema()
        {
            if (Singleton<MappingSchema>.Instance is null)
            {
                var mappings = new MappingSchema(ConfigurationName)
                {
                    MetadataReader = new FluentMigratorMetadataReader(this)
                };

                if (MiniProfillerEnabled)
                {
                    mappings.SetConvertExpression<ProfiledDbConnection, IDbConnection>(db => db.WrappedConnection);
                    mappings.SetConvertExpression<ProfiledDbDataReader, IDataReader>(db => db.WrappedReader);
                    mappings.SetConvertExpression<ProfiledDbTransaction, IDbTransaction>(db => db.WrappedTransaction);
                    mappings.SetConvertExpression<ProfiledDbCommand, IDbCommand>(db => db.InternalCommand);
                }

                Singleton<MappingSchema>.Instance = mappings;
            }

            return Singleton<MappingSchema>.Instance;
        }

        /// <summary>
        /// Returns queryable source for specified mapping class for current connection,
        /// mapped to database table or view.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>Queryable source</returns>
        public virtual IQueryable<TEntity> GetNonBaseEntityTable<TEntity>() where TEntity : class
        {
            return new DataContext(LinqToDbDataProvider, GetCurrentConnectionString()) { MappingSchema = GetMappingSchema() }
                .GetTable<TEntity>();
        }

        #region Extension methods

        /// <summary>
        /// Insert a new entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the entity
        /// </returns>
        public virtual async Task<TEntity> InsertNonBaseEntityAsync<TEntity>(TEntity entity) where TEntity : class
        {
            using var dataContext = CreateDataConnection();

            return entity;
        }

        /// <summary>
        /// Insert a new entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Entity</returns>
        public virtual TEntity InsertNonBaseEntity<TEntity>(TEntity entity) where TEntity : class
        {
            using var dataContext = CreateDataConnection();
            return entity;
        }

        /// <summary>
        /// Updates record in table, using values from entity parameter. 
        /// Record to update identified by match on primary key value from obj value.
        /// </summary>
        /// <param name="entity">Entity with data to update</param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateNonBaseEntityAsync<TEntity>(TEntity entity) where TEntity : class
        {
            using var dataContext = CreateDataConnection();
            await dataContext.UpdateAsync(entity);
        }

        /// <summary>
        /// Updates records in table, using values from entity parameter. 
        /// Records to update are identified by match on primary key value from obj value.
        /// </summary>
        /// <param name="entities">Entities with data to update</param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task UpdateNonBaseEntitiesAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            //we don't use the Merge API on this level, because this API not support all databases.
            //you may see all supported databases by the following link: https://linq2db.github.io/articles/sql/merge/Merge-API.html#supported-databases
            foreach (var entity in entities)
                await UpdateNonBaseEntityAsync(entity);
        }

        /// <summary>
        /// Deletes record in table. Record to delete identified
        /// by match on primary key value from obj value.
        /// </summary>
        /// <param name="entity">Entity for delete operation</param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task DeleteNonBaseEntityAsync<TEntity>(TEntity entity) where TEntity : class
        {
            using var dataContext = CreateDataConnection();
            await dataContext.DeleteAsync(entity);
        }

        /// <summary>
        /// Performs delete records in a table
        /// </summary>
        /// <param name="entities">Entities for delete operation</param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task BulkDeleteNonBaseEntitiesAsync<TEntity>(IList<TEntity> entities) where TEntity : class
        {
            using var dataContext = CreateDataConnection();
            await dataContext.GetTable<TEntity>()
                   .Where(e => entities.Contains(e))
                   .DeleteAsync();
        }

        /// <summary>
        /// Performs delete records in a table by a condition
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the number of deleted records
        /// </returns>
        public async Task<int> BulkDeleteNonBaseEntitiesAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            using var dataContext = CreateDataConnection();
            return await dataContext.GetTable<TEntity>()
                .Where(predicate)
                .DeleteAsync();
        }

        /// <summary>
        /// Performs bulk insert entities operation
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entities">Collection of Entities</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task BulkInsertNonBaseEntitiesAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class
        {
            using var dataContext = CreateDataConnection(LinqToDbDataProvider);
            await dataContext.BulkCopyAsync(new BulkCopyOptions(), entities.RetrieveIdentity(dataContext));
        }

        #endregion
    }
}
