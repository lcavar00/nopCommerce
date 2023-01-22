using Nop.Data;
using Nop.Plugin.ImportExport.Core.Domain;
using Nop.Plugin.ImportExport.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.ImportExport.Core.Services
{
    public class DataEntryMappingService : IDataEntryMappingService
    {
        #region Fields

        private readonly IRepository<DataEntryMapping> _dataEntryMappingRepository;

        #endregion

        #region Ctor

        public DataEntryMappingService(IRepository<DataEntryMapping> dataEntryMappingRepository)
        {
            _dataEntryMappingRepository = dataEntryMappingRepository;
        }

        #endregion

        /// <summary>
        /// Gets a list of all data entry mappings.
        /// </summary>
        /// <returns>A list with all data entry mappings</returns>
        public async Task<IList<DataEntryMapping>> GetAllAsync()
        {
            var dataEntryMappings = await _dataEntryMappingRepository.Table.ToListAsync();

            return dataEntryMappings;
        }

        /// <summary>
        /// Gets data entry mappings by entity mapping Id.
        /// </summary>
        /// <param name="entityMappingId">Entity mapping Id</param>
        /// <returns>a list of data entry mappings</returns>
        public async Task<IList<DataEntryMapping>> GetDataEntryMappingsByEntityMappingIdAsync(int entityMappingId)
        {
            var dataEntryMappings = await _dataEntryMappingRepository.Table.Where(a => a.EntityMappingId == entityMappingId && !a.Deleted).ToListAsync();

            return dataEntryMappings;
        }

        /// <summary>
        /// Gets a data entry mapping by it's Id.
        /// </summary>
        /// <param name="dataEntryMappingId">Id of the data entry mapping</param>
        /// <returns>Data entry mapping</returns>
        public async Task<DataEntryMapping> GetDataEntryMappingByIdAsync(int dataEntryMappingId)
        {
            var dataEntryMapping = await _dataEntryMappingRepository.GetByIdAsync(dataEntryMappingId);

            return dataEntryMapping;
        }

        /// <summary>
        /// Inserts a data entry mapping into the database.
        /// </summary>
        /// <param name="dataEntryMapping">Data entry mapping to insert</param>
        /// <returns>Awaitable task</returns>
        public async Task InsertAsync(DataEntryMapping dataEntryMapping)
        {
            dataEntryMapping.CreatedAtUtc = DateTime.UtcNow;
            await _dataEntryMappingRepository.InsertAsync(dataEntryMapping);
        }

        /// <summary>
        /// Inserts multiple data entry mappings into the database.
        /// </summary>
        /// <param name="dataEntryMappings">Data entry mappings to insert</param>
        /// <returns>Awaitable task</returns>
        public async Task InsertAsync(IEnumerable<DataEntryMapping> dataEntryMappings)
        {
            foreach(var dataEntryMapping in dataEntryMappings)
            {
                dataEntryMapping.CreatedAtUtc = DateTime.UtcNow;
            }

            await _dataEntryMappingRepository.InsertAsync(dataEntryMappings.ToList());
        }

        /// <summary>
        /// Inserts multiple data entry mappings into the database in batches.
        /// </summary>
        /// <param name="dataEntryMappings">Data entry mappings to insert</param>
        /// <returns>Awaitable task</returns>
        public async Task BatchInsertAsync(IEnumerable<DataEntryMapping> dataEntryMappings, int batchSize = 50)
        {
            foreach (var dataEntryMapping in dataEntryMappings)
            {
                dataEntryMapping.CreatedAtUtc = DateTime.UtcNow;
            }

            await _dataEntryMappingRepository.BatchInsertAsync(dataEntryMappings.ToList(), batchSize);
        }

        /// <summary>
        /// Updates an existing data entry mapping in the database.
        /// </summary>
        /// <param name="dataEntryMapping">Data entry mapping to update</param>
        /// <returns>Awaitable task</returns>
        public async Task UpdateAsync(DataEntryMapping dataEntryMapping)
        {
            dataEntryMapping.UpdatedAtUtc = DateTime.UtcNow;
            await _dataEntryMappingRepository.UpdateAsync(dataEntryMapping);
        }

        /// <summary>
        /// Updates multiple existing data entry mappings in the database.
        /// </summary>
        /// <param name="dataEntryMappings">Data entry mappings to update</param>
        /// <returns>Awaitable task</returns>
        public async Task UpdateAsync(IEnumerable<DataEntryMapping> dataEntryMappings)
        {
            foreach(var dataEntryMapping in dataEntryMappings)
            {
                dataEntryMapping.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _dataEntryMappingRepository.UpdateAsync(dataEntryMappings.ToList());
        }

        /// <summary>
        /// Updates multiple existing data entry mappings in the database.
        /// </summary>
        /// <param name="dataEntryMappings">Data entry mappings to update</param>
        /// <returns>Awaitable task</returns>
        public async Task BatchUpdateAsync(IEnumerable<DataEntryMapping> dataEntryMappings, int batchSize = 50)
        {
            foreach (var dataEntryMapping in dataEntryMappings)
            {
                dataEntryMapping.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _dataEntryMappingRepository.BatchUpdateAsync(dataEntryMappings.ToList(), batchSize);
        }

        /// <summary>
        /// Deletes a data entry mapping from the database.
        /// </summary>
        /// <param name="dataEntryMapping">Data entry mapping to delete</param>
        public async Task DeleteAsync(DataEntryMapping dataEntryMapping)
        {
            dataEntryMapping.Deleted = true;
            await _dataEntryMappingRepository.UpdateAsync(dataEntryMapping);
        }

        /// <summary>
        /// Deletes multiple data entry mappings from the database.
        /// </summary>
        /// <param name="dataEntryMappings">Data entry mappings to delete</param>
        /// <returns>Awaitable task</returns>
        public async Task DeleteAsync(IEnumerable<DataEntryMapping> dataEntryMappings)
        {
            foreach(var dataEntryMapping in dataEntryMappings)
            {
                dataEntryMapping.Deleted = true;
            }

            await _dataEntryMappingRepository.UpdateAsync(dataEntryMappings.ToList());
        }

        /// <summary>
        /// Deletes multiple data entry mappings from the database.
        /// </summary>
        /// <param name="dataEntryMappings">Data entry mappings to delete</param>
        /// <returns>Awaitable task</returns>
        public async Task BatchDeleteAsync(IEnumerable<DataEntryMapping> dataEntryMappings, int batchSize = 50)
        {
            foreach (var dataEntryMapping in dataEntryMappings)
            {
                dataEntryMapping.Deleted = true;
            }

            await _dataEntryMappingRepository.BatchUpdateAsync(dataEntryMappings.ToList(), batchSize);
        }

        /// <summary>
        /// Finds a data entry mapping using it's Id and deletes it from the database .
        /// </summary>
        /// <param name="dataEntryMappingId">Id of the data entry mapping to delete</param>
        public async Task DeleteByIdAsync(int dataEntryMappingId)
        {
            var dataEntryMapping = await _dataEntryMappingRepository.GetByIdAsync(dataEntryMappingId);

            if(dataEntryMapping != null)
            {
                dataEntryMapping.Deleted = true;
                await _dataEntryMappingRepository.UpdateAsync(dataEntryMapping);
            }
        }
    }
}
