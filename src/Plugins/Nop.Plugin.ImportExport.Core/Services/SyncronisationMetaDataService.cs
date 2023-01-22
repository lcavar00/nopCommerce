using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Plugin.ImportExport.Core.Domain;

namespace Nop.Plugin.ImportExport.Core.Services
{
    public class SyncronisationMetaDataService : ISyncronisationMetaDataService
    {
        #region Fields

        private readonly IDataEntryMappingService _dataEntryMappingService;
        private readonly IEntityMappingService _entityMappingService;

        #endregion

        #region Ctor

        public SyncronisationMetaDataService(IDataEntryMappingService dataEntryMappingService,
            IEntityMappingService entityMappingService)
        {
            _dataEntryMappingService = dataEntryMappingService;
            _entityMappingService = entityMappingService;
        }

        #endregion

        public async Task<IDictionary<EntityMapping, IList<DataEntryMapping>>> GetSyncronisationMetaData()
        {
            var entityMappings = await GetEntityMappingsAsync();

            var entityMappingDictionary = new Dictionary<EntityMapping, IList<DataEntryMapping>>();
            foreach (var entityMapping in entityMappings)
            {
                var dataEntryMappings = await _dataEntryMappingService.GetDataEntryMappingsByEntityMappingIdAsync(entityMapping.Id);

                entityMappingDictionary.Add(entityMapping, dataEntryMappings);
            }

            return entityMappingDictionary;
        }

        public async Task<IList<EntityMapping>> GetEntityMappingsAsync()
        {
            return await _entityMappingService.GetAllAsync();
        }

        public async Task<IDictionary<EntityMapping, IList<DataEntryMapping>>> GetDestinationEntityDataEntryMappingsAsync<TNopEntity>() where TNopEntity : class
        {
            var entityMappings = (await GetEntityMappingsAsync()).Where(a => a.DestinationEntityName == typeof(TNopEntity).Name);

            var entityMappingDictionary = new Dictionary<EntityMapping, IList<DataEntryMapping>>();
            foreach(var entityMapping in entityMappings)
            {
                var dataEntryMappings = await _dataEntryMappingService.GetDataEntryMappingsByEntityMappingIdAsync(entityMapping.Id);

                entityMappingDictionary.Add(entityMapping, dataEntryMappings);
            }

            return entityMappingDictionary;
        }

        public async Task<IDictionary<EntityMapping, IList<DataEntryMapping>>> GetSourceEntityDataEntryMappingsAsync<TNopEntity>() where TNopEntity : class
        {
            var entityMappings = (await GetEntityMappingsAsync()).Where(a => a.SourceEntityName == typeof(TNopEntity).Name);

            var entityMappingDictionary = new Dictionary<EntityMapping, IList<DataEntryMapping>>();
            foreach (var entityMapping in entityMappings)
            {
                var dataEntryMappings = await _dataEntryMappingService.GetDataEntryMappingsByEntityMappingIdAsync(entityMapping.Id);

                entityMappingDictionary.Add(entityMapping, dataEntryMappings);
            }

            return entityMappingDictionary;
        }

        public async Task<KeyValuePair<EntityMapping, IList<DataEntryMapping>>> GetEntityDataEntryMappingsAsync<TSourceEntity, TDestinationEntity>()
        {
            var entityMapping = (await GetEntityMappingsAsync()).FirstOrDefault(a => a.SourceEntityName == typeof(TSourceEntity).Name && a.DestinationEntityName == typeof(TDestinationEntity).Name)
                ?? await _entityMappingService.InsertAsync(new EntityMapping
                {
                    CreatedAtUtc = DateTime.UtcNow,
                    Deleted = false,
                    DestinationEntityName = typeof(TDestinationEntity).Name,
                    SourceEntityName = typeof(TSourceEntity).Name,
                });


            var dataEntryMappings = await _dataEntryMappingService.GetDataEntryMappingsByEntityMappingIdAsync(entityMapping.Id);

            return new KeyValuePair<EntityMapping, IList<DataEntryMapping>>(entityMapping, dataEntryMappings);
        }
    }
}
