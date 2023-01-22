using Nop.Plugin.ImportExport.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.ImportExport.Core.Services
{
    public interface IDataEntryMappingService
    {
        /// <summary>
        /// Gets a list of all data entry mappings.
        /// </summary>
        /// <returns>A list with all data entry mappings</returns>
        Task<IList<DataEntryMapping>> GetAllAsync();

        /// <summary>
        /// Gets a data entry mapping by it's Id.
        /// </summary>
        /// <param name="dataEntryMappingId">Id of the data entry mapping</param>
        /// <returns>Data entry mapping</returns>
        Task<DataEntryMapping> GetDataEntryMappingByIdAsync(int dataEntryMappingId);

        /// <summary>
        /// Gets data entry mappings by entity mapping Id.
        /// </summary>
        /// <param name="entityMappingId">Entity mapping Id</param>
        /// <returns>a list of data entry mappings</returns>
        Task<IList<DataEntryMapping>> GetDataEntryMappingsByEntityMappingIdAsync(int entityMappingId);

        /// <summary>
        /// Inserts a data entry mapping into the database.
        /// </summary>
        /// <param name="dataEntryMapping">Data entry mapping to insert</param>
        /// <returns>Awaitable task</returns>
        Task InsertAsync(DataEntryMapping dataEntryMapping);

        /// <summary>
        /// Inserts multiple data entry mappings into the database in batches.
        /// </summary>
        /// <param name="dataEntryMappings">Data entry mappings to insert</param>
        /// <returns>Awaitable task</returns>
        Task BatchInsertAsync(IEnumerable<DataEntryMapping> dataEntryMappings, int batchSize = 50);

        /// <summary>
        /// Inserts multiple data entry mappings into the database.
        /// </summary>
        /// <param name="dataEntryMappings">Data entry mappings to insert</param>
        /// <returns>Awaitable task</returns>
        Task InsertAsync(IEnumerable<DataEntryMapping> dataEntryMappings);

        /// <summary>
        /// Updates an existing data entry mapping in the database.
        /// </summary>
        /// <param name="dataEntryMapping">Data entry mapping to update</param>
        /// <returns>Awaitable task</returns>
        Task UpdateAsync(DataEntryMapping dataEntryMapping);

        /// <summary>
        /// Updates multiple existing data entry mappings in the database.
        /// </summary>
        /// <param name="dataEntryMappings">Data entry mappings to update</param>
        /// <returns>Awaitable task</returns>
        Task UpdateAsync(IEnumerable<DataEntryMapping> dataEntryMappings);

        /// <summary>
        /// Updates multiple existing data entry mappings in the database in batches.
        /// </summary>
        /// <param name="dataEntryMappings">Data entry mappings to update</param>
        /// <returns>Awaitable task</returns>
        Task BatchUpdateAsync(IEnumerable<DataEntryMapping> dataEntryMappings, int batchSize = 50);

        /// <summary>
        /// Deletes a data entry mapping from the database.
        /// </summary>
        /// <param name="dataEntryMapping">Data entry mapping to delete</param>
        Task DeleteAsync(DataEntryMapping dataEntryMapping);

        /// <summary>
        /// Deletes multiple data entry mappings from the database.
        /// </summary>
        /// <param name="dataEntryMappings">Data entry mappings to delete</param>
        /// <returns>Awaitable task</returns>
        Task DeleteAsync(IEnumerable<DataEntryMapping> dataEntryMappings);

        /// <summary>
        /// Deletes multiple data entry mappings from the database in batches.
        /// </summary>
        /// <param name="dataEntryMappings">Data entry mappings to delete</param>
        /// <returns>Awaitable task</returns>
        Task BatchDeleteAsync(IEnumerable<DataEntryMapping> dataEntryMappings, int batchSize = 50);

        /// <summary>
        /// Finds a data entry mapping using it's Id and deletes it from the database .
        /// </summary>
        /// <param name="dataEntryMapping">Id of the data entry mapping to delete</param>
        Task DeleteByIdAsync(int dataEntryMapping);
    }
}
