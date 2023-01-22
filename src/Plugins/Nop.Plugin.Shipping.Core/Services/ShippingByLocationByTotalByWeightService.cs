using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Data;
using Nop.Plugin.Shipping.Core.Domain;

namespace Nop.Plugin.Shipping.Core.Services
{
    /// <summary>
    /// Represents service shipping by weight service implementation
    /// </summary>
    public partial class ShippingByLocationByTotalByWeightService : IShippingByLocationByTotalByWeightService
    {
        #region Constants

        /// <summary>
        /// Key for caching all records
        /// </summary>
        /// <remarks>
        /// {0} : page index
        /// {1} : page size
        /// </remarks>
        private readonly CacheKey _shippingByLocationByTotalByWeightAllKey = new CacheKey("Plugins.Shipping.Core.shippingbylocationbytotalbyweight.all", SHIPPINGBYLOCATIONBYTOTALBYWEIGHT_PATTERN_KEY);
        private const string SHIPPINGBYLOCATIONBYTOTALBYWEIGHT_PATTERN_KEY = "Nop.shippingbylocationbytotalbyweight.";

        #endregion

        #region Fields

        private readonly IRepository<ShippingByLocationByTotalByWeightRecord> _sbwtRepository;
        private readonly IStaticCacheManager _staticCacheManager;

        #endregion

        #region Ctor

        public ShippingByLocationByTotalByWeightService(IRepository<ShippingByLocationByTotalByWeightRecord> sbwtRepository,
            IStaticCacheManager staticCacheManager)
        {
            _sbwtRepository = sbwtRepository;
            _staticCacheManager = staticCacheManager;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get all shipping by weight records
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of the shipping by weight record</returns>
        public virtual async Task<IPagedList<ShippingByLocationByTotalByWeightRecord>> GetAllAsync(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var rez = await _sbwtRepository.GetAllAsync(query =>
            {
                return from sbw in query
                       orderby sbw.StoreId, sbw.CountryId, sbw.StateProvinceId, sbw.Zip, sbw.ShippingMethodId,
                           sbw.WeightFrom, sbw.OrderSubtotalFrom
                       select sbw;
            }, cache => cache.PrepareKeyForShortTermCache(_shippingByLocationByTotalByWeightAllKey));

            var records = new PagedList<ShippingByLocationByTotalByWeightRecord>(rez, pageIndex, pageSize);

            return records;
        }

        /// <summary>
        /// Filter Shipping Weight Records
        /// </summary>
        /// <param name="shippingMethodId">Shipping method identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="countryId">Country identifier</param>
        /// <param name="stateProvinceId">State identifier</param>
        /// <param name="zip">Zip postal code</param>
        /// <param name="weight">Weight</param>
        /// <param name="orderSubtotal">Order subtotal</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of the shipping by weight record</returns>
        public virtual async Task<IPagedList<ShippingByLocationByTotalByWeightRecord>> FindRecordsAsync(int shippingMethodId, int storeId, int warehouseId,
            int countryId, int stateProvinceId, string zip, decimal? weight, decimal? orderSubtotal, int pageIndex, int pageSize)
        {
            zip = zip?.Trim() ?? string.Empty;

            //filter by weight and shipping method
            var existingRates = (await GetAllAsync())
                .Where(sbw => sbw.ShippingMethodId == shippingMethodId && (!weight.HasValue || weight >= sbw.WeightFrom && weight <= sbw.WeightTo))
                .ToList();

            //filter by order subtotal
            var matchedBySubtotal = !orderSubtotal.HasValue ? existingRates :
                existingRates.Where(sbw => orderSubtotal >= sbw.OrderSubtotalFrom && orderSubtotal <= sbw.OrderSubtotalTo);

            //filter by store
            var matchedByStore = storeId == 0
                ? matchedBySubtotal
                : matchedBySubtotal.Where(r => r.StoreId == storeId || r.StoreId == 0);

            //filter by warehouse
            var matchedByWarehouse = warehouseId == 0
                ? matchedByStore
                : matchedByStore.Where(r => r.WarehouseId == warehouseId || r.WarehouseId == 0);

            //filter by country
            var matchedByCountry = countryId == 0
                ? matchedByWarehouse
                : matchedByWarehouse.Where(r => r.CountryId == countryId || r.CountryId == 0);

            //filter by state/province
            var matchedByStateProvince = stateProvinceId == 0
                ? matchedByCountry
                : matchedByCountry.Where(r => r.StateProvinceId == stateProvinceId || r.StateProvinceId == 0);

            //filter by zip
            var matchedByZip = string.IsNullOrEmpty(zip)
                ? matchedByStateProvince
                : matchedByStateProvince.Where(r => string.IsNullOrEmpty(r.Zip) || r.Zip.Equals(zip, StringComparison.InvariantCultureIgnoreCase));

            //sort from particular to general, more particular cases will be the first
            var foundRecords = matchedByZip.OrderBy(r => r.StoreId == 0).ThenBy(r => r.WarehouseId == 0)
                .ThenBy(r => r.CountryId == 0).ThenBy(r => r.StateProvinceId == 0)
                .ThenBy(r => string.IsNullOrEmpty(r.Zip));

            var records = new PagedList<ShippingByLocationByTotalByWeightRecord>(foundRecords.ToList(), pageIndex, pageSize);
            return records;
        }

        /// <summary>
        /// Get a shipping by weight record by passed parameters
        /// </summary>
        /// <param name="shippingMethodId">Shipping method identifier</param>
        /// <param name="storeId">Store identifier</param>
        /// <param name="warehouseId">Warehouse identifier</param>
        /// <param name="countryId">Country identifier</param>
        /// <param name="stateProvinceId">State identifier</param>
        /// <param name="zip">Zip postal code</param>
        /// <param name="weight">Weight</param>
        /// <param name="orderSubtotal">Order subtotal</param>
        /// <returns>Shipping by weight record</returns>
        public virtual async Task<ShippingByLocationByTotalByWeightRecord> FindRecordsAsync(int shippingMethodId, int storeId, int warehouseId, 
            int countryId, int stateProvinceId, string zip, decimal weight, decimal orderSubtotal)
        {
            var foundRecords = await FindRecordsAsync(shippingMethodId, storeId, warehouseId, countryId, stateProvinceId, zip, weight, orderSubtotal, 0, int.MaxValue);

            return foundRecords.FirstOrDefault();
        }

        /// <summary>
        /// Get a shipping by weight record by identifier
        /// </summary>
        /// <param name="shippingByWeightRecordId">Record identifier</param>
        /// <returns>Shipping by weight record</returns>
        public virtual async Task<ShippingByLocationByTotalByWeightRecord> GetByIdAsync(int shippingByWeightRecordId)
        {
            if (shippingByWeightRecordId == 0)
                return null;

            return await _sbwtRepository.GetByIdAsync(shippingByWeightRecordId);
        }

        /// <summary>
        /// Insert the shipping by weight record
        /// </summary>
        /// <param name="shippingByWeightRecord">Shipping by weight record</param>
        public virtual async void InsertShippingByLocationByTotalByWeightRecordAsync(ShippingByLocationByTotalByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException(nameof(shippingByWeightRecord));

            await _sbwtRepository.InsertAsync(shippingByWeightRecord);

            await _staticCacheManager.RemoveByPrefixAsync(SHIPPINGBYLOCATIONBYTOTALBYWEIGHT_PATTERN_KEY);
        }

        /// <summary>
        /// Update the shipping by weight record
        /// </summary>
        /// <param name="shippingByWeightRecord">Shipping by weight record</param>
        public virtual async void UpdateShippingByLocationByTotalByWeightRecordAsync(ShippingByLocationByTotalByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException(nameof(shippingByWeightRecord));

            await _sbwtRepository.UpdateAsync(shippingByWeightRecord);

            await _staticCacheManager.RemoveByPrefixAsync(SHIPPINGBYLOCATIONBYTOTALBYWEIGHT_PATTERN_KEY);
        }

        /// <summary>
        /// Delete the shipping by weight record
        /// </summary>
        /// <param name="shippingByWeightRecord">Shipping by weight record</param>
        public virtual async void DeleteShippingByLocationByTotalByWeightRecordAsync(ShippingByLocationByTotalByWeightRecord shippingByWeightRecord)
        {
            if (shippingByWeightRecord == null)
                throw new ArgumentNullException(nameof(shippingByWeightRecord));

            await _sbwtRepository.DeleteAsync(shippingByWeightRecord);

            await _staticCacheManager.RemoveByPrefixAsync(SHIPPINGBYLOCATIONBYTOTALBYWEIGHT_PATTERN_KEY);
        }

        #endregion
    }
}
