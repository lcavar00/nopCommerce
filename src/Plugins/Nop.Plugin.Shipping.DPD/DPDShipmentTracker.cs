using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Core.Services;
using Nop.Plugin.Shipping.DPD.Services;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping.Tracking;

namespace Nop.Plugin.Shipping.DPD
{
    public class DPDShipmentTracker : IShipmentTracker
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IDPDApiService _dpdApiService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IShipmentRequestService _shipmentRequestService;

        #endregion

        #region Ctor

        public DPDShipmentTracker(IAddressService addressService,
            ICountryService countryService,
            IDPDApiService dpdApiService,
            ILogger logger,
            IOrderService orderService,
            IShipmentRequestService shipmentRequestService)
        {
            _addressService = addressService;
            _countryService = countryService;
            _dpdApiService = dpdApiService;
            _logger = logger;
            _orderService = orderService;
            _shipmentRequestService = shipmentRequestService;
        }

        #endregion

        public async Task<IList<ShipmentStatusEvent>> GetShipmentEventsAsync(string trackingNumber)
        {
            var result = new List<ShipmentStatusEvent>();

            if (string.IsNullOrEmpty(trackingNumber))
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"ShipmentRequest for tracking number : {trackingNumber} can not be found.");
            }

            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByPackageReferenceAsync(trackingNumber);
            if(shipmentRequest == null)
            {
                await _logger.InsertLogAsync(LogLevel.Error, $"ShipmentRequest for tracking number: {trackingNumber} can not be found.");
            }
            else
            {
                var order = await _orderService.GetOrderByIdAsync(shipmentRequest.OrderId);

                int.TryParse(shipmentRequest.PackageReference, out int parcelId);
                var response = await _dpdApiService.GetParcelStatusAsync(shipmentRequest);

                if (response == null || string.IsNullOrEmpty(response.parcel_status))
                {
                    await _logger.InsertLogAsync(LogLevel.Error, $"Tracking number: {trackingNumber}, DPD response is null or empty");
                }
                else
                {
                    var shipmentStatusEvent = new ShipmentStatusEvent
                    {
                        CountryCode = (await _countryService.GetCountryByAddressAsync(await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0)))?.TwoLetterIsoCode ?? string.Empty,
                        Date = DateTime.Now,
                        EventName = $"{response}",
                        Location = string.Empty,
                    };
                    result.Add(shipmentStatusEvent);
                }
            }

            return result;
        }

        public Task<IList<ShipmentStatusEvent>> GetShipmentEventsAsync(string trackingNumber, Shipment shipment = null)
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetUrlAsync(string trackingNumber, Shipment shipment = null)
        {
            return "https://www.dpdgroup.com/hr/mydpd/my-parcels/incoming";
        }

        public async Task<bool> IsMatchAsync(string trackingNumber)
        {
            return trackingNumber.ToCharArray().Length == 14;
        }
    }
}
