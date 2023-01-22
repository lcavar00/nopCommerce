using Nop.Plugin.ImportExport.Core.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.ImportExport.Core.Services
{
    public interface IEntityMappingService
    {
        Task<IList<EntityMapping>> GetAllAsync();
        Task<EntityMapping> GetEntityMappingByIdAsync(int id);
        Task<EntityMapping> InsertAsync(EntityMapping entityMapping);
        Task<EntityMapping> UpdateAsync(EntityMapping entityMapping);
        Task DeleteAsync(EntityMapping entityMapping);
        Task DeleteEntityMappingByIdAsync(int entityMappingId);
    }
}
