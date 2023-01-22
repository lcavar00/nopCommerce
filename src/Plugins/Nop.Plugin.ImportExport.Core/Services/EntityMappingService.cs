using Nop.Data;
using Nop.Plugin.ImportExport.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.ImportExport.Core.Services
{
    public class EntityMappingService : IEntityMappingService
    {
        #region Fields

        private readonly IRepository<EntityMapping> _entityMappingRepository;

        #endregion

        #region Ctor

        public EntityMappingService(IRepository<EntityMapping> entityMappingRepository)
        {
            _entityMappingRepository = entityMappingRepository;
        }

        #endregion

        /// <summary>
        /// Gets all entity mappings.
        /// </summary>
        /// <returns>List of entity mappings</returns>
        public async Task<IList<EntityMapping>> GetAllAsync()
        {
            var entityMappings = await _entityMappingRepository.Table.ToListAsync();

            return entityMappings;
        }

        /// <summary>
        /// Gets a entity mapping using it's Id.
        /// </summary>
        /// <param name="id">Id of the entity mapping</param>
        /// <returns>Table map</returns>
        public Task<EntityMapping> GetEntityMappingByIdAsync(int id)
        {
            return _entityMappingRepository.GetByIdAsync(id);
        }

        /// <summary>
        /// Inserts a new entity mapping into the database.
        /// </summary>
        /// <param name="entityMapping">EntityMapping to be inserted</param>
        public async Task<EntityMapping> InsertAsync(EntityMapping entityMapping)
        {
            entityMapping.CreatedAtUtc = DateTime.UtcNow;
            await _entityMappingRepository.InsertAsync(entityMapping);

            return entityMapping;
        }

        /// <summary>
        /// Updates a entity mapping.
        /// </summary>
        /// <param name="entityMapping">EntityMapping to be updated</param>
        public async Task<EntityMapping> UpdateAsync(EntityMapping entityMapping)
        {
            entityMapping.UpdatedAtUtc = DateTime.UtcNow;
            await _entityMappingRepository.UpdateAsync(entityMapping);

            return entityMapping;
        }

        /// <summary>
        /// Deletes a entity mapping.
        /// </summary>
        /// <param name="entityMapping">EntityMapping to be deleted</param>
        public async Task DeleteAsync(EntityMapping entityMapping)
        {
            entityMapping.Deleted = true;
            await _entityMappingRepository.UpdateAsync(entityMapping);
        }

        /// <summary>
        /// Deletes a entity mapping with given Id.
        /// </summary>
        /// <param name="tableMapId">Id of the EntityMapping to be deleted</param>
        public async Task DeleteEntityMappingByIdAsync(int tableMapId)
        {
            var entityMapping = await _entityMappingRepository.GetByIdAsync(tableMapId);
            if(entityMapping != null)
            {
                entityMapping.Deleted = true;
                await _entityMappingRepository.UpdateAsync(entityMapping);
            }
        }
    }
}
