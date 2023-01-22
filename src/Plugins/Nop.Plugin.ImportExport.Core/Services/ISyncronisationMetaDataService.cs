using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.ImportExport.Core.Domain;

namespace Nop.Plugin.ImportExport.Core.Services
{
    public interface ISyncronisationMetaDataService
    {
        Task<IList<EntityMapping>> GetEntityMappingsAsync();
        Task<IDictionary<EntityMapping, IList<DataEntryMapping>>> GetSyncronisationMetaData();
        Task<IDictionary<EntityMapping, IList<DataEntryMapping>>> GetSourceEntityDataEntryMappingsAsync<TNopEntity>() where TNopEntity : class;
        Task<IDictionary<EntityMapping, IList<DataEntryMapping>>> GetDestinationEntityDataEntryMappingsAsync<TEntity>() where TEntity : class;
        Task<KeyValuePair<EntityMapping, IList<DataEntryMapping>>> GetEntityDataEntryMappingsAsync<TSourceEntity, TDestinationEntity>();
    }
}