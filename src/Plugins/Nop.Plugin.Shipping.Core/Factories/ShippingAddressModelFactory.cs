using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Data;
using Nop.Services.Directory;

namespace Nop.Plugin.Shipping.Core.Factories
{
    public class ShippingAddressModelFactory : IShippingAddressModelFactory
    {
        #region Fields

        private readonly IRepository<Address> _addressRepository;
        private readonly ICountryService _countryService;
        private readonly IRepository<Store> _storeRepository;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly ShippingSettings _shippingSettings;

        #endregion

        #region Ctor

        public ShippingAddressModelFactory(ICountryService countryService,
            IRepository<Address> addressRepository,
            IRepository<Store> storeRepository,
            IRepository<Warehouse> warehouseRepository,
            ShippingSettings shippingSettings)
        {
            _countryService = countryService;
            _addressRepository = addressRepository;
            _storeRepository = storeRepository;
            _warehouseRepository = warehouseRepository;
            _shippingSettings = shippingSettings;
        }

        #endregion

        public async Task<IList<KeyValuePair<int, string>>> PrepareAddressesAsync(Store store)
        {
            var addresses = new List<Address>();

            var warehouseAddresses = (from address in _addressRepository.Table
                                      where _warehouseRepository.Table.Any(a => a.AddressId == address.Id)
                                      select address).ToList();

            addresses.AddRange(warehouseAddresses);

            var defaultAddress = await _addressRepository.GetByIdAsync(_shippingSettings.ShippingOriginAddressId);
            addresses.Add(defaultAddress);

            var formattedAddresses = new List<KeyValuePair<int, string>>();
            foreach (var address in addresses)
            {
                var houseNumber = address.Address2;

                formattedAddresses.Add(new KeyValuePair<int, string>(address.Id, $"{address.Address1} {houseNumber} {address.ZipPostalCode}, {address.City}, {(await _countryService.GetCountryByAddressAsync(address)).ThreeLetterIsoCode}"));
            }

            return formattedAddresses;
        }

        public async Task<IList<KeyValuePair<int, string>>> PrepareAddressesAsync()
        {
            var addresses = new List<Address>();

            var warehouseAddresses = (from address in _addressRepository.Table
                                      where _warehouseRepository.Table.Any(a => a.AddressId == address.Id)
                                      select address).ToList();

            addresses.AddRange(warehouseAddresses);

            var defaultAddress = await _addressRepository.GetByIdAsync(_shippingSettings.ShippingOriginAddressId);
            addresses.Add(defaultAddress);

            var formattedAddresses = new List<KeyValuePair<int, string>>();
            foreach(var address in addresses)
            {
                var houseNumber = address.Address2;

                formattedAddresses.Add(new KeyValuePair<int, string>(address.Id, $"{address.Address1 ?? string.Empty} {houseNumber} {address.ZipPostalCode ?? string.Empty}, {address.City ?? string.Empty}, {(await _countryService.GetCountryByAddressAsync(address))?.Name ?? string.Empty}"));
            }

            return formattedAddresses;
        }

        public async Task<IList<KeyValuePair<int, string>>> PrepareAddressesAsync(int storeId)
        {
            var store = _storeRepository.Table.FirstOrDefault(a => a.Id == storeId);

            return await PrepareAddressesAsync(store);
        }

        public async Task<IList<KeyValuePair<int, string>>> PrepareAddressesAsync(string storeName)
        {
            var store = _storeRepository.Table.FirstOrDefault(a => a.Name == storeName);

            return await PrepareAddressesAsync(store);
        }
    }
}
