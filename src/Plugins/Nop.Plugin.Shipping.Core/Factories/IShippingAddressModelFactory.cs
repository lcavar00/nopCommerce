using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Stores;

namespace Nop.Plugin.Shipping.Core.Factories
{
    public interface IShippingAddressModelFactory
    {
        Task<IList<KeyValuePair<int, string>>> PrepareAddressesAsync(Store store);
        Task<IList<KeyValuePair<int, string>>> PrepareAddressesAsync();
        Task<IList<KeyValuePair<int, string>>> PrepareAddressesAsync(int storeId);
        Task<IList<KeyValuePair<int, string>>> PrepareAddressesAsync(string storeName);
    }
}