using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Infrastructure;
using Nop.Plugin.Payments.WsPay.Models;
using Nop.Services.Catalog;
using Nop.Services.Orders;
using Nop.Services.Tax;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Plugin.Payments.WsPay.Factories
{
    public class RatePaymentModelFactory : IRatePaymentModelFactory
    {
        #region Fields

        private readonly INopFileProvider _fileProvider;
        private readonly IProductService _productService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly ITaxService _taxService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public RatePaymentModelFactory(INopFileProvider fileProvider,
            IProductService productService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            ITaxService taxService,
            IWorkContext workContext)
        {
            _fileProvider = fileProvider;
            _productService = productService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _taxService = taxService;
            _workContext = workContext;
        }

        #endregion

        public async Task<RatePaymentModel> PrepareRatePaymentModelAsync()
        {
            var model = new RatePaymentModel();

            var shoppingCart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

            var subTotalExclTax = decimal.Zero;
            var subTotalInclTax = decimal.Zero;
            var subTotalTaxRates = new SortedDictionary<decimal, decimal>();

            foreach (var shoppingCartItem in shoppingCart)
            {
                var product = await _productService.GetProductByIdAsync(shoppingCartItem.ProductId);
                var itemSubTotalExclTax = product.Price;
                var productPriceTuple = (await _taxService.GetProductPriceAsync(product, product.Price));
                var itemSubTotalInclTax = productPriceTuple.price;
                var taxRate = productPriceTuple.taxRate;

                subTotalExclTax += itemSubTotalExclTax * shoppingCartItem.Quantity;
                subTotalInclTax += itemSubTotalInclTax * shoppingCartItem.Quantity;

                //tax rates
                var itemTaxValue = itemSubTotalInclTax - itemSubTotalExclTax;
                if (taxRate <= decimal.Zero || itemTaxValue <= decimal.Zero)
                    continue;

                if (!subTotalTaxRates.ContainsKey(taxRate))
                    subTotalTaxRates.Add(taxRate, itemTaxValue);
                else
                    subTotalTaxRates[taxRate] = subTotalTaxRates[taxRate] + itemTaxValue;
            }

            model.AllowRatePayment = false;
            model.Cards = await GetCardsForOrderTotalAsync(subTotalInclTax);
            if (model.Cards != null && model.Cards.Count > 0)
            {
                model.AllowRatePayment = true;
            }
            model.UseRatePayment = false;
            model.OrderTotalValue = subTotalInclTax;
            model.OrderTotal = subTotalInclTax.ToString();

            return model;
        }

        public async Task<List<CardSelectionModel>> GetCardsForOrderTotalAsync(decimal orderTotal)
        {
            var cards = new List<CardSelectionModel>();

            if (_fileProvider.FileExists(WsPayDefaults.RATE_PAYMENT_CARDS_FILE_PATH))
            {
                var json = _fileProvider.ReadAllText(WsPayDefaults.RATE_PAYMENT_CARDS_FILE_PATH, Encoding.Default);

                var ratePaymentDefaults = JsonConvert.DeserializeObject<RatePaymentDefaults>(json);
                var ratePaymentDefault = ratePaymentDefaults.CardDefaults.OrderBy(a => a.MinimalAmount).LastOrDefault(a => a.MinimalAmount <= orderTotal);

                if(ratePaymentDefault != null)
                {
                    foreach (var card in ratePaymentDefault.Cards)
                    {
                        var cardSelectionModel = new CardSelectionModel();
                        cardSelectionModel.CardName = card.Name;
                        cardSelectionModel.BankMaxRates = card.Banks.ToDictionary(a => a.Name, a => a.MaxRateNumber);

                        cards.Add(cardSelectionModel);
                    }
                }
            }

            return cards;
        }
    }
}
