using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Areas.Admin.Models.Settings;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Models
{
    public record ShippingSettingsModelExtension : ShippingSettingsModel
    {
        public ShippingSettingsModelExtension(ShippingSettingsModel model)
        {
            ActiveStoreScopeConfiguration = model.ActiveStoreScopeConfiguration;
            AllowPickupInStore = model.AllowPickupInStore;
            AllowPickupInStore_OverrideForStore = model.AllowPickupInStore_OverrideForStore;
            BypassShippingMethodSelectionIfOnlyOne = model.BypassShippingMethodSelectionIfOnlyOne;
            BypassShippingMethodSelectionIfOnlyOne_OverrideForStore = model.BypassShippingMethodSelectionIfOnlyOne_OverrideForStore;
            ConsiderAssociatedProductsDimensions = model.ConsiderAssociatedProductsDimensions;
            ConsiderAssociatedProductsDimensions_OverrideForStore = model.ConsiderAssociatedProductsDimensions_OverrideForStore;
            CustomProperties = model.CustomProperties;
            DisplayPickupPointsOnMap = model.DisplayPickupPointsOnMap;
            DisplayPickupPointsOnMap_OverrideForStore = model.DisplayPickupPointsOnMap_OverrideForStore;
            DisplayShipmentEventsToCustomers = model.DisplayShipmentEventsToCustomers;
            DisplayShipmentEventsToCustomers_OverrideForStore = model.DisplayShipmentEventsToCustomers_OverrideForStore;
            DisplayShipmentEventsToStoreOwner = model.DisplayShipmentEventsToStoreOwner;
            DisplayShipmentEventsToStoreOwner_OverrideForStore = model.DisplayShipmentEventsToStoreOwner_OverrideForStore;
            EstimateShippingCartPageEnabled = model.EstimateShippingCartPageEnabled;
            EstimateShippingCartPageEnabled_OverrideForStore = model.EstimateShippingCartPageEnabled_OverrideForStore;
            EstimateShippingProductPageEnabled = model.EstimateShippingProductPageEnabled;
            EstimateShippingProductPageEnabled_OverrideForStore = model.EstimateShippingProductPageEnabled_OverrideForStore;
            FreeShippingOverXEnabled = model.FreeShippingOverXEnabled;
            FreeShippingOverXEnabled_OverrideForStore = model.FreeShippingOverXEnabled_OverrideForStore;
            FreeShippingOverXIncludingTax = model.FreeShippingOverXIncludingTax;
            FreeShippingOverXIncludingTax_OverrideForStore = model.FreeShippingOverXIncludingTax_OverrideForStore;
            FreeShippingOverXValue = model.FreeShippingOverXValue;
            FreeShippingOverXValue_OverrideForStore = model.FreeShippingOverXValue_OverrideForStore;
            GoogleMapsApiKey = model.GoogleMapsApiKey;
            GoogleMapsApiKey_OverrideForStore = model.GoogleMapsApiKey_OverrideForStore;
            HideShippingTotal = model.HideShippingTotal;
            HideShippingTotal_OverrideForStore = model.HideShippingTotal_OverrideForStore;
            IgnoreAdditionalShippingChargeForPickupInStore = model.IgnoreAdditionalShippingChargeForPickupInStore;
            IgnoreAdditionalShippingChargeForPickupInStore_OverrideForStore = model.IgnoreAdditionalShippingChargeForPickupInStore_OverrideForStore;
            NotifyCustomerAboutShippingFromMultipleLocations = model.NotifyCustomerAboutShippingFromMultipleLocations;
            NotifyCustomerAboutShippingFromMultipleLocations_OverrideForStore = model.NotifyCustomerAboutShippingFromMultipleLocations_OverrideForStore;
            PrimaryStoreCurrencyCode = model.PrimaryStoreCurrencyCode;
            ShippingOriginAddress = model.ShippingOriginAddress;
            ShippingOriginAddress_OverrideForStore = model.ShippingOriginAddress_OverrideForStore;
            ShipToSameAddress = model.ShipToSameAddress;
            ShipToSameAddress_OverrideForStore = model.ShipToSameAddress_OverrideForStore;
            UseWarehouseLocation = model.UseWarehouseLocation;
            UseWarehouseLocation_OverrideForStore = model.UseWarehouseLocation_OverrideForStore;

            ShippingPickupAddressSettingsModel = new ShippingPickupAddressSettingsModel();
        }

        public ShippingSettingsModelExtension()
        {
            ShippingOriginAddress = new AddressModel();
            ShippingPickupAddressSettingsModel = new ShippingPickupAddressSettingsModel();
        }

        public ShippingPickupAddressSettingsModel ShippingPickupAddressSettingsModel { get; set; }
    }
}
