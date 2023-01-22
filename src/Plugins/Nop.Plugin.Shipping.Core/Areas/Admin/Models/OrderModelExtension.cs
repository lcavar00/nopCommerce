using Nop.Plugin.Shipping.Core.Models;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Models
{
    public record OrderModelExtension : OrderModel
    {
        public OrderModelExtension()
        {

        }

        public OrderModelExtension(OrderModel model)
        {
            AddOrderNoteDisplayToCustomer = model.AddOrderNoteDisplayToCustomer;
            AddOrderNoteDownloadId = model.AddOrderNoteDownloadId;
            AddOrderNoteHasDownload = model.AddOrderNoteHasDownload;
            AddOrderNoteMessage = model.AddOrderNoteMessage;
            AffiliateId = model.AffiliateId;
            AffiliateName = model.AffiliateName;
            AllowCustomersToSelectTaxDisplayType = model.AllowCustomersToSelectTaxDisplayType;
            AllowStoringCreditCardNumber = model.AllowStoringCreditCardNumber;
            AmountToRefund = model.AmountToRefund;
            AuthorizationTransactionId = model.AuthorizationTransactionId;
            BillingAddress = model.BillingAddress;
            CanAddNewShipments = model.CanAddNewShipments;
            CanCancelOrder = model.CanCancelOrder;
            CanCapture = model.CanCapture;
            CanMarkOrderAsPaid = model.CanMarkOrderAsPaid;
            CanPartiallyRefund = model.CanPartiallyRefund;
            CanPartiallyRefundOffline = model.CanPartiallyRefundOffline;
            CanRefund = model.CanRefund;
            CanRefundOffline = model.CanRefundOffline;
            CanVoid = model.CanVoid;
            CanVoidOffline = model.CanVoidOffline;
            CaptureTransactionId = model.CaptureTransactionId;
            CardCvv2 = model.CardCvv2;
            CardExpirationMonth = model.CardExpirationMonth;
            CardExpirationYear = model.CardExpirationYear;
            CardName = model.CardName;
            CardNumber = model.CardNumber;
            CardType = model.CardType;
            CheckoutAttributeInfo = model.CheckoutAttributeInfo;
            CreatedOn = model.CreatedOn;
            CustomerEmail = model.CustomerEmail;
            CustomerFullName = model.CustomerFullName;
            CustomerId = model.CustomerId;
            CustomerInfo = model.CustomerInfo;
            CustomerIp = model.CustomerIp;
            CustomOrderNumber = model.CustomOrderNumber;
            CustomProperties = model.CustomProperties;
            CustomValues = model.CustomValues;
            DisplayTax = model.DisplayTax;
            DisplayTaxRates = model.DisplayTaxRates;
            GiftCards = model.GiftCards;
            HasDownloadableProducts = model.HasDownloadableProducts;
            Id = model.Id;
            IsLoggedInAsVendor = model.IsLoggedInAsVendor;
            IsShippable = model.IsShippable;
            Items = model.Items;
            MaxAmountToRefund = model.MaxAmountToRefund;
            OrderGuid = model.OrderGuid;
            OrderNoteSearchModel = model.OrderNoteSearchModel;
            OrderShipmentSearchModel = model.OrderShipmentSearchModel;
            OrderShippingExclTax = model.OrderShippingExclTax;
            OrderShippingExclTaxValue = model.OrderShippingExclTaxValue;
            OrderShippingInclTax = model.OrderShippingInclTax;
            OrderShippingInclTaxValue = model.OrderShippingInclTaxValue;
            OrderStatus = model.OrderStatus;
            OrderStatusId = model.OrderStatusId;
            OrderSubTotalDiscountExclTax = model.OrderSubTotalDiscountExclTax;
            OrderSubTotalDiscountExclTaxValue = model.OrderSubTotalDiscountExclTaxValue;
            OrderSubTotalDiscountInclTax = model.OrderSubTotalDiscountInclTax;
            OrderSubTotalDiscountInclTaxValue = model.OrderSubTotalDiscountInclTaxValue;
            OrderSubtotalExclTax = model.OrderSubtotalExclTax;
            OrderSubtotalExclTaxValue = model.OrderSubtotalExclTaxValue;
            OrderSubtotalInclTax = model.OrderSubtotalInclTax;
            OrderSubtotalInclTaxValue = model.OrderSubtotalInclTaxValue;
            OrderTotal = model.OrderTotal;
            OrderTotalDiscount = model.OrderTotalDiscount;
            OrderTotalDiscountValue = model.OrderTotalDiscountValue;
            OrderTotalValue = model.OrderTotalValue;
            PaymentMethod = model.PaymentMethod;
            PaymentMethodAdditionalFeeExclTax = model.PaymentMethodAdditionalFeeExclTax;
            PaymentMethodAdditionalFeeExclTaxValue = model.PaymentMethodAdditionalFeeExclTaxValue;
            PaymentMethodAdditionalFeeInclTax = model.PaymentMethodAdditionalFeeInclTax;
            PaymentMethodAdditionalFeeInclTaxValue = model.PaymentMethodAdditionalFeeInclTaxValue;
            PaymentStatus = model.PaymentStatus;
            PaymentStatusId = model.PaymentStatusId;
            PickupAddress = model.PickupAddress;
            PickupAddressGoogleMapsUrl = model.PickupAddressGoogleMapsUrl;
            PickupInStore = model.PickupInStore;
            PrimaryStoreCurrencyCode = model.PrimaryStoreCurrencyCode;
            Profit = model.Profit;
            RecurringPaymentId = model.RecurringPaymentId;
            RedeemedRewardPoints = model.RedeemedRewardPoints;
            RedeemedRewardPointsAmount = model.RedeemedRewardPointsAmount;
            RefundedAmount = model.RefundedAmount;
            ShippingAddress = model.ShippingAddress;
            ShippingAddressGoogleMapsUrl = model.ShippingAddressGoogleMapsUrl;
            ShippingMethod = model.ShippingMethod;
            ShippingStatus = model.ShippingStatus;
            ShippingStatusId = model.ShippingStatusId;
            StoreName = model.StoreName;
            SubscriptionTransactionId = model.SubscriptionTransactionId;
            Tax = model.Tax;
            TaxDisplayType = model.TaxDisplayType;
            TaxRates = model.TaxRates;
            TaxRatesValue = model.TaxRatesValue;
            TaxValue = model.TaxValue;
            UsedDiscounts = model.UsedDiscounts;
            VatNumber = model.VatNumber;
        }

        public ShipmentRequestModel ShipmentRequestModel { get; set; }
    }
}
