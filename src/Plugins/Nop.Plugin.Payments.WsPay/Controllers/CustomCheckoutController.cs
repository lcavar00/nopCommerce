using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Http.Extensions;
using Nop.Plugin.Payments.WsPay.Domain;
using Nop.Plugin.Payments.WsPay.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;

namespace Nop.Plugin.Payments.WsPay.Controllers
{
    public class CustomCheckoutController : CheckoutController
    {
        #region Fields

        private readonly ICheckoutModelFactory _checkoutModelFactory;
        private readonly ICustomerService _customerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPaymentService _paymentService;
        private readonly IRatePaymentService _ratePaymentService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;

        #endregion

        #region Ctor

        public CustomCheckoutController(AddressSettings addressSettings,
            CustomerSettings customerSettings, 
            IAddressAttributeParser addressAttributeParser, 
            IAddressModelFactory addressModelFactory, 
            IAddressService addressService, 
            ICheckoutModelFactory checkoutModelFactory, 
            ICountryService countryService, 
            ICustomerService customerService, 
            IGenericAttributeService genericAttributeService, 
            ILocalizationService localizationService, 
            ILogger logger, 
            IOrderProcessingService orderProcessingService, 
            IOrderService orderService, 
            IPaymentPluginManager paymentPluginManager, 
            IPaymentService paymentService, 
            IProductService productService, 
            IRatePaymentService ratePaymentService,
            IShippingService shippingService, 
            IShoppingCartService shoppingCartService, 
            IStoreContext storeContext, 
            IWebHelper webHelper, 
            IWorkContext workContext, 
            OrderSettings orderSettings, 
            PaymentSettings paymentSettings, 
            RewardPointsSettings rewardPointsSettings, 
            ShippingSettings shippingSettings) : base(addressSettings, customerSettings, addressAttributeParser, addressModelFactory, addressService, checkoutModelFactory, countryService, customerService, genericAttributeService, localizationService, logger, orderProcessingService, orderService, paymentPluginManager, paymentService, productService, shippingService, shoppingCartService, storeContext, webHelper, workContext, orderSettings, paymentSettings, rewardPointsSettings, shippingSettings)
        {
            _checkoutModelFactory = checkoutModelFactory;
            _customerService = customerService;
            _genericAttributeService = genericAttributeService;
            _localizationService = localizationService;
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _paymentService = paymentService;
            _ratePaymentService = ratePaymentService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _webHelper = webHelper;
            _workContext = workContext;
            _orderSettings = orderSettings;
        }

        #endregion



        public override async Task<IActionResult> OpcSavePaymentInfo(IFormCollection form)
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                var paymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                var paymentMethod = await _paymentPluginManager
                    .LoadPluginBySystemNameAsync(paymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id)
                    ?? throw new Exception("Payment method is not selected");

                var warnings = await paymentMethod.ValidatePaymentFormAsync(form);
                foreach (var warning in warnings)
                    ModelState.AddModelError("", warning);
                if (ModelState.IsValid)
                {
                    var useRatePayment = form["UseRatePayment"].Any(a => a == "true");

                    if (useRatePayment)
                    {
                        var card = form["Card"].FirstOrDefault(a => !string.IsNullOrEmpty(a));
                        var bank = form["Bank"].FirstOrDefault(a => !string.IsNullOrEmpty(a));
                        var rates = form["Rates"].FirstOrDefault(a => !string.IsNullOrEmpty(a));

                        if (!string.IsNullOrEmpty(card))
                        {
                            var orderNoteText = $"{(await _localizationService.GetLocaleStringResourceByNameAsync("Plugins.Payments.WsPay.Checkout.Card", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ?? string.Empty}{card}";
                            HttpContext.Session.SetString("Card", card);
                        }
                        if (!string.IsNullOrEmpty(bank))
                        {
                            var orderNoteText = $"{(await _localizationService.GetLocaleStringResourceByNameAsync("Plugins.Payments.WsPay.Checkout.Bank", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ?? string.Empty}{card}";
                            HttpContext.Session.SetString("Bank", bank);
                        }
                        if (!string.IsNullOrEmpty(rates))
                            HttpContext.Session.SetString("Rates", rates);
                    }

                    //get payment info
                    var paymentInfo = await paymentMethod.GetPaymentInfoAsync(form);
                    //set previous order GUID (if exists)
                    _paymentService.GenerateOrderGuid(paymentInfo);

                    //session save
                    HttpContext.Session.Set("OrderPaymentInfo", paymentInfo);
                    var confirmOrderModel = await _checkoutModelFactory.PrepareConfirmOrderModelAsync(cart);
                    return Json(new
                    {
                        update_section = new UpdateSectionJsonModel
                        {
                            name = "confirm-order",
                            html = await RenderPartialViewToStringAsync("OpcConfirmOrder", confirmOrderModel)
                        },
                        goto_section = "confirm_order"
                    });
                }

                //If we got this far, something failed, redisplay form
                var paymenInfoModel = await _checkoutModelFactory.PreparePaymentInfoModelAsync(paymentMethod);
                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "payment-info",
                        html = await RenderPartialViewToStringAsync("OpcPaymentInfo", paymenInfoModel)
                    }
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }

        public override async Task<IActionResult> OpcConfirmOrder()
        {
            try
            {
                //validation
                if (_orderSettings.CheckoutDisabled)
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.Disabled"));

                var cart = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

                if (!cart.Any())
                    throw new Exception("Your cart is empty");

                if (!_orderSettings.OnePageCheckoutEnabled)
                    throw new Exception("One page checkout is disabled");

                if (await _customerService.IsGuestAsync(await _workContext.GetCurrentCustomerAsync()) && !_orderSettings.AnonymousCheckoutAllowed)
                    throw new Exception("Anonymous checkout is not allowed");

                //prevent 2 orders being placed within an X seconds time frame
                if (!await IsMinimumOrderPlacementIntervalValidAsync(await _workContext.GetCurrentCustomerAsync()))
                    throw new Exception(await _localizationService.GetResourceAsync("Checkout.MinOrderPlacementInterval"));

                //place order
                var processPaymentRequest = HttpContext.Session.Get<ProcessPaymentRequest>("OrderPaymentInfo");
                if (processPaymentRequest == null)
                {
                    //Check whether payment workflow is required
                    if (await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart))
                    {
                        throw new Exception("Payment information is not entered");
                    }

                    processPaymentRequest = new ProcessPaymentRequest();
                }
                _paymentService.GenerateOrderGuid(processPaymentRequest);
                processPaymentRequest.StoreId = (await _storeContext.GetCurrentStoreAsync()).Id;
                processPaymentRequest.CustomerId = (await _workContext.GetCurrentCustomerAsync()).Id;
                processPaymentRequest.PaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(await _workContext.GetCurrentCustomerAsync(),
                    NopCustomerDefaults.SelectedPaymentMethodAttribute, (await _storeContext.GetCurrentStoreAsync()).Id);
                HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", processPaymentRequest);
                var placeOrderResult = await _orderProcessingService.PlaceOrderAsync(processPaymentRequest);
                if (placeOrderResult.Success)
                {
                    HttpContext.Session.Set<ProcessPaymentRequest>("OrderPaymentInfo", null);

                    var card = HttpContext.Session.GetString("Card");
                    if (!string.IsNullOrEmpty(card))
                    {
                        var orderNoteText = $"{(await _localizationService.GetLocaleStringResourceByNameAsync("Plugins.Payments.WsPay.Checkout.Card", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ?? string.Empty}{card}";

                        var orderNote = new OrderNote
                        {
                            CreatedOnUtc = DateTime.Now,
                            DisplayToCustomer = false,
                            DownloadId = 0,
                            Note = orderNoteText,
                            OrderId = placeOrderResult.PlacedOrder.Id,
                        };
                        await _orderService.InsertOrderNoteAsync(orderNote);

                        placeOrderResult.PlacedOrder.CardName = card;
                    }
                    var bank = HttpContext.Session.GetString("Bank");
                    if (!string.IsNullOrEmpty(bank))
                    {
                        var orderNoteText = $"{(await _localizationService.GetLocaleStringResourceByNameAsync("Plugins.Payments.WsPay.Checkout.Bank", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ?? string.Empty}{bank}";

                        var orderNote = new OrderNote
                        {
                            CreatedOnUtc = DateTime.Now,
                            DisplayToCustomer = false,
                            DownloadId = 0,
                            Note = orderNoteText,
                            OrderId = placeOrderResult.PlacedOrder.Id,
                        };
                        await _orderService.InsertOrderNoteAsync(orderNote);
                    }
                    var parsed = int.TryParse(HttpContext.Session.GetString("Rates"), out var ratesValue);
                    if (parsed && ratesValue != 0)
                    {
                        var ratePayment = new RatePayment
                        {
                            OrderId = placeOrderResult.PlacedOrder.Id,
                            CardProvider = bank,
                            RatesNumber = ratesValue,
                        };
                        await _ratePaymentService.InsertRatePaymentAsync(ratePayment);

                        var rates = $"{ratesValue.ToString().PadLeft(2, '0')}00";

                        var orderNoteText = $"{(await _localizationService.GetLocaleStringResourceByNameAsync("Plugins.Payments.WsPay.Checkout.Rates", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ?? string.Empty}{rates}";

                        var orderNote = new OrderNote
                        {
                            CreatedOnUtc = DateTime.Now,
                            DisplayToCustomer = false,
                            DownloadId = 0,
                            Note = orderNoteText,
                            OrderId = placeOrderResult.PlacedOrder.Id,
                        };
                        await _orderService.InsertOrderNoteAsync(orderNote);

                        var ratePaymentText = $"WsPay {(await _localizationService.GetLocaleStringResourceByNameAsync("Plugins.Payments.WsPay.Checkout.UseRatePayment", (await _workContext.GetWorkingLanguageAsync()).Id))?.ResourceValue ?? "Rate"}";
                        var rateOrderNote = new OrderNote
                        {
                            CreatedOnUtc = DateTime.Now,
                            DisplayToCustomer = false,
                            DownloadId = 0,
                            Note = ratePaymentText,
                            OrderId = placeOrderResult.PlacedOrder.Id,
                        };
                        await _orderService.InsertOrderNoteAsync(rateOrderNote);
                    }

                    await _orderService.UpdateOrderAsync(placeOrderResult.PlacedOrder);

                    var postProcessPaymentRequest = new PostProcessPaymentRequest
                    {
                        Order = placeOrderResult.PlacedOrder
                    };

                    var paymentMethod = await _paymentPluginManager
                        .LoadPluginBySystemNameAsync(placeOrderResult.PlacedOrder.PaymentMethodSystemName, await _workContext.GetCurrentCustomerAsync(), (await _storeContext.GetCurrentStoreAsync()).Id);
                    if (paymentMethod == null)
                        //payment method could be null if order total is 0
                        //success
                        return Json(new { success = 1 });

                    if (paymentMethod.PaymentMethodType == PaymentMethodType.Redirection)
                    {
                        //Redirection will not work because it's AJAX request.
                        //That's why we don't process it here (we redirect a user to another page where he'll be redirected)

                        //redirect
                        return Json(new
                        {
                            redirect = $"{_webHelper.GetStoreLocation()}checkout/OpcCompleteRedirectionPayment"
                        });
                    }

                    await _paymentService.PostProcessPaymentAsync(postProcessPaymentRequest);
                    //success
                    return Json(new { success = 1 });
                }

                //error
                var confirmOrderModel = new CheckoutConfirmModel();
                foreach (var error in placeOrderResult.Errors)
                    confirmOrderModel.Warnings.Add(error);

                return Json(new
                {
                    update_section = new UpdateSectionJsonModel
                    {
                        name = "confirm-order",
                        html = await RenderPartialViewToStringAsync("OpcConfirmOrder", confirmOrderModel)
                    },
                    goto_section = "confirm_order"
                });
            }
            catch (Exception exc)
            {
                await _logger.WarningAsync(exc.Message, exc, await _workContext.GetCurrentCustomerAsync());
                return Json(new { error = 1, message = exc.Message });
            }
        }
    }
}
