using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Core.Domain;
using Nop.Plugin.Shipping.Core.Services;
using Nop.Plugin.Shipping.DPD.Models;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.DPD.Services
{
    public class DPDApiService : IDPDApiService
    {
        #region Fields

        private readonly DPDSettings _dpdSettings;
        private readonly IAddressAttributeParser _addressAttributeParser;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IProductService _productService;
        private readonly IShipmentRequestService _shipmentRequestService;
        private readonly IShipmentService _shipmentService;

        #endregion

        #region Ctor

        public DPDApiService(DPDSettings dpdSettings,
            IAddressAttributeParser addressAttributeParser,
            IAddressService addressService,
            ICountryService countryService,
            ILogger logger,
            IOrderService orderService,
            IProductService productService,
            IShipmentRequestService shipmentRequestService,
            IShipmentService shipmentService)
        {
            _dpdSettings = dpdSettings;
            _addressAttributeParser = addressAttributeParser;
            _addressService = addressService;
            _countryService = countryService;
            _logger = logger;
            _orderService = orderService;
            _productService = productService;
            _shipmentRequestService = shipmentRequestService;
            _shipmentService = shipmentService;
        }

        #endregion

        public async Task<DeleteOrCancelParcelResponse> CancelParcelsAsync(Order order)
        {
            return await CancelParcelsAsync(await _shipmentService.GetShipmentsByOrderIdAsync(order.Id));
        }

        public async Task<DeleteOrCancelParcelResponse> CancelParcelsAsync(IList<Shipment> shipments)
        {
            try
            {
                var trackingNumbers = string.Empty;
                foreach (var shipment in shipments)
                {
                    trackingNumbers += $"{shipment.TrackingNumber},";
                }
                _ = trackingNumbers.Remove(trackingNumbers.LastIndexOf(","), trackingNumbers.Length);
                var url = _dpdSettings.UseSandbox ? _dpdSettings.TestWebServiceUrl : _dpdSettings.WebServiceUrl;
                var uri = new Uri($"{url}/parcel/parcel_cancel");
                var uriBuilder = new UriBuilder(uri);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["username"] = _dpdSettings.UseSandbox ? _dpdSettings.TestUsername : _dpdSettings.Username;
                query["password"] = _dpdSettings.UseSandbox ? _dpdSettings.TestPassword : _dpdSettings.Password;
                query["parcels"] = trackingNumbers;

                uriBuilder.Query = query.ToString();
                var req = (HttpWebRequest)WebRequest.Create(uriBuilder.ToString());
                req.ContentType = string.Empty;
                req.Method = "POST";


                var response = (HttpWebResponse)req.GetResponse();

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    var responseString = sr.ReadToEnd();

                    return DeserializeToObject<DeleteOrCancelParcelResponse>(responseString);
                }
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
                return null;
            }
        }

        public async Task<DeleteOrCancelParcelResponse> CancelParcelAsync(Shipment shipment)
        {
            try
            {
                var url = _dpdSettings.UseSandbox ? _dpdSettings.TestWebServiceUrl : _dpdSettings.WebServiceUrl;
                var uri = new Uri($"{url}/parcel/parcel_cancel");
                var uriBuilder = new UriBuilder(uri);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["username"] = _dpdSettings.UseSandbox ? _dpdSettings.TestUsername : _dpdSettings.Username;
                query["password"] = _dpdSettings.UseSandbox ? _dpdSettings.TestPassword : _dpdSettings.Password;
                query["parcels"] = shipment.TrackingNumber;

                uriBuilder.Query = query.ToString();
                var req = (HttpWebRequest)WebRequest.Create(uriBuilder.ToString());
                req.ContentType = string.Empty;
                req.Method = "POST";


                var response = (HttpWebResponse)req.GetResponse();

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    var responseString = sr.ReadToEnd();

                    return DeserializeToObject<DeleteOrCancelParcelResponse>(responseString);
                }
            }
            catch(Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
                return null;
            }
        }

        public async Task<DeleteOrCancelParcelResponse> CancelParcelAsync(ShipmentRequest shipmentRequest)
        {
            return shipmentRequest.ShipmentId != null ? await CancelParcelByShipmentIdAsync(shipmentRequest.ShipmentId ?? 0) : null;
        }

        public async Task<DeleteOrCancelParcelResponse> CancelParcelsByOrderIdAsync(int orderId)
        {
            var shipments = await _shipmentService.GetShipmentsByOrderIdAsync(orderId);
            return await CancelParcelsAsync(shipments);
        }

        public async Task<DeleteOrCancelParcelResponse> CancelParcelByShipmentIdAsync(int shipmentId)
        {
            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);

            return await CancelParcelAsync(shipment);
        }

        public async Task<DeleteOrCancelParcelResponse> CancelParcelByShipmentRequestIdAsync(int shipmentRequestId)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByIdAsync(shipmentRequestId);

            return await CancelParcelAsync(shipmentRequest);
        }

        public async Task<IList<DeleteOrCancelParcelResponse>> DeleteParcelsAsync(Order order)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByOrderIdAsync(order.Id);

            var model = new List<DeleteOrCancelParcelResponse>
            {
                await DeleteParcelAsync(shipmentRequest)
            };
            return model;
        }

        public async Task<DeleteOrCancelParcelResponse> DeleteParcelAsync(ShipmentRequest shipmentRequest)
        {
            try
            {
                var url = _dpdSettings.UseSandbox ? _dpdSettings.TestWebServiceUrl : _dpdSettings.WebServiceUrl;
                var uri = new Uri($"{url}/parcel/parcel_delete");
                var uriBuilder = new UriBuilder(uri);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["username"] = _dpdSettings.UseSandbox ? _dpdSettings.TestUsername : _dpdSettings.Username;
                query["password"] = _dpdSettings.UseSandbox ? _dpdSettings.TestPassword : _dpdSettings.Password;
                query["parcels"] = shipmentRequest.PackageReference;

                uriBuilder.Query = query.ToString();
                var req = (HttpWebRequest)WebRequest.Create(uriBuilder.ToString());
                req.ContentType = string.Empty;
                req.Method = "POST";


                var response = (HttpWebResponse)req.GetResponse();

                using var sr = new StreamReader(response.GetResponseStream());
                var responseString = sr.ReadToEnd();

                return DeserializeToObject<DeleteOrCancelParcelResponse>(responseString);
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
                return null;
            }
        }

        public async Task<IList<DeleteOrCancelParcelResponse>> DeleteParcelsByOrderIdAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            return await DeleteParcelsAsync(order);
        }

        public async Task<DeleteOrCancelParcelResponse> DeleteParcelByShipmentRequestIdAsync(int shipmentRequestId)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByIdAsync(shipmentRequestId);

            return await DeleteParcelAsync(shipmentRequest);
        }

        public async Task<ParcelResponse> GenerateParcelsAsync(Order order, Address pickupAddress)
        {
            try
            {
                var recipientAddress = await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? order.BillingAddressId);
                var numberOfParcels = 1;
                decimal weight = 0;
                foreach (var orderItem in await _orderService.GetOrderItemsAsync(order.Id))
                {
                    var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
                    if (product.ShipSeparately)
                        numberOfParcels += 1;

                    weight += product.Weight;
                }
                var url = _dpdSettings.UseSandbox ? _dpdSettings.TestWebServiceUrl : _dpdSettings.WebServiceUrl;
                var uri = new Uri($"{url}/parcel/parcel_import");
                var uriBuilder = new UriBuilder(uri);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["username"] = _dpdSettings.UseSandbox ? _dpdSettings.TestUsername : _dpdSettings.Username;
                query["password"] = _dpdSettings.UseSandbox ? _dpdSettings.TestPassword : _dpdSettings.Password;
                query["name1"] = $"{recipientAddress.FirstName} {recipientAddress.LastName}";
                query["name2"] = string.Empty;
                query["contact"] = string.Empty;
                var addressAttributes = await _addressAttributeParser.ParseAddressAttributesAsync(recipientAddress.CustomAttributes);
                var houseNumber = _addressAttributeParser.ParseValues(recipientAddress.CustomAttributes, addressAttributes.FirstOrDefault(a => a.Name == "Address 1 House Number")?.Id ?? 0) ?? Array.Empty<string>();
               
                query["street"] = $"{recipientAddress.Address1}";
                query["rPropNum"] = $" {houseNumber[0] ?? string.Empty}".Trim();
                query["city"] = recipientAddress.City;
                query["country"] = (await _countryService.GetCountryByAddressAsync(recipientAddress)).TwoLetterIsoCode;
                query["pcode"] = recipientAddress.ZipPostalCode;
                query["email"] = recipientAddress.Email;
                query["phone"] = recipientAddress.PhoneNumber;
                query["remark"] = string.Empty;
                query["weight"] = ((int)weight).ToString();
                query["order_number"] = order.CustomOrderNumber;
                query["parcel_type"] = "D";
                query["num_of_parcel"] = numberOfParcels.ToString();
                if (order.PaymentMethodSystemName.ToLower().Contains("cod") || order.PaymentMethodSystemName.Replace(" ", string.Empty).Contains("CashOnDelivery"))
                {
                    query["parcel_cod_type"] = "firstonly";
                    query["cod_amount"] = order.OrderTotal.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
                    query["cod_purpose"] = order.CustomOrderNumber;
                    query["parcel_type"] = "D-COD";
                }
                //query["predict"] = string.Empty; //WIP
                //query["is_id_check"] = string.Empty; //WIP
                //query["id_check_receiver"] = string.Empty; //WIP
                //query["id_check_num"] = string.Empty; //WIP
                query["sender_name"] = $"{pickupAddress.FirstName} {pickupAddress.LastName}";
                query["sender_city"] = pickupAddress.City;
                query["sender_pcode"] = pickupAddress.ZipPostalCode;
                query["sender_country"] = (await _countryService.GetCountryByAddressAsync(pickupAddress)).TwoLetterIsoCode;
                query["sender_street"] = $"{pickupAddress.Address1} {pickupAddress.Address2}";
                query["sender_contact"] = $"{pickupAddress.FirstName} {pickupAddress.LastName}";
                query["sender_phone"] = pickupAddress.PhoneNumber;
                //query["pudo_id"] = string.Empty; //WIP


                uriBuilder.Query = query.ToString();
                var req = (HttpWebRequest)WebRequest.Create(uriBuilder.ToString());
                req.ContentType = string.Empty;
                req.Method = "POST";

                var response = (HttpWebResponse)req.GetResponse();

                using var sr = new StreamReader(response.GetResponseStream());
                var responseString = sr.ReadToEnd();

                return DeserializeToObject<ParcelResponse>(responseString);
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
                return null;
            }
        }

        public async Task<ParcelResponse> GenerateParcelsByOrderIdAsync(int orderId, Address address)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            return await GenerateParcelsAsync(order, address);
        }

        public async Task<byte[]> GetParcelManifestAsync(Order order)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByOrderIdAsync(order.Id);

            return await GetParcelManifestAsync(shipmentRequest);
        }

        public async Task<byte[]> GetParcelManifestAsync(ShipmentRequest shipmentRequest)
        {
            try
            {
                var url = _dpdSettings.UseSandbox ? _dpdSettings.TestWebServiceUrl : _dpdSettings.WebServiceUrl;
                var uri = new Uri($"{url}/parcel/parcel_manifest_print");
                var uriBuilder = new UriBuilder(uri);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["username"] = _dpdSettings.UseSandbox ? _dpdSettings.TestUsername : _dpdSettings.Username;
                query["password"] = _dpdSettings.UseSandbox ? _dpdSettings.TestPassword : _dpdSettings.Password;
                query["type"] = "manifest";
                query["date"] = shipmentRequest.CreatedAt?.ToString("yyyy-MM-dd");

                uriBuilder.Query = query.ToString();
                var req = (HttpWebRequest)WebRequest.Create(uriBuilder.ToString());
                req.ContentType = string.Empty;
                req.Method = "POST";


                var response = (HttpWebResponse)req.GetResponse();

                using (var sr = new StreamReader(response.GetResponseStream()))
                {
                    var responseString = sr.ReadToEnd();

                    return null; //WIP
                }
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
                return null;
            }
        }

        public async Task<byte[]> GetParcelManifestsByOrderIdAsync(int orderId)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByOrderIdAsync(orderId);

            return await GetParcelManifestAsync(shipmentRequest);
        }

        public async Task<byte[]> GetParcelManifestByShipmentRequestIdAsync(int shipmentRequestId)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByIdAsync(shipmentRequestId);

            return await GetParcelManifestAsync(shipmentRequest);
        }

        public async Task<IEnumerable<ParcelStatusResponse>> GetParcelStatusesAsync(Order order)
        {
            var shipments = await _shipmentService.GetShipmentsByOrderIdAsync(order.Id);
            return await GetParcelStatusesAsync(shipments);
        }

        public async Task<IEnumerable<ParcelStatusResponse>> GetParcelStatusesAsync(IList<Shipment> shipments)
        {
            var statuses = new List<ParcelStatusResponse>();
            foreach(var shipment in shipments)
            {
                statuses.Add(await GetParcelStatusAsync(shipment));
            }

            return statuses;
        }

        public async Task<ParcelStatusResponse> GetParcelStatusAsync(Shipment shipment)
        {
            try
            {
                var url = _dpdSettings.UseSandbox ? _dpdSettings.TestWebServiceUrl : _dpdSettings.WebServiceUrl;
                var uri = new Uri($"{url}/parcel/parcel_status");
                var uriBuilder = new UriBuilder(uri);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["secret"] = "FcJyN7vU7WKPtUh7m1bx";
                query["parcel_number"] = shipment.TrackingNumber;

                uriBuilder.Query = query.ToString();
                var req = (HttpWebRequest)WebRequest.Create(uriBuilder.ToString());
                req.ContentType = string.Empty;
                req.Method = "POST";


                var response = (HttpWebResponse)req.GetResponse();

                using var sr = new StreamReader(response.GetResponseStream());
                var responseString = sr.ReadToEnd();

                return DeserializeToObject<ParcelStatusResponse>(responseString);
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
                return null;
            }
        }

        public async Task<ParcelStatusResponse> GetParcelStatusAsync(ShipmentRequest shipmentRequest)
        {
            try
            {
                var url = _dpdSettings.UseSandbox ? _dpdSettings.TestWebServiceUrl : _dpdSettings.WebServiceUrl;
                var uri = new Uri($"{url}/parcel/parcel_status");
                var uriBuilder = new UriBuilder(uri);
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                query["secret"] = "FcJyN7vU7WKPtUh7m1bx";
                query["parcel_number"] = shipmentRequest.PackageReference;

                uriBuilder.Query = query.ToString();
                var req = (HttpWebRequest)WebRequest.Create(uriBuilder.ToString());
                req.ContentType = string.Empty;
                req.Method = "POST";


                var response = (HttpWebResponse)req.GetResponse();

                using var sr = new StreamReader(response.GetResponseStream());
                var responseString = sr.ReadToEnd();

                return DeserializeToObject<ParcelStatusResponse>(responseString);
            }
            catch (Exception e)
            {
                await _logger.ErrorAsync(e.Message, e);
                return null;
            }
        }

        public async Task<IEnumerable<ParcelStatusResponse>> GetParcelStatusesByOrderIdAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            return await GetParcelStatusesAsync(order);
        }

        public async Task<ParcelStatusResponse> GetParcelStatusByShipmentIdAsync(int shipmentId)
        {
            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);

            return await GetParcelStatusAsync(shipment);
        }

        public async Task<ParcelStatusResponse> GetParcelStatusByShipmentRequestIdAsync(int shipmentRequestId)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByIdAsync(shipmentRequestId);

            return await GetParcelStatusAsync(shipmentRequest);
        }

        public async Task<PrintLabelResponse> PrintLabelAsync(ShipmentRequest shipmentRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<PrintLabelResponse> PrintLabelsByOrderIdAsync(int orderId)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByIdAsync(orderId);
            return await PrintLabelAsync(shipmentRequest);
        }

        public async Task<PrintLabelResponse> PrintLabelByShipmentRequestIdAsync(int shipmentRequestId)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByIdAsync(shipmentRequestId);

            return await PrintLabelAsync(shipmentRequest);
        }
       
        /// <summary>
        /// Object deserialization.
        /// </summary>
        public static T DeserializeToObject<T>(string data)
        {
            var result = default(T);

            if (data == null)
            {
                return result;
            }

            try
            {
                var serializer = new DataContractJsonSerializer(typeof(T));

                var byteArray = Encoding.ASCII.GetBytes(data);
                using var memoryStream = new MemoryStream(byteArray);
                result = (T)serializer.ReadObject(memoryStream);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return result;
        }
    }
}
