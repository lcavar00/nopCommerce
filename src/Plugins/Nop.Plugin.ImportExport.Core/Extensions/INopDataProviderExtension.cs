using Nop.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Nop.Plugin.ImportExport.Core.Extensions
{
    public partial interface INopDataProviderExtension : INopDataProvider
    {
        #region Methods

        /// <summary>
        /// Returns queryable source for specified mapping class for current connection,
        /// mapped to database table or view.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>Queryable source</returns>
        IQueryable<TEntity> GetNonBaseEntityTable<TEntity>() where TEntity : class;

        /// <summary>
        /// Insert a new entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the entity
        /// </returns>
        Task<TEntity> InsertNonBaseEntityAsync<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Insert a new entity
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entity">Entity</param>
        /// <returns>Entity</returns>
        TEntity InsertNonBaseEntity<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Updates record in table, using values from entity parameter. 
        /// Record to update identified by match on primary key value from obj value.
        /// </summary>
        /// <param name="entity">Entity with data to update</param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateNonBaseEntityAsync<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Updates records in table, using values from entity parameter. 
        /// Records to update are identified by match on primary key value from obj value.
        /// </summary>
        /// <param name="entities">Entities with data to update</param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task UpdateNonBaseEntitiesAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

        /// <summary>
        /// Deletes record in table. Record to delete identified
        /// by match on primary key value from obj value.
        /// </summary>
        /// <param name="entity">Entity for delete operation</param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task DeleteNonBaseEntityAsync<TEntity>(TEntity entity) where TEntity : class;

        /// <summary>
        /// Performs delete records in a table
        /// </summary>
        /// <param name="entities">Entities for delete operation</param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task BulkDeleteNonBaseEntitiesAsync<TEntity>(IList<TEntity> entities) where TEntity : class;

        /// <summary>
        /// Performs delete records in a table by a condition
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the number of deleted records
        /// </returns>
        Task<int> BulkDeleteNonBaseEntitiesAsync<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class;

        /// <summary>
        /// Performs bulk insert entities operation
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="entities">Collection of Entities</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        Task BulkInsertNonBaseEntitiesAsync<TEntity>(IEnumerable<TEntity> entities) where TEntity : class;

        #endregion

        #region Properties

        /// <summary>
        /// Name of database provider
        /// </summary>
        string ConfigurationName { get; }

        /// <summary>
        /// Gets allowed a limit input value of the data for hashing functions, returns 0 if not limited
        /// </summary>
        int SupportedLengthOfBinaryHash { get; }

        /// <summary>
        /// Gets a value indicating whether this data provider supports backup
        /// </summary>
        bool BackupSupported { get; }

        #endregion
    }
}
