using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Topics;
using Nop.Data;
using Nop.Plugin.Payments.WsPay.Controllers;
using Nop.Plugin.Payments.WsPay.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Topics;
using Nop.Web.Framework;

namespace Nop.Plugin.Payments.WsPay
{
    public class WsPayPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Constants

        /// <summary>
        /// nopCommerce partner code
        /// </summary>
        private const string BN_CODE = "nopCommerce_SP";

        #endregion

        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IPaymentService _paymentService;
        private readonly IRatePaymentService _ratePaymentService;
        private readonly IRepository<Language> _languageRepository;
        private readonly IRepository<UrlRecord> _urlRecordRepository;
        private readonly ISettingService _settingService;
        private readonly ITopicService _topicService;
        private readonly IWebHelper _webHelper;
        private readonly IWebHostEnvironment _hostingEnviroment;
        private readonly WsPayPaymentSettings _wspayPaymentSettings;

        private NameValueCollection _paymentGatewayFields = new NameValueCollection();

        #endregion

        #region Ctor

        public WsPayPaymentProcessor(IAddressService addressService,
            ICountryService countryService,
            IHttpContextAccessor httpContextAccessor,
            ILocalizationService localizationService,
            ILogger logger,
            IPaymentService paymentService,
            IRatePaymentService ratePaymentService,
            IRepository<Language> languageRepository,
            IRepository<UrlRecord> urlRecordRepository,
            ISettingService settingService,
            ITopicService topicService,
            IWebHelper webHelper,
            IWebHostEnvironment hostingEnviroment,
            WsPayPaymentSettings wsPayPaymentSettings)
        {
            _addressService = addressService;
            _countryService = countryService;
            _httpContextAccessor = httpContextAccessor;
            _localizationService = localizationService;
            _logger = logger;
            _paymentService = paymentService;
            _ratePaymentService = ratePaymentService;
            _urlRecordRepository = urlRecordRepository;
            _languageRepository = languageRepository;
            _settingService = settingService;
            _topicService = topicService;
            _webHelper = webHelper;
            _hostingEnviroment = hostingEnviroment;
            _wspayPaymentSettings = wsPayPaymentSettings;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType
        {
            get { return RecurringPaymentType.NotSupported; }
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType
        {
            get { return PaymentMethodType.Redirection; }
        }

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription => _localizationService.GetResourceAsync("Plugins.Payments.WsPay.PaymentMethodDescription").Result;

        #endregion



        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult
            {
                NewPaymentStatus = PaymentStatus.Pending
            };
            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        //2019-07-29 LĆ (write async)
        public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            await SetupPaymentGatewayFieldsAsync(postProcessPaymentRequest);

            var post = new RemotePost(_httpContextAccessor, _webHelper)
            {
                FormName = "Pay",
                Url = GetWsPayUrl(),
                Method = "POST",
            };

            foreach (var name in _paymentGatewayFields.AllKeys)
            {
                post.Add(name, _paymentGatewayFields[name]);
            }

            post.Post();
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            // WsPay nema additional fee
            return 0;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return result;
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();
            result.AddError("Refund method not supported");
            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public async Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            result.AddError("Recurring payment not supported");
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public async Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException("order");

            //let's ensure that at least 5 seconds passed after order is placed
            //P.S. there's no any particular reason for that. we just do it
            if ((DateTime.UtcNow - order.CreatedOnUtc).TotalSeconds < 5)
                return false;

            return true;
        }

        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/WsPay/Configure";
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "WsPay";
            routeValues = new RouteValueDictionary { { "Namespaces", "Nop.Plugin.Payments.WsPay.Controllers" }, { "area", null } };
        }

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            return new List<string>();
        }

        public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return new ProcessPaymentRequest();
        }

        public string GetPublicViewComponentName()
        {
            return "WsPay";
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        /// <returns>A task that represents the asynchronous operation</returns>
        public async Task<string> GetPaymentMethodDescriptionAsync()
        {
            return await _localizationService.GetResourceAsync("Plugins.Payments.WsPay.PaymentMethodDescription");
        }

        /// <summary>
        /// Verifies the success signature that we get from WsPay.
        /// </summary>
        /// <param name="shoppingCartID">Cart ID as string</param>
        /// <param name="approvalCode">Approval code as string</param>
        /// <returns>Success signature as string</returns>
        public string CreateSuccessSignature(string shoppingCartID, string approvalCode)
        {
            var secretKey = GetSecretKey();
            var shopId = GetShopId();
            var success = "1";

            var sucessSignature = GetSHA512(string.Format("{1}{0}{2}{0}{3}{0}{4}{0}", secretKey, shopId, shoppingCartID, success, approvalCode));

            return sucessSignature;
        }

        public string CreateCallbackSignature(string wsPayOrderId, string approvalCode)
        {
            var secretKey = GetSecretKey();
            var shopId = GetShopId();
            var success = "1";

            var sucessSignature = GetMD5(string.Format("{1}{0}{2}{3}{0}{1}{3}{4}", secretKey, shopId, success, approvalCode, wsPayOrderId));

            return sucessSignature;
        }


        /// <summary>
        /// Get type of controller
        /// </summary>
        /// <returns>Type</returns>
        public Type GetControllerType()
        {
            return typeof(WsPayController);
        }


        public override async Task InstallAsync()
        {
            await CreateSettingsAsync();
            await CreateErrorTopicPageAsync();
            await AddLocalizationAsync();

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await DeleteSettingsAsync ();
            await DeleteErrorTopicPageAsync ();
            await DeleteLocalizationAsync();

            await base.UninstallAsync();
        }

        #endregion


        #region Utility

        public bool GetSuccessDetails(IFormCollection form, out Dictionary<string, string> values)
        {
            bool success = true;
            values = new Dictionary<string, string>();

            values.Add("OrderId", form["pgw_order_id"]);
            values.Add("Signature", form["pgw_signature"]);
            //values.Add("TransactionId", form.Get("pgw_transaction_id"));

            //Može se ubaciti i ove parametre ako treba
            //pgw_trace_ref
            //pgw_amount
            //pgw_installments
            //pgw_card_type_id
            //pgw_merchant_data

            //TODO: možda treba provjeriti signature

            return success;
        }

        public bool GetFalureCancelDetails(IFormCollection form, out Dictionary<string, string> values)
        {
            bool success = true;
            values = new Dictionary<string, string>();

            values.Add("ResultCode", form["pgw_result_code"]);
            values.Add("TraceReference", form["pgw_trace_ref"]);

            return success;
        }

        private string GetWsPayUrl()
        {
            return _wspayPaymentSettings.UseSandbox ? _wspayPaymentSettings.PaymentUrlTest :
              _wspayPaymentSettings.PaymentUrl;
        }


        private string GetSecretKey()
        {
            return _wspayPaymentSettings.UseSandbox ? _wspayPaymentSettings.SecretKeyTest :
              _wspayPaymentSettings.SecretKey;
        }

        private string GetShopId()
        {
            return _wspayPaymentSettings.UseSandbox ? _wspayPaymentSettings.ShopIdTest :
             _wspayPaymentSettings.ShopId;
        }

        private void AddPaymentGatewayField(string name, string value)
        {
            if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(value))
            {
                _paymentGatewayFields.Add(name, value);
            }
        }

        private string GetSHA512(string input)
        {
            var bytes = Encoding.UTF8.GetBytes(input);
            using var hash = SHA512.Create();
            var hashedInputBytes = hash.ComputeHash(bytes);

            // Convert to text
            // StringBuilder Capacity is 128, because 512 bits / 8 bits in byte * 2 symbols for byte 
            var hashedInputStringBuilder = new StringBuilder(128);
            foreach (var b in hashedInputBytes)
                hashedInputStringBuilder.Append(b.ToString("X2"));
            return hashedInputStringBuilder.ToString().ToLower();
        }

        private string GetMD5(string input)
        {
            // step 1, calculate MD5 hash from input
            var md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] byteHash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i <= byteHash.Length - 1; i++)
            {
                sb.Append(byteHash[i].ToString("X2"));
            }
            return sb.ToString().ToLower();
        }

        private async Task SetupPaymentGatewayFieldsAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var storeLocation = _webHelper.GetStoreLocation();

            string secretKey = GetSecretKey(); // "_q=UrisTT-R!duBsF54/3hys@cI";
            string shopId = GetShopId(); // "20000187";

            // LJ 09.11.2017 - WSPay specifičnosti:
            // za slanje forme na POST total order amount (polje TotalAmount) mora biti sa zarezom (npr 89,90),
            // dok pri kreiranju signatura mora biti total order amount (polje Total Amount) bez decimalnih separatora (točka ili zarez) npr . 8900

            string totalAmount = postProcessPaymentRequest.Order.OrderTotal.ToString("0.00");
            decimal totalAmountDecimal = decimal.Parse(totalAmount);
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ",";
            nfi.NumberGroupSeparator = ".";
            string totalAmountSaZarezom = totalAmountDecimal.ToString(nfi);
            string totalAmountBezZareza = totalAmount.Replace(",", "").Replace(".", "");

            string returnUrl = $"{storeLocation}Plugins/WsPay/SuccessHandler";
            string cancelReturnUrl = $"{storeLocation}Plugins/WsPay/CancelHandler";
            string errorUrl = $"{storeLocation}Plugins/WsPay/ErrorHandler";

            var input = string.Format("{1}{0}{2}{0}{3}{0}", secretKey, shopId, postProcessPaymentRequest.Order.CustomOrderNumber.ToString(), totalAmountBezZareza);

            string signature = GetSHA512(input);
            //GetMD5 is depricated in WSPay generate signature Version 2.0
            //string signature = GetMD5(input);

            AddPaymentGatewayField("TotalAmount", totalAmountSaZarezom);
            AddPaymentGatewayField("ShopID", shopId);
            AddPaymentGatewayField("ShoppingCartID", postProcessPaymentRequest.Order.CustomOrderNumber.ToString());
            AddPaymentGatewayField("Signature", signature);
            //Version parameter is necessary to use SHA512 (instead of MD5) when generating signature
            AddPaymentGatewayField("Version", "2.0");

            AddPaymentGatewayField("ReturnURL", returnUrl);
            AddPaymentGatewayField("ReturnErrorURL", errorUrl);
            AddPaymentGatewayField("CancelURL", cancelReturnUrl);

            AddPaymentGatewayField("Lang", "HR");

            var ratePayment = await _ratePaymentService.GetRatePaymentByOrderIdAsync(postProcessPaymentRequest.Order.Id);

            if (ratePayment != null)
            {
                var rates = $"{ratePayment.RatesNumber.ToString().PadLeft(2, '0')}00";

                AddPaymentGatewayField("PaymentPlan", rates);
            }
            else
            {
                AddPaymentGatewayField("PaymentPlan", "0000");
            }

            var bilingAddress = await _addressService.GetAddressByIdAsync(postProcessPaymentRequest.Order.BillingAddressId);
            var country = await _countryService.GetCountryByIdAsync(bilingAddress?.Id ?? 0);

            AddPaymentGatewayField("CustomerFirstName", bilingAddress.FirstName);
            AddPaymentGatewayField("CustomerLastName", bilingAddress.LastName);
            AddPaymentGatewayField("CustomerAddress", bilingAddress.Address1);
            AddPaymentGatewayField("CustomerCity", bilingAddress.City);
            AddPaymentGatewayField("CustomerZIP", bilingAddress.ZipPostalCode);
            AddPaymentGatewayField("CustomerCountry", (country != null) ? country.TwoLetterIsoCode : "");
            AddPaymentGatewayField("CustomerEmail", bilingAddress.Email);
            AddPaymentGatewayField("CustomerPhone", bilingAddress.PhoneNumber);
        }

        private async Task CreateSettingsAsync()
        {
            var settings = await _settingService.LoadSettingAsync<WsPayPaymentSettings>();

            if(settings == null)
            {
                //settings
                settings = new WsPayPaymentSettings
                {
                    UseSandbox = true,
                    PaymentUrl = "",
                    PaymentUrlTest = "",
                    SecretKey = "Shop secret key here...",
                    ShopId = "",
                    ShopIdTest = "",
                    SecretKeyTest = "Shop secret key here...",
                };
                await _settingService.SaveSettingAsync(settings);
            }
        }

        private async Task DeleteSettingsAsync()
        {
            var settings = await _settingService.LoadSettingAsync<WsPayPaymentSettings>();

            if(settings != null)
            {
                await _settingService.DeleteSettingAsync<WsPayPaymentSettings>();
            }
        }

        private async Task AddLocalizationAsync()
        {
            var languages = _languageRepository.Table.ToList();
            foreach (var language in languages)
            {
                var document = new XmlDocument();

                var path = _hostingEnviroment.ContentRootPath + "\\Plugins\\Payments.WsPay\\Localization\\WsPay_localization_" + language.UniqueSeoCode + ".xml";
                try
                {
                    document.Load(path);

                    foreach (XmlNode node in document.SelectSingleNode("Localization").ChildNodes)
                    {
                        var localizationName = node.Attributes["Name"].Value;
                        var localizationValue = node.ChildNodes[0].InnerText;

                        LocaleStringResource localeStringResource = new LocaleStringResource
                        {
                            LanguageId = language.Id,
                            ResourceName = localizationName,
                            ResourceValue = localizationValue,
                        };

                        await _localizationService.InsertLocaleStringResourceAsync(localeStringResource);
                    }
                }
                catch
                {
                    await _logger.InsertLogAsync(LogLevel.Information, "No localization .xml file for language: " + language.UniqueSeoCode + " - " + language.Name);
                }
            }
        }

        private async Task DeleteLocalizationAsync()
        {
            var language = _languageRepository.Table.FirstOrDefault();
            if (language != null)
            {
                var document = new XmlDocument();

                var path = _hostingEnviroment.ContentRootPath + "\\Plugins\\Payments.WsPay\\Localization\\WsPay_localization_" + language.UniqueSeoCode + ".xml";
                try
                {
                    document.Load(path);

                    foreach (XmlNode node in document.SelectSingleNode("Localization").ChildNodes)
                    {
                        var localizationName = node.Attributes["Name"].Value;

                        await _localizationService.DeleteLocaleResourceAsync(localizationName);
                    }
                }
                catch
                {
                    await _logger.InsertLogAsync(LogLevel.Information, "No localization .xml file for language: " + language.UniqueSeoCode + " - " + language.Name);
                }
            }
        }

        private async Task CreateErrorTopicPageAsync()
        {
            var errorTopicPage = await _topicService.GetTopicBySystemNameAsync(WsPayDefaults.ERROR_TOPIC_PAGE_SYSTEM_NAME);

            if (errorTopicPage != null)
            {
                errorTopicPage.SystemName = WsPayDefaults.ERROR_TOPIC_PAGE_SYSTEM_NAME;
                errorTopicPage.Title = WsPayDefaults.ERROR_TOPIC_PAGE_TITLE;
                errorTopicPage.IncludeInSitemap = false;
                errorTopicPage.IncludeInTopMenu = false;
                errorTopicPage.IncludeInFooterColumn1 = false;
                errorTopicPage.IncludeInFooterColumn2 = false;
                errorTopicPage.IncludeInFooterColumn3 = false;
                errorTopicPage.DisplayOrder = 1;
                errorTopicPage.AccessibleWhenStoreClosed = false;
                errorTopicPage.IsPasswordProtected = false;
                errorTopicPage.Password = null;
                errorTopicPage.Body = WsPayDefaults.ERROR_TOPIC_PAGE_BODY;
                errorTopicPage.Published = true;
                errorTopicPage.TopicTemplateId = 1;
                errorTopicPage.MetaKeywords = null;
                errorTopicPage.MetaDescription = null;
                errorTopicPage.MetaTitle = null;
                errorTopicPage.SubjectToAcl = false;
                errorTopicPage.LimitedToStores = false;

                await _topicService.UpdateTopicAsync(errorTopicPage);
            }
            else
            {
                errorTopicPage = new Topic
                {
                    SystemName = WsPayDefaults.ERROR_TOPIC_PAGE_SYSTEM_NAME,
                    Title = WsPayDefaults.ERROR_TOPIC_PAGE_TITLE,
                    IncludeInSitemap = false,
                    IncludeInTopMenu = false,
                    IncludeInFooterColumn1 = false,
                    IncludeInFooterColumn2 = false,
                    IncludeInFooterColumn3 = false,
                    DisplayOrder = 1,
                    AccessibleWhenStoreClosed = false,
                    IsPasswordProtected = false,
                    Password = null,
                    Body = WsPayDefaults.ERROR_TOPIC_PAGE_BODY,
                    Published = true,
                    TopicTemplateId = 1,
                    MetaKeywords = null,
                    MetaDescription = null,
                    MetaTitle = null,
                    SubjectToAcl = false,
                    LimitedToStores = false,
                };

                await _topicService.InsertTopicAsync(errorTopicPage);
            }

            var errorTopicPageUrl = _urlRecordRepository.Table.Where(a => a.EntityName == "Topic" && a.EntityId == errorTopicPage.Id).ToList();

            if (errorTopicPageUrl == null || errorTopicPageUrl.Count == 0)
            {
                var newErrorTopicPageUrl = new UrlRecord
                {
                    EntityId = errorTopicPage.Id,
                    EntityName = "Topic",
                    Slug = "wspay-error",
                    IsActive = true,
                    LanguageId = 0,
                };
                await _urlRecordRepository.InsertAsync(newErrorTopicPageUrl);
            }
        }

        private async Task DeleteErrorTopicPageAsync()
        {
            var errorTopicPage = await _topicService.GetTopicBySystemNameAsync(WsPayDefaults.ERROR_TOPIC_PAGE_SYSTEM_NAME);
            if (errorTopicPage != null)
            {
                var urlRecords = _urlRecordRepository.Table.Where(a => a.EntityName == "Topic" && a.EntityId == errorTopicPage.Id).ToList();
                foreach (var urlRecord in urlRecords)
                {
                    await _urlRecordRepository.DeleteAsync(urlRecord);
                }
                await _topicService.DeleteTopicAsync(errorTopicPage);
            }
        }

        #endregion
    }
}
