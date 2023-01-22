using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;

namespace Nop.Plugin.ImportExport.Core.Extensions
{
    public static class Extensions
    {
        public static async Task BatchDeleteAsync<TEntity>(this IRepository<TEntity> repository, IEnumerable<TEntity> entities, int batchSize = 50) where TEntity : BaseEntity
        {
            var entitiesChunks = entities.Split(batchSize);

            foreach (var entitiesChunk in entitiesChunks)
            {
                await repository.DeleteAsync(entitiesChunk);
            }
        }

        public static async Task BatchInsertAsync<TEntity>(this IRepository<TEntity> repository, IEnumerable<TEntity> entities, int batchSize = 50) where TEntity : BaseEntity
        {
            var entitiesChunks = entities.Split(batchSize);

            foreach (var entitiesChunk in entitiesChunks)
            {
                await repository.InsertAsync(entitiesChunk);
            }
        }

        public static async Task BatchUpdateAsync<TEntity>(this IRepository<TEntity> repository, IEnumerable<TEntity> entities, int batchSize = 50) where TEntity : BaseEntity
        {
            var entitiesChunks = entities.Split(batchSize);

            foreach (var entitiesChunk in entitiesChunks)
            {
                await repository.UpdateAsync(entitiesChunk);
            }
        }

        private static IList<List<T>> Split<T>(this IEnumerable<T> source, int batchSize)
        {
            return source
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / batchSize)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();
        }
    }
}
