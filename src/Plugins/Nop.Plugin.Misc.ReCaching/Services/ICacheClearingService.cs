using Nop.Core.Domain.Catalog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.ReCaching.Services
{
    public interface ICacheClearingService<T>
    {
        Task ClearCacheAsync();
        Task ClearCacheAsync(IEnumerable<Product> entities);
    }
}