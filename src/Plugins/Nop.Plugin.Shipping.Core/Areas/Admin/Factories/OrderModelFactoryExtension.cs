using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Plugin.Shipping.Core.Areas.Admin.Models;
using Nop.Plugin.Shipping.Core.Factories;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Factories
{
    public class OrderModelFactoryExtension : OrderModelFactory, IOrderModelFactoryExtension
    {
        #region Fields

        private readonly IAffiliateService _affiliateService;
        private readonly ICustomerService _customerService;
        private readonly IDateTimeHelper _dateTimeHelper;
        private readonly ILocalizationService _localizationService;
        private readonly IPaymentService _paymentService;
        private readonly IShipmentRequestModelFactory _shipmentRequestModelFactory;
        private readonly IStoreService _storeService;
        private readonly IWorkContext _workContext;
        private readonly TaxSettings _taxSettings;

        #endregion

        #region Ctor

        public OrderModelFactoryExtension(AddressSettings addressSettings,
            CatalogSettings catalogSettings, 
            CurrencySettings currencySettings, 
            IActionContextAccessor actionContextAccessor, 
            IAddressModelFactory addressModelFactory, 
            IAddressService addressService, 
            IAffiliateService affiliateService, 
            IBaseAdminModelFactory baseAdminModelFactory, 
            ICountryService countryService, 
            ICurrencyService currencyService, 
            ICustomerService customerService, 
            IDateTimeHelper dateTimeHelper, 
            IDiscountService discountService, 
            IDownloadService downloadService, 
            IEncryptionService encryptionService, 
            IGiftCardService giftCardService, 
            ILocalizationService localizationService, 
            IMeasureService measureService, 
            IOrderProcessingService orderProcessingService, 
            IOrderReportService orderReportService, 
            IOrderService orderService, 
            IPaymentPluginManager paymentPluginManager, 
            IPaymentService paymentService, 
            IPictureService pictureService, 
            IPriceCalculationService priceCalculationService, 
            IPriceFormatter priceFormatter, 
            IProductAttributeService productAttributeService, 
            IProductService productService, 
            IReturnRequestService returnRequestService, 
            IRewardPointService rewardPointService, 
            IShipmentRequestModelFactory shipmentRequestModelFactory,
            IShipmentService shipmentService, 
            IShippingService shippingService, 
            IStateProvinceService stateProvinceService, 
            IStoreService storeService, 
            ITaxService taxService, 
            IUrlHelperFactory urlHelperFactory,
            IUrlRecordService urlRecordService,
            IVendorService vendorService, 
            IWorkContext workContext, 
            MeasureSettings measureSettings, 
            OrderSettings orderSettings, 
            ShippingSettings shippingSettings, 
            TaxSettings taxSettings) : base(addressSettings, catalogSettings, currencySettings, actionContextAccessor, addressModelFactory, addressService, affiliateService, baseAdminModelFactory, countryService, currencyService, customerService, dateTimeHelper, discountService, downloadService, encryptionService, giftCardService, localizationService, measureService, orderProcessingService, orderReportService, orderService, paymentPluginManager, paymentService, pictureService, priceCalculationService, priceFormatter, productAttributeService, productService, returnRequestService, rewardPointService, shipmentService, shippingService, stateProvinceService, storeService, taxService, urlHelperFactory, vendorService, workContext, measureSettings, orderSettings, shippingSettings, urlRecordService, taxSettings)
        {
            _affiliateService = affiliateService;
            _customerService = customerService;
            _dateTimeHelper = dateTimeHelper;
            _localizationService = localizationService;
            _paymentService = paymentService;
            _shipmentRequestModelFactory = shipmentRequestModelFactory;
            _storeService = storeService;
            _taxSettings = taxSettings;
            _workContext = workContext;
        }

        #endregion

        public async Task<OrderModelExtension> PrepareOrderModelExtensionAsync(OrderModel orderModel, Order order, bool excludeProperties = false)
        {
            if (order != null)
            {
                //fill in orderModel values from the entity
                orderModel ??= new OrderModel
                {
                    Id = order.Id,
                    OrderStatusId = order.OrderStatusId,
                    VatNumber = order.VatNumber,
                    CheckoutAttributeInfo = order.CheckoutAttributeDescription
                };

                var customer = await _customerService.GetCustomerByIdAsync(order.CustomerId);

                orderModel.OrderGuid = order.OrderGuid;
                orderModel.CustomOrderNumber = order.CustomOrderNumber;
                orderModel.CustomerIp = order.CustomerIp;
                orderModel.CustomerId = customer.Id;
                orderModel.OrderStatus = await _localizationService.GetLocalizedEnumAsync(order.OrderStatus);
                orderModel.StoreName = _storeService.GetStoreByIdAsync(order.StoreId).Result.Name ?? "Deleted";
                orderModel.CustomerInfo = await _customerService.IsRegisteredAsync(customer) ? customer.Email : await _localizationService.GetResourceAsync("Admin.Customers.Guest");
                orderModel.CreatedOn = await _dateTimeHelper.ConvertToUserTimeAsync(order.CreatedOnUtc, DateTimeKind.Utc);
                orderModel.CustomValues = _paymentService.DeserializeCustomValues(order);

                var affiliate = await _affiliateService.GetAffiliateByIdAsync(order.AffiliateId);
                if (affiliate != null)
                {
                    orderModel.AffiliateId = affiliate.Id;
                    orderModel.AffiliateName = await _affiliateService.GetAffiliateFullNameAsync(affiliate);
                }

                //prepare order totals
                await PrepareOrderModelTotalsAsync(orderModel, order);

                //prepare order items
                await PrepareOrderItemModelsAsync(orderModel.Items, order);
                orderModel.HasDownloadableProducts = orderModel.Items.Any(item => item.IsDownload);

                //prepare payment info
                await PrepareOrderModelPaymentInfoAsync(orderModel, order);

                //prepare shipping info
                await PrepareOrderModelShippingInfoAsync(orderModel, order);

                //prepare nested search orderModel
                PrepareOrderShipmentSearchModel(orderModel.OrderShipmentSearchModel, order);
                PrepareOrderNoteSearchModel(orderModel.OrderNoteSearchModel, order);
            }

            orderModel.IsLoggedInAsVendor = await _workContext.GetCurrentVendorAsync() != null;
            orderModel.AllowCustomersToSelectTaxDisplayType = _taxSettings.AllowCustomersToSelectTaxDisplayType;
            orderModel.TaxDisplayType = _taxSettings.TaxDisplayType;

            var model = new OrderModelExtension(orderModel)
            {
                ShipmentRequestModel = await _shipmentRequestModelFactory.PrepareShipmentRequestModelAsync(order)
            };

            return model;
        }
    }
}
