using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Plugin.Shipping.Core.Areas.Admin.Factories;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.ExportImport;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class CustomOrderController : OrderController
    {
        #region Fields

        private readonly IEncryptionService _encryptionService;
        private readonly IGiftCardService _giftCardService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IOrderModelFactoryExtension _orderModelFactory;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IPaymentService _paymentService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly OrderSettings _orderSettings;

        #endregion

        #region Ctor

        public CustomOrderController(IAddressAttributeParser addressAttributeParser,
            IAddressService addressService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            IDateTimeHelper dateTimeHelper,
            IDownloadService downloadService,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IExportManager exportManager,
            IGiftCardService giftCardService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IOrderModelFactoryExtension orderModelFactory,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            IPdfService pdfService,
            IPermissionService permissionService,
            IPriceCalculationService priceCalculationService,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductAttributeService productAttributeService,
            IProductService productService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            OrderSettings orderSettings) : base(addressAttributeParser, addressService, customerActivityService, customerService, dateTimeHelper, downloadService, encryptionService, eventPublisher, exportManager, giftCardService, localizationService, notificationService, orderModelFactory, orderProcessingService, orderService, paymentService, pdfService, permissionService, priceCalculationService, productAttributeFormatter, productAttributeParser, productAttributeService, productService, shipmentService, shippingService, shoppingCartService, workContext, workflowMessageService, orderSettings)
        {
            _encryptionService = encryptionService;
            _giftCardService = giftCardService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _orderModelFactory = orderModelFactory;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _productService = productService;
            _paymentService = paymentService;
            _permissionService = permissionService;
            _workContext = workContext;
            _orderSettings = orderSettings;
        }

        #endregion

        public override async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null || order.Deleted)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToOrderAsync(order))
                return RedirectToAction("List");

            //prepare model
            var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

            return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("cancelorder")]
        public override async Task<IActionResult> CancelOrder(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                await _orderProcessingService.CancelOrderAsync(order, true);
                await LogEditOrderAsync(order.Id);

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                await _notificationService.ErrorNotificationAsync(exc);
                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("captureorder")]
        public override async Task<IActionResult> CaptureOrder(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                var errors = await _orderProcessingService.CaptureAsync(order);
                await LogEditOrderAsync(order.Id);

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                foreach (var error in errors)
                    _notificationService.ErrorNotification(error);

                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                await _notificationService.ErrorNotificationAsync(exc);
                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("markorderaspaid")]
        public override async Task<IActionResult> MarkOrderAsPaid(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                await _orderProcessingService.MarkOrderAsPaidAsync(order);
                await LogEditOrderAsync(order.Id);

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                await _notificationService.ErrorNotificationAsync(exc);
                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorder")]
        public override async Task<IActionResult> RefundOrder(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                var errors = await _orderProcessingService.RefundAsync(order);
                await LogEditOrderAsync(order.Id);

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                foreach (var error in errors)
                    _notificationService.ErrorNotification(error);

                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                await _notificationService.ErrorNotificationAsync(exc);
                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("refundorderoffline")]
        public override async Task<IActionResult> RefundOrderOffline(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });


            try
            {
                await _orderProcessingService.RefundOfflineAsync(order);
                await LogEditOrderAsync(order.Id);

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                await _notificationService.ErrorNotificationAsync(exc);
                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorder")]
        public override async Task<IActionResult> VoidOrder(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                var errors = await _orderProcessingService.VoidAsync(order);
                await LogEditOrderAsync(order.Id);

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                foreach (var error in errors)
                    _notificationService.ErrorNotification(error);

                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                await _notificationService.ErrorNotificationAsync(exc);
                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("voidorderoffline")]
        public override async Task<IActionResult> VoidOrderOffline(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                await _orderProcessingService.VoidOfflineAsync(order);
                await LogEditOrderAsync(order.Id);

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
            catch (Exception exc)
            {
                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                await _notificationService.ErrorNotificationAsync(exc);
                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderStatus")]
        public override async Task<IActionResult> ChangeOrderStatus(int id, OrderModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });

            try
            {
                order.OrderStatusId = model.OrderStatusId;
                await _orderService.UpdateOrderAsync(order);

                //add a note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = $"Order status has been edited. New status: {await _localizationService.GetLocalizedEnumAsync(order.OrderStatus)}",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });
                await _orderService.UpdateOrderAsync(order);
                await LogEditOrderAsync(order.Id);

                //prepare model
                model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
            catch (Exception exc)
            {
                //prepare model
                model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                await _notificationService.ErrorNotificationAsync(exc);
                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveCC")]
        public override async Task<IActionResult> EditCreditCardInfo(int id, OrderModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });

            if (order.AllowStoringCreditCardNumber)
            {
                var cardType = model.CardType;
                var cardName = model.CardName;
                var cardNumber = model.CardNumber;
                var cardCvv2 = model.CardCvv2;
                var cardExpirationMonth = model.CardExpirationMonth;
                var cardExpirationYear = model.CardExpirationYear;

                order.CardType = _encryptionService.EncryptText(cardType);
                order.CardName = _encryptionService.EncryptText(cardName);
                order.CardNumber = _encryptionService.EncryptText(cardNumber);
                order.MaskedCreditCardNumber = _encryptionService.EncryptText(_paymentService.GetMaskedCreditCardNumber(cardNumber));
                order.CardCvv2 = _encryptionService.EncryptText(cardCvv2);
                order.CardExpirationMonth = _encryptionService.EncryptText(cardExpirationMonth);
                order.CardExpirationYear = _encryptionService.EncryptText(cardExpirationYear);
                await _orderService.UpdateOrderAsync(order);
            }

            //add a note
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = "Credit card info has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            await _orderService.UpdateOrderAsync(order);
            await LogEditOrderAsync(order.Id);

            //prepare model
            model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

            return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired("btnSaveOrderTotals")]
        public override async Task<IActionResult> EditOrderTotals(int id, OrderModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });

            order.OrderSubtotalInclTax = model.OrderSubtotalInclTaxValue;
            order.OrderSubtotalExclTax = model.OrderSubtotalExclTaxValue;
            order.OrderSubTotalDiscountInclTax = model.OrderSubTotalDiscountInclTaxValue;
            order.OrderSubTotalDiscountExclTax = model.OrderSubTotalDiscountExclTaxValue;
            order.OrderShippingInclTax = model.OrderShippingInclTaxValue;
            order.OrderShippingExclTax = model.OrderShippingExclTaxValue;
            order.PaymentMethodAdditionalFeeInclTax = model.PaymentMethodAdditionalFeeInclTaxValue;
            order.PaymentMethodAdditionalFeeExclTax = model.PaymentMethodAdditionalFeeExclTaxValue;
            order.TaxRates = model.TaxRatesValue;
            order.OrderTax = model.TaxValue;
            order.OrderDiscount = model.OrderTotalDiscountValue;
            order.OrderTotal = model.OrderTotalValue;
            await _orderService.UpdateOrderAsync(order);

            //add a note
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = "Order totals have been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            await _orderService.UpdateOrderAsync(order);
            await LogEditOrderAsync(order.Id);

            //prepare model
            model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

            return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnSaveOrderItem")]
        public override async Task<IActionResult> EditOrderItem(int id, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });

            //get order item identifier
            var orderItemId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnSaveOrderItem", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnSaveOrderItem".Length));

            var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
                ?? throw new ArgumentException("No order item found with the specified id");

            if (!decimal.TryParse(form["pvUnitPriceInclTax" + orderItemId], out var unitPriceInclTax))
                unitPriceInclTax = orderItem.UnitPriceInclTax;
            if (!decimal.TryParse(form["pvUnitPriceExclTax" + orderItemId], out var unitPriceExclTax))
                unitPriceExclTax = orderItem.UnitPriceExclTax;
            if (!int.TryParse(form["pvQuantity" + orderItemId], out var quantity))
                quantity = orderItem.Quantity;
            if (!decimal.TryParse(form["pvDiscountInclTax" + orderItemId], out var discountInclTax))
                discountInclTax = orderItem.DiscountAmountInclTax;
            if (!decimal.TryParse(form["pvDiscountExclTax" + orderItemId], out var discountExclTax))
                discountExclTax = orderItem.DiscountAmountExclTax;
            if (!decimal.TryParse(form["pvPriceInclTax" + orderItemId], out var priceInclTax))
                priceInclTax = orderItem.PriceInclTax;
            if (!decimal.TryParse(form["pvPriceExclTax" + orderItemId], out var priceExclTax))
                priceExclTax = orderItem.PriceExclTax;

            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

            if (quantity > 0)
            {
                var qtyDifference = orderItem.Quantity - quantity;

                if (!_orderSettings.AutoUpdateOrderTotalsOnEditingOrder)
                {
                    orderItem.UnitPriceInclTax = unitPriceInclTax;
                    orderItem.UnitPriceExclTax = unitPriceExclTax;
                    orderItem.Quantity = quantity;
                    orderItem.DiscountAmountInclTax = discountInclTax;
                    orderItem.DiscountAmountExclTax = discountExclTax;
                    orderItem.PriceInclTax = priceInclTax;
                    orderItem.PriceExclTax = priceExclTax;
                    await _orderService.UpdateOrderItemAsync(orderItem);
                }

                //adjust inventory
                await _productService.AdjustInventoryAsync(product, qtyDifference, orderItem.AttributesXml,
                    string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.EditOrder"), order.Id));
            }
            else
            {
                //adjust inventory
                await _productService.AdjustInventoryAsync(product, orderItem.Quantity, orderItem.AttributesXml,
                    string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.DeleteOrderItem"), order.Id));

                //delete item
                await _orderService.DeleteOrderItemAsync(orderItem);
            }

            //update order totals
            var updateOrderParameters = new UpdateOrderParameters(order, orderItem)
            {
                PriceInclTax = unitPriceInclTax,
                PriceExclTax = unitPriceExclTax,
                DiscountAmountInclTax = discountInclTax,
                DiscountAmountExclTax = discountExclTax,
                SubTotalInclTax = priceInclTax,
                SubTotalExclTax = priceExclTax,
                Quantity = quantity
            };
            await _orderProcessingService.UpdateOrderTotalsAsync(updateOrderParameters);

            //add a note
            await _orderService.InsertOrderNoteAsync(new OrderNote
            {
                OrderId = order.Id,
                Note = "Order item has been edited",
                DisplayToCustomer = false,
                CreatedOnUtc = DateTime.UtcNow
            });
            await LogEditOrderAsync(order.Id);

            //prepare model
            var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

            foreach (var warning in updateOrderParameters.Warnings)
                _notificationService.WarningNotification(warning);

            //selected panel
            SaveSelectedCardName("order-products", persistForTheNextRequest: false);

            return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnDeleteOrderItem")]
        public override async Task<IActionResult> DeleteOrderItem(int id, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //a vendor does not have access to this functionality
            if (await _workContext.GetCurrentVendorAsync() != null)
                return RedirectToAction("Edit", "Order", new { id });

            //get order item identifier
            var orderItemId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnDeleteOrderItem", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnDeleteOrderItem".Length));

            var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
                ?? throw new ArgumentException("No order item found with the specified id");

            if ((await _giftCardService.GetGiftCardsByPurchasedWithOrderItemIdAsync(orderItem.Id)).Any())
            {
                //we cannot delete an order item with associated gift cards
                //a store owner should delete them first

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Orders.OrderItem.DeleteAssociatedGiftCardRecordError"));

                //selected panel
                SaveSelectedCardName("order-products", persistForTheNextRequest: false);

                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
            else
            {
                var product = await _productService.GetProductByIdAsync(orderItem.ProductId);

                //adjust inventory
                await _productService.AdjustInventoryAsync(product, orderItem.Quantity, orderItem.AttributesXml,
                    string.Format(await _localizationService.GetResourceAsync("Admin.StockQuantityHistory.Messages.DeleteOrderItem"), order.Id));

                //delete item
                await _orderService.DeleteOrderItemAsync(orderItem);

                //update order totals
                var updateOrderParameters = new UpdateOrderParameters(order, orderItem);
                await _orderProcessingService.UpdateOrderTotalsAsync(updateOrderParameters);

                //add a note
                await _orderService.InsertOrderNoteAsync(new OrderNote
                {
                    OrderId = order.Id,
                    Note = "Order item has been deleted",
                    DisplayToCustomer = false,
                    CreatedOnUtc = DateTime.UtcNow
                });

                await LogEditOrderAsync(order.Id);

                //prepare model
                var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

                foreach (var warning in updateOrderParameters.Warnings)
                    _notificationService.WarningNotification(warning);

                //selected panel
                SaveSelectedCardName("order-products", persistForTheNextRequest: false);

                return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
            }
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnResetDownloadCount")]
        public override async Task<IActionResult> ResetDownloadCount(int id, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //get order item identifier
            var orderItemId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnResetDownloadCount", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnResetDownloadCount".Length));

            var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
                ?? throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToProductAsync(orderItem))
                return RedirectToAction("List");

            orderItem.DownloadCount = 0;
            await _orderService.UpdateOrderItemAsync(orderItem);
            await LogEditOrderAsync(order.Id);

            //prepare model
            var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

            //selected panel
            SaveSelectedCardName("order-products", persistForTheNextRequest: false);

            return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
        }

        [HttpPost, ActionName("Edit")]
        [FormValueRequired(FormValueRequirement.StartsWith, "btnPvActivateDownload")]
        public override async Task<IActionResult> ActivateDownloadItem(int id, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            //try to get an order with the specified id
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
                return RedirectToAction("List");

            //get order item identifier
            var orderItemId = 0;
            foreach (var formValue in form.Keys)
                if (formValue.StartsWith("btnPvActivateDownload", StringComparison.InvariantCultureIgnoreCase))
                    orderItemId = Convert.ToInt32(formValue.Substring("btnPvActivateDownload".Length));

            var orderItem = await _orderService.GetOrderItemByIdAsync(orderItemId)
                ?? throw new ArgumentException("No order item found with the specified id");

            //ensure a vendor has access only to his products 
            if (await _workContext.GetCurrentVendorAsync() != null && !await HasAccessToProductAsync(orderItem))
                return RedirectToAction("List");

            orderItem.IsDownloadActivated = !orderItem.IsDownloadActivated;
            await _orderService.UpdateOrderItemAsync(orderItem);

            await LogEditOrderAsync(order.Id);

            //prepare model
            var model = await _orderModelFactory.PrepareOrderModelExtensionAsync(null, order);

            //selected panel
            SaveSelectedCardName("order-products", persistForTheNextRequest: false);
            return View("~/Plugins/Shipping.Core/Areas/Admin/Views/CustomOrder/Edit.cshtml", model);
        }
    }
}
