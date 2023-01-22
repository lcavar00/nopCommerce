using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.MorePrices.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.MorePrices.Services
{
    public partial class PriceService : IPriceService
    {
        #region Fields

        private IRepository<Price> _priceRepository;

        #endregion

        #region Ctor

        public PriceService(IRepository<Price> priceRepository)
        {
            _priceRepository = priceRepository;
        }

        #endregion

        public void DeletePrice(Price price)
        {
            _priceRepository.Delete(price);
        }

        public void DeletePrices(IList<Price> prices)
        {
            _priceRepository.Delete(prices);
        }

        public int GetNumberOfPricesInCategory(IList<int> categoryIds = null, int storeId = 0)
        {
            throw new NotImplementedException();
        }

        public Price GetPriceById(int priceId)
        {
            return _priceRepository.GetById(priceId);
        }

        public IList<Price> GetPricesByIds(int[] priceIds)
        {
            var prices = new List<Price>();
            foreach(var priceId in priceIds)
            {
                prices.Add(_priceRepository.GetById(priceId));
            }
            return prices;
        }

        public void InsertPrice(Price price)
        {
            _priceRepository.Insert(price);
        }

        public IPagedList<Price> SearchPrices(int pageIndex = 0, int pageSize = int.MaxValue, IList<int> categoryIds = null, int manufacturerId = 0, int storeId = 0, int vendorId = 0, int warehouseId = 0, int languageId = 0, IList<int> filteredSpecs = null)
        {
            throw new NotImplementedException();
        }

        public IPagedList<Price> SearchPrices(out IList<int> filterableSpecificationAttributeOptionIds, bool loadFilterableSpecificationAttributeOptionIds = false, int pageIndex = 0, int pageSize = int.MaxValue, IList<int> categoryIds = null, int manufacturerId = 0, int storeId = 0, int vendorId = 0, int warehouseId = 0, int priceTagId = 0, string keywords = null, int languageId = 0)
        {
            throw new NotImplementedException();
        }

        public void UpdatePrice(Price price)
        {
            _priceRepository.Update(price);
        }

        public void UpdatePrices(IList<Price> prices)
        {
            _priceRepository.Update(prices);
        }

        public void UpdatePriceStoreMappings(Price price, IList<int> limitedToStoresIds)
        {
            throw new NotImplementedException();
        }

    }
}
