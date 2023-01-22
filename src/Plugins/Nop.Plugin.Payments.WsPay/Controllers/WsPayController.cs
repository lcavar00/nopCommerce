using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.WsPay.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Topics;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Payments.WsPay.Controllers
{
    public class WsPayController : BasePaymentController
    {
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentPluginManager _paymentPluginManager;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly ITopicService _topicService;
        private readonly IUrlRecordService _urlService;
        private readonly IWebHelper _webHelper;
        private readonly ShoppingCartSettings _shoppingCartSettings;

        /// <summary>
        //Enumi MB
        /// </summary>
        public enum ValidateResponseStatusEnum
        {
            Process_Response = 0,
            Process_Success = 100,

            Process_Error = -100,
            Process_Cancel = -200,
            Process_Timeout = -300,
            
            NotValid_Wrong_Signature = -1,
            NotValid_Missing_Signature = -10,
            NotValid_Missing_ShoppingCartID = -11,
            NotValid_Empty_ShoppingCartID = -12,
            NotValid_Missing_ApprovalCode = -13,
            NotValid_Empty_ApprovalCode = -14,
            NotValid_Missing_Success = -15,
            NotValid_CompletedEqualZero = -16,
            NotValid_SuccessNotEqualOne = -18,
            ErrorMessage_Odbijeno = -20
        }

        public ValidateResponseStatusEnum ValidateResponseStatus;

        public NameValueCollection PgResponseFields = new();

        #region Ctor

        public WsPayController(ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentPluginManager paymentPluginManager,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            ITopicService topicService,
            IUrlRecordService urlService,
            IWebHelper webHelper,
            ShoppingCartSettings shoppingCartSettings)
        {
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentPluginManager = paymentPluginManager;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _topicService = topicService;
            _urlService = urlService;
            _webHelper = webHelper;
            _shoppingCartSettings = shoppingCartSettings;
        }

        #endregion

        #region Configure

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var wsPayPaymentSettings = await _settingService.LoadSettingAsync<WsPayPaymentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                UseSandbox = wsPayPaymentSettings.UseSandbox,
                PaymentUrl = wsPayPaymentSettings.PaymentUrl,
                ShopId = wsPayPaymentSettings.ShopId,
                ShopIdTest = wsPayPaymentSettings.ShopIdTest,
                SecretKey = wsPayPaymentSettings.SecretKey,
                SecretKeyTest = wsPayPaymentSettings.SecretKeyTest,
                PaymentUrlTest = wsPayPaymentSettings.PaymentUrlTest,
                PassProductNamesAndTotals = wsPayPaymentSettings.PassProductNamesAndTotals,
                ReturnFromWsPayWithoutPaymentRedirectsToOrderDetailsPage = wsPayPaymentSettings.ReturnFromWsPayWithoutPaymentRedirectsToOrderDetailsPage,
                ActiveStoreScopeConfiguration = storeScope,
                CallbackUrl = $"{_webHelper.GetStoreLocation()}Plugins/WsPay/CallbackUrl",
            };

            if (storeScope > 0)
            {
                model.UseSandbox_OverrideForStore = await _settingService.SettingExistsAsync(wsPayPaymentSettings, x => x.UseSandbox, storeScope);
                model.SecretKey_OverrideForStore = await _settingService.SettingExistsAsync(wsPayPaymentSettings, x => x.SecretKey, storeScope);
                model.SecretKeyTest_OverrideForStore = await _settingService.SettingExistsAsync(wsPayPaymentSettings, x => x.SecretKeyTest, storeScope);
                model.PaymentUrl_OverrideForStore = await _settingService.SettingExistsAsync(wsPayPaymentSettings, x => x.PaymentUrl, storeScope);
                model.PaymentUrlTest_OverrideForStore = await _settingService.SettingExistsAsync(wsPayPaymentSettings, x => x.PaymentUrlTest, storeScope);
                model.ShopId_OverrideForStore = await _settingService.SettingExistsAsync(wsPayPaymentSettings, x => x.ShopId, storeScope);
                model.ShopIdTest_OverrideForStore = await _settingService.SettingExistsAsync(wsPayPaymentSettings, x => x.ShopIdTest, storeScope);

                model.PassProductNamesAndTotals_OverrideForStore = await _settingService.SettingExistsAsync(wsPayPaymentSettings, x => x.PassProductNamesAndTotals, storeScope);
                model.ReturnFromWsPayWithoutPaymentRedirectsToOrderDetailsPage_OverrideForStore = await _settingService.SettingExistsAsync(wsPayPaymentSettings, x => x.ReturnFromWsPayWithoutPaymentRedirectsToOrderDetailsPage, storeScope);
            }

            return View("~/Plugins/Payments.WsPay/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePaymentMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            //load settings for a chosen store scope
            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var wsPayPaymentSettings = await _settingService.LoadSettingAsync<WsPayPaymentSettings>(storeScope);

            //save settings
            wsPayPaymentSettings.UseSandbox = model.UseSandbox;

            wsPayPaymentSettings.SecretKey = model.SecretKey;
            wsPayPaymentSettings.SecretKeyTest = model.SecretKeyTest;
            wsPayPaymentSettings.PaymentUrl = model.PaymentUrl;
            wsPayPaymentSettings.PaymentUrlTest = model.PaymentUrlTest;
            wsPayPaymentSettings.ShopId = model.ShopId;
            wsPayPaymentSettings.ShopIdTest = model.ShopIdTest;

            wsPayPaymentSettings.PassProductNamesAndTotals = model.PassProductNamesAndTotals;
            wsPayPaymentSettings.ReturnFromWsPayWithoutPaymentRedirectsToOrderDetailsPage = model.ReturnFromWsPayWithoutPaymentRedirectsToOrderDetailsPage;

            /* We do not clear cache after each setting update.
             * This behavior can increase performance because cached settings will not be cleared 
             * and loaded from database after each update */
            await _settingService.SaveSettingOverridablePerStoreAsync(wsPayPaymentSettings, x => x.UseSandbox, model.UseSandbox_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(wsPayPaymentSettings, x => x.SecretKey, model.SecretKey_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(wsPayPaymentSettings, x => x.SecretKeyTest, model.SecretKeyTest_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(wsPayPaymentSettings, x => x.PaymentUrl, model.PaymentUrl_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(wsPayPaymentSettings, x => x.PaymentUrlTest, model.PaymentUrlTest_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(wsPayPaymentSettings, x => x.ShopId, model.ShopId_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(wsPayPaymentSettings, x => x.ShopIdTest, model.ShopIdTest_OverrideForStore, storeScope, false);

            await _settingService.SaveSettingOverridablePerStoreAsync(wsPayPaymentSettings, x => x.PassProductNamesAndTotals, model.PassProductNamesAndTotals_OverrideForStore, storeScope, false);
            await _settingService.SaveSettingOverridablePerStoreAsync(wsPayPaymentSettings, x => x.ReturnFromWsPayWithoutPaymentRedirectsToOrderDetailsPage, model.ReturnFromWsPayWithoutPaymentRedirectsToOrderDetailsPage_OverrideForStore, storeScope, false);

            //now clear settings cache
            await _settingService.ClearCacheAsync();

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        #endregion

        //action displaying notification (warning) to a store owner about inaccurate WsPay rounding
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public async Task<IActionResult> RoundingWarning(bool passProductNamesAndTotals)
        {
            //prices and total aren't rounded, so display warning
            if (passProductNamesAndTotals && !_shoppingCartSettings.RoundPricesDuringCalculation)
                return Json(new { Result = await _localizationService.GetResourceAsync("Plugins.Payments.WsPay.RoundingWarning") });

            return Json(new { Result = string.Empty });
        }

        #region Handlers

        public async Task CallbackUrl([FromBody] CallbackData callbackData)
        {
            try
            {
                await _logger.InsertLogAsync(LogLevel.Information, "Callback hit", $"CallbackData JSON: {JsonConvert.SerializeObject(callbackData)}");

                if (ModelState.IsValid)
                {
                    var validate = await ValidateCallbackResponseAsync(callbackData);

                    if (validate == ValidateResponseStatusEnum.Process_Success)
                    {
                        var order = await _orderService.GetOrderByCustomOrderNumberAsync(callbackData.ShoppingCartID);
                        var newPaymentStatus = await GetCallbackPaymentStatusAsync(callbackData);
                        //order note
                        await _orderService.InsertOrderNoteAsync(new OrderNote
                        {
                            OrderId = order.Id,
                            Note = string.Format("WsPay callback payment status: {0}, ", newPaymentStatus.ToString()),
                            DisplayToCustomer = false,
                            CreatedOnUtc = DateTime.UtcNow
                        });

                        order.CardType = WsPayHelper.GetCardTypeName(callbackData.CreditCardName);

                        order.PaymentStatus = newPaymentStatus;
                        await _orderService.UpdateOrderAsync(order);
                    }
                    else
                    {
                        await _logger.InsertLogAsync(LogLevel.Error, $"CallbackData validation: {validate}", $"Callback JSON:{Environment.NewLine}{JsonConvert.SerializeObject(callbackData)}");
                    }
                }
                else
                {
                    await _logger.InsertLogAsync(LogLevel.Error, $"Callback data model is invalid", $"Callback JSON:{Environment.NewLine}{JsonConvert.SerializeObject(callbackData)}");
                }
            }
            catch(Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
            }
        }

        public async Task<IActionResult> SuccessHandler(IFormCollection form)
        {
            var query = HttpContext.Request.Query;

            var shoppingCartID = query["ShoppingCartID"].ToString() ?? form["ShoppingCartID"].ToString();
            var success = query["Success"].ToString() ?? form["Success"].ToString();
            var cardTypeCodeID = query["PaymentType"].ToString() ?? form["PaymentType"].ToString();

            if (await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.WsPay") is not WsPayPaymentProcessor processor ||
                !_paymentPluginManager.IsPluginActive(processor) || !processor.PluginDescriptor.Installed)
                throw new NopException("WsPay plugin cannot be loaded");


            var validate = await ValidateSuccessResponseAsync();

            if (success == "1")
            {
                if (validate == ValidateResponseStatusEnum.Process_Success)
                {
                    var order = await _orderService.GetOrderByCustomOrderNumberAsync(shoppingCartID);

                    var newPaymentStatus = WsPayHelper.GetPaymentStatus("success", "");

                    //order note
                    await _orderService.InsertOrderNoteAsync(new OrderNote
                    {
                        OrderId = order.Id,
                        Note = string.Format("New payment status: {0}, ", newPaymentStatus),
                        DisplayToCustomer = false,
                        CreatedOnUtc = DateTime.UtcNow
                    });

                    //mark order as paid
                    if (newPaymentStatus == PaymentStatus.Paid)
                    {
                        //25.6.2020. L.Ć. - card name is now saved in Order.CardName instead of Order.CardType
                        order.CardName = WsPayHelper.GetCardTypeName(cardTypeCodeID);
                        await _orderProcessingService.MarkOrderAsPaidAsync(order);
                    }
                    else if (newPaymentStatus == PaymentStatus.Authorized)
                    {
                        //25.6.2020. L.Ć. - card name is now saved in Order.CardName instead of Order.CardType
                        order.CardName = WsPayHelper.GetCardTypeName(cardTypeCodeID);
                        await _orderProcessingService.MarkAsAuthorizedAsync(order);
                    }
                    await _orderService.UpdateOrderAsync(order);

                    return RedirectToRoute("CheckoutCompleted", new { orderId = order.Id });
                }
                else if(validate == ValidateResponseStatusEnum.Process_Cancel)
                {
                    return RedirectToAction("CancelHandler");
                }
                else
                {
                    return RedirectToAction("ErrorHandler");
                }
            }
            else
            {
                return RedirectToAction("ErrorHandler");
            }
        }

        public async Task<IActionResult> CancelHandler(IFormCollection form)
        {
            try
            {
                var query = HttpContext.Request.Query;

                var shoppingCartID = query["ShoppingCartID"].ToString() ?? form["ShoppingCartID"].ToString();
                var approvalCode = query["ApprovalCode"].ToString() ?? form["ApprovalCode"].ToString();
                var success = query["Success"].ToString() ?? form["Success"].ToString();
                var responseCode = query["ResponseCode"].ToString() ?? form["ResponseCode"].ToString();
                var cardTypeCodeID = query["PaymentType"].ToString() ?? form["PaymentType"].ToString();

                if (await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.WsPay") is not WsPayPaymentProcessor processor ||
                    !_paymentPluginManager.IsPluginActive(processor) || !processor.PluginDescriptor.Installed)
                    throw new NopException("WsPay module cannot be loaded");

                var order = await _orderService.GetOrderByCustomOrderNumberAsync(shoppingCartID);
                if (order == null)
                {
                    throw new Exception($"Order cannot be found (ShoppingCartID = {shoppingCartID})");
                }

                var validate = await ValidateSuccessResponseAsync();

                if (success == "0" && responseCode == "15")
                {
                    var orderNote = new OrderNote
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = false,
                        DownloadId = 0,
                        Note = $"WsPay - user cancelled payment (ResponseCode = {responseCode}, Success = {success})",
                        OrderId = order.Id,
                    };
                    await _orderService.InsertOrderNoteAsync(orderNote);

                    _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugins.Payments.WsPay.PaymentCancelledWarning"));
                }

                if (success == "0" && responseCode == "16")
                {
                    var orderNote = new OrderNote
                    {
                        CreatedOnUtc = DateTime.UtcNow,
                        DisplayToCustomer = false,
                        DownloadId = 0,
                        Note = $"WsPay - server timeout (ResponseCode = {responseCode}, Success = {success})",
                        OrderId = order.Id,
                    };
                    await _orderService.InsertOrderNoteAsync(orderNote);

                    _notificationService.WarningNotification(await _localizationService.GetResourceAsync("Plugins.Payments.WsPay.PaymentTimeoutWarning"));
                }

                var storeLocation = _webHelper.GetStoreLocation();

                return Redirect($"{storeLocation}OrderDetails/{order.Id}");
            }
            catch(Exception e)
            {
                await _logger.ErrorAsync(e.Message);
                return await ErrorHandler(form);
            }
        }

        public async Task<IActionResult> ErrorHandler(IFormCollection form)
        {
            var query = HttpContext.Request.Query;

            var shoppingCartID = query["ShoppingCartID"].ToString() ?? form["ShoppingCartID"].ToString();

            if (await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.WsPay") is not WsPayPaymentProcessor processor ||
                !_paymentPluginManager.IsPluginActive(processor) || !processor.PluginDescriptor.Installed)
                throw new NopException("WsPay plugin cannot be loaded");

            var validate = await ValidateSuccessResponseAsync();

            if (validate == ValidateResponseStatusEnum.Process_Success)
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"WsPay hit error handler, but validation returned process success", $"ShoppingCartID = {shoppingCartID}");
                return RedirectToAction("SuccessHandler");
            }
            else if (validate == ValidateResponseStatusEnum.Process_Cancel)
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"WsPay hit error handler, but validation returned process cancel", $"ShoppingCartID = {shoppingCartID}");
                return RedirectToAction("CancelHandler");
            }

            var errorTopicPage = await _topicService.GetTopicBySystemNameAsync("wspayerror");
            var languageId = (await _storeContext.GetCurrentStoreAsync()).DefaultLanguageId;

            var url = await _urlService.GetActiveSlugAsync(errorTopicPage.Id, "Topic", languageId);
            if(url == null || url == "")
            {
                url = await _urlService.GetActiveSlugAsync(errorTopicPage.Id, "Topic", 0);
            }

            var store = await _storeContext.GetCurrentStoreAsync();

            return Redirect(_webHelper.GetStoreLocation(store.SslEnabled) + url);
        }

        [HttpPost]
        public async Task TransactionHandler(TransactionModel model)
        {
            var processor = await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.WsPay") as WsPayPaymentProcessor;
            var order = await _orderService.GetOrderByCustomOrderNumberAsync(model.ShoppingCartId);
            var successSignature = processor.CreateSuccessSignature(model.ShoppingCartId, model.ApprovalCode);

            if(successSignature != model.Signature)
            {
                return;
            }

            if(model.Authorized == "1")
            {
                order.PaymentStatus = PaymentStatus.Authorized;
            }
            else if(model.Completed == "1")
            {
                order.PaymentStatus = PaymentStatus.Paid;
            }
            else if(model.Refunded == "1")
            {
                order.PaymentStatus = PaymentStatus.Refunded;
            }
            else if(model.Voided == "1")
            {
                order.PaymentStatus = PaymentStatus.Voided;
            }

            await _orderService.UpdateOrderAsync(order);
        }

        #endregion

        #region Utilities

        private async Task<PaymentStatus> GetCallbackPaymentStatusAsync(CallbackData callbackData)
        {
            var order = await _orderService.GetOrderByCustomOrderNumberAsync(callbackData.ShoppingCartID);

            if(callbackData.Completed == "1" && callbackData.Authorized == "1")
            {
                return PaymentStatus.Authorized;
            }

            if(callbackData.Authorized == "1" && callbackData.Completed == "1")
            {
                return PaymentStatus.Paid;
            }

            if(callbackData.Authorized == "1" && callbackData.Voided == "1")
            {
                return PaymentStatus.Voided;
            }

            if(callbackData.Authorized == "1" && callbackData.Refunded == "1")
            {
                return PaymentStatus.Refunded;
            }

            return order.PaymentStatus;
        }


        private async Task<ValidateResponseStatusEnum> ValidateSuccessResponseAsync()
        {
            var processor = await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.WsPay") as WsPayPaymentProcessor;

            ReadQueryString();

            // imamo li query string Signature?
            if (PgResponseFields["Signature"] == null)
                return ValidateResponseStatusEnum.NotValid_Missing_Signature;
            var qSignature = PgResponseFields["Signature"];

            // imamo li query string ShoppingCartID
            if (PgResponseFields["ShoppingCartID"] == null)
                return ValidateResponseStatusEnum.NotValid_Missing_ShoppingCartID;
            var qShoppingCartID = PgResponseFields["ShoppingCartID"];
            if (string.IsNullOrEmpty(qShoppingCartID))
                return ValidateResponseStatusEnum.NotValid_Empty_ShoppingCartID;

            // imamo li query string Success
            var qSuccess = PgResponseFields["Success"];
            if (string.IsNullOrEmpty(qSuccess))
                return ValidateResponseStatusEnum.NotValid_Missing_Success;

            var qResponseCode = PgResponseFields["ResponseCode"];
            if(qSuccess == "0" && qResponseCode == "15")
            {
                return ValidateResponseStatusEnum.Process_Cancel;
            }
            if (qSuccess == "0" && qResponseCode == "15")
            {
                return ValidateResponseStatusEnum.Process_Timeout;
            }

            // imamo li query string ApprovalCode
            if (PgResponseFields["ApprovalCode"] == null)
                return ValidateResponseStatusEnum.NotValid_Missing_ApprovalCode;
            var qApprovalCode = PgResponseFields["ApprovalCode"];
            if (string.IsNullOrEmpty(qApprovalCode))
                return ValidateResponseStatusEnum.NotValid_Empty_ApprovalCode;

            if (qSuccess != "1")
                return ValidateResponseStatusEnum.NotValid_SuccessNotEqualOne;

            // da li je ispravan Signature
            var successSignature = processor.CreateSuccessSignature(qShoppingCartID, qApprovalCode);
            if (successSignature != qSignature)
                return ValidateResponseStatusEnum.NotValid_Wrong_Signature;

            // da li je transakcija odbijena
            if (PgResponseFields["ErrorMessage"] == "ODBIJENO" || PgResponseFields["ErrorMessage"] == "DECLINED")
                return ValidateResponseStatusEnum.ErrorMessage_Odbijeno;

            //OK, sve smo provjerili i response smatramo validnim
            return ValidateResponseStatusEnum.Process_Success;
        }

        private async Task<ValidateResponseStatusEnum> ValidateCallbackResponseAsync(CallbackData callbackData)
        {
            var processor = await _paymentPluginManager.LoadPluginBySystemNameAsync("Payments.WsPay") as WsPayPaymentProcessor;
            // imamo li query string Signature?
            if (callbackData.Signature == null)
                return ValidateResponseStatusEnum.NotValid_Missing_Signature;
            var qSignature = callbackData.Signature;

            // imamo li query string ShoppingCartID
            if (callbackData.ShoppingCartID == null)
                return ValidateResponseStatusEnum.NotValid_Missing_ShoppingCartID;
            string qWsPayOrderId = callbackData.WsPayOrderId;
            if (string.IsNullOrEmpty(qWsPayOrderId))
                return ValidateResponseStatusEnum.NotValid_Empty_ShoppingCartID;

            // imamo li query string ApprovalCode
            if (callbackData.ApprovalCode == null)
                return ValidateResponseStatusEnum.NotValid_Missing_ApprovalCode;
            var qApprovalCode = callbackData.ApprovalCode;
            if (string.IsNullOrEmpty(qApprovalCode))
                return ValidateResponseStatusEnum.NotValid_Empty_ApprovalCode;

            // imamo li query string Success (21.7.2021. success is depricated, use Completed)
            var qCompleted = callbackData.Completed;
            if (string.IsNullOrEmpty(qCompleted))
                return ValidateResponseStatusEnum.NotValid_Missing_Success;
            if (qCompleted == "0")
                return ValidateResponseStatusEnum.NotValid_CompletedEqualZero;
            if (qCompleted != "1")
                return ValidateResponseStatusEnum.NotValid_SuccessNotEqualOne;

            // da li je ispravan Signature
            var successSignature = processor.CreateCallbackSignature(qWsPayOrderId, qApprovalCode);
            if (successSignature != qSignature)
                return ValidateResponseStatusEnum.NotValid_Wrong_Signature;

            //OK, sve smo provjerili i response smatramo validnim
            return ValidateResponseStatusEnum.Process_Success;
        }

        private void ReadQueryString()
        {
            // transfer values from request QueryString collection to our prublic property namevalue collection
            foreach (var key in HttpContext.Request.Query.Keys)
            {
                PgResponseFields.Add(key, HttpContext.Request.Query[key]);
            }
        }

        #endregion
    }
}
