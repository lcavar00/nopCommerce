using Nop.Core;
using Nop.Plugin.Misc.MorePrices.Domain;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.MorePrices.Services
{
    public partial interface IPriceService
    {
        #region Prices

        /// <summary>
        /// Delete a price
        /// </summary>
        /// <param name="price">Price</param>
        void DeletePrice(Price price);

        /// <summary>
        /// Delete prices
        /// </summary>
        /// <param name="prices">Prices</param>
        void DeletePrices(IList<Price> prices);


        /// <summary>
        /// Gets price
        /// </summary>
        /// <param name="priceId">Price identifier</param>
        /// <returns>Price</returns>
        Price GetPriceById(int priceId);

        /// <summary>
        /// Gets prices by identifier
        /// </summary>
        /// <param name="priceIds">Price identifiers</param>
        /// <returns>Prices</returns>
        IList<Price> GetPricesByIds(int[] priceIds);

        /// <summary>
        /// Inserts a price
        /// </summary>
        /// <param name="price">Price</param>
        void InsertPrice(Price price);

        /// <summary>
        /// Updates the price
        /// </summary>
        /// <param name="price">Price</param>
        void UpdatePrice(Price price);

        /// <summary>
        /// Updates the prices
        /// </summary>
        /// <param name="prices">Price</param>
        void UpdatePrices(IList<Price> prices);

        /// <summary>
        /// Get number of price (published and visible) in certain category
        /// </summary>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <returns>Number of prices</returns>
        int GetNumberOfPricesInCategory(IList<int> categoryIds = null, int storeId = 0);

        /// <summary>
        /// Search prices
        /// </summary>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="filteredSpecs">Filtered price specification identifiers</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <param name="overridePublished">
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" prices
        /// false - load only "Unpublished" prices
        /// </param>
        /// <returns>Prices</returns>
        IPagedList<Price> SearchPrices(
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            int languageId = 0,
            IList<int> filteredSpecs = null);

        /// <summary>
        /// Search prices
        /// </summary>
        /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded prices (all pages)</param>
        /// <param name="loadFilterableSpecificationAttributeOptionIds">A value indicating whether we should load the specification attribute option identifiers applied to loaded prices (all pages)</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="categoryIds">Category identifiers</param>
        /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
        /// <param name="storeId">Store identifier; 0 to load all records</param>
        /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
        /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
        /// <param name="priceTagId">Price tag identifier; 0 to load all records</param>
        /// <param name="keywords">Keywords</param>
        /// <param name="languageId">Language identifier (search for text searching)</param>
        /// <param name="orderBy">Order by</param>
        /// null - process "Published" property according to "showHidden" parameter
        /// true - load only "Published" prices
        /// false - load only "Unpublished" prices
        /// </param>
        /// <returns>Prices</returns>
        IPagedList<Price> SearchPrices(
            out IList<int> filterableSpecificationAttributeOptionIds,
            bool loadFilterableSpecificationAttributeOptionIds = false,
            int pageIndex = 0,
            int pageSize = int.MaxValue,
            IList<int> categoryIds = null,
            int manufacturerId = 0,
            int storeId = 0,
            int vendorId = 0,
            int warehouseId = 0,
            int priceTagId = 0,
            string keywords = null,
            int languageId = 0);

        /// <summary>
        /// Update price store mappings
        /// </summary>
        /// <param name="price">Price</param>
        /// <param name="limitedToStoresIds">A list of store ids for mapping</param>
        void UpdatePriceStoreMappings(Price price, IList<int> limitedToStoresIds);

        #endregion


        //#region Tier prices

        ///// <summary>
        ///// Gets a price tier prices for customer
        ///// </summary>
        ///// <param name="price">Price</param>
        ///// <param name="customer">Customer</param>
        ///// <param name="storeId">Store identifier</param>
        //IList<TierPrice> GetTierPrices(Price price, Customer customer, int storeId);

        ///// <summary>
        ///// Gets a tier prices by price identifier
        ///// </summary>
        ///// <param name="priceId">Price identifier</param>
        //IList<TierPrice> GetTierPricesByPrice(int priceId);

        ///// <summary>
        ///// Deletes a tier price
        ///// </summary>
        ///// <param name="tierPrice">Tier price</param>
        //void DeleteTierPrice(TierPrice tierPrice);

        ///// <summary>
        ///// Gets a tier price
        ///// </summary>
        ///// <param name="tierPriceId">Tier price identifier</param>
        ///// <returns>Tier price</returns>
        //TierPrice GetTierPriceById(int tierPriceId);

        ///// <summary>
        ///// Inserts a tier price
        ///// </summary>
        ///// <param name="tierPrice">Tier price</param>
        //void InsertTierPrice(TierPrice tierPrice);

        ///// <summary>
        ///// Updates the tier price
        ///// </summary>
        ///// <param name="tierPrice">Tier price</param>
        //void UpdateTierPrice(TierPrice tierPrice);

        ///// <summary>
        ///// Gets a preferred tier price
        ///// </summary>
        ///// <param name="price">Price</param>
        ///// <param name="customer">Customer</param>
        ///// <param name="storeId">Store identifier</param>
        ///// <param name="quantity">Quantity</param>
        ///// <returns>Tier price</returns>
        //TierPrice GetPreferredTierPrice(Price price, Customer customer, int storeId, int quantity);

        //#endregion
    }
}