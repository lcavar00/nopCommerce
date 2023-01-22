using System.Threading.Tasks;
using Nop.Plugin.Misc.LowestPrice.Models;

namespace Nop.Plugin.Misc.LowestPrice.Factories
{
    public interface ILowestPriceModelFactory
    {
        Task<LowestPriceModel> PrepareLowestProductPriceModelAsync(int productId);
    }
}
