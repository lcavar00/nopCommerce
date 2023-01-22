using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Core;
using Nop.Plugin.Shipping.Core.Domain;
using Nop.Plugin.Shipping.Core.Models;
using Nop.Plugin.Shipping.Core.Services;
using Nop.Plugin.Shipping.DPD.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Shipping.DPD
{
    public class DPDPlugin : BasePlugin, IShippingRateComputationMethod, IShippingProviderPlugin
    {
        #region Fields

        private readonly DPDSettings _dpdSettings;
        private readonly IAddressService _addressService;
        private readonly ICountryService _countryService;
        private readonly IDPDApiService _dpdApiService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderService _orderService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly IShippingByLocationByTotalByWeightService _shippingByLocationByTotalByWeightService;
        private readonly IShipmentRequestService _shipmentRequestService;
        private readonly IShipmentService _shipmentService;
        private readonly IShippingService _shippingService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IStoreContext _storeContext;
        private readonly IWebHelper _webHelper;

        #endregion

        #region Ctor

        public DPDPlugin(DPDSettings dpdSettings,
            IAddressService addressService,
            ICountryService countryService,
            IDPDApiService dpdApiService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderService orderService,
            IScheduleTaskService scheduleTaskService,
            ISettingService settingService,
            IShippingByLocationByTotalByWeightService shippingByLocationByTotalByWeightService,
            IShipmentRequestService shipmentRequestService,
            IShipmentService shipmentService,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStoreContext storeContext,
            IWebHelper webHelper)
        {
            _dpdSettings = dpdSettings;
            _addressService = addressService;
            _countryService = countryService;
            _dpdApiService = dpdApiService;
            _localizationService = localizationService;
            _logger = logger;
            _orderService = orderService;
            _scheduleTaskService = scheduleTaskService;
            _shippingByLocationByTotalByWeightService = shippingByLocationByTotalByWeightService;
            _settingService = settingService;
            _shipmentRequestService = shipmentRequestService;
            _shipmentService = shipmentService;
            _shippingService = shippingService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
            _webHelper = webHelper;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get fixed rate
        /// </summary>
        /// <param name="shippingMethodId">Shipping method ID</param>
        /// <returns>Rate</returns>
        private async Task<decimal> GetRateAsync(int shippingMethodId)
        {
            return await _settingService.GetSettingByKeyAsync<decimal>(string.Format(ShippingCoreDefaults.FIXED_RATE_SETTINGS_KEY, shippingMethodId));
        }

        /// <summary>
        /// Get rate by weight and by total
        /// </summary>
        /// <param name="subTotal">Subtotal</param>
        /// <param name="weight">Weight</param>
        /// <param name="shippingMethodId">Shipping method ID</param>
        /// <param name="storeId">Store ID</param>
        /// <param name="warehouseId">Warehouse ID</param>
        /// <param name="countryId">Country ID</param>
        /// <param name="stateProvinceId">State/Province ID</param>
        /// <param name="zip">Zip code</param>
        /// <returns>Rate</returns>
        private async Task<decimal?> GetRateAsync(decimal subTotal, decimal weight, int shippingMethodId,
            int storeId, int warehouseId, int countryId, int stateProvinceId, string zip)
        {
            var shippingByLocationByTotalByWeightRecord = await _shippingByLocationByTotalByWeightService.FindRecordsAsync(shippingMethodId, storeId, warehouseId, countryId, stateProvinceId, zip, weight, subTotal);
            if (shippingByLocationByTotalByWeightRecord == null)
            {
                if (_dpdSettings.LimitMethodsToCreated)
                    return null;

                return decimal.Zero;
            }

            //additional fixed cost
            var shippingTotal = shippingByLocationByTotalByWeightRecord.AdditionalFixedCost;

            //charge amount per weight unit
            if (shippingByLocationByTotalByWeightRecord.RatePerWeightUnit > decimal.Zero)
            {
                var weightRate = Math.Max(weight - shippingByLocationByTotalByWeightRecord.LowerWeightLimit, decimal.Zero);
                shippingTotal += shippingByLocationByTotalByWeightRecord.RatePerWeightUnit * weightRate;
            }

            //percentage rate of subtotal
            if (shippingByLocationByTotalByWeightRecord.PercentageRateOfSubtotal > decimal.Zero)
            {
                shippingTotal += Math.Round((decimal)((((float)subTotal) * ((float)shippingByLocationByTotalByWeightRecord.PercentageRateOfSubtotal)) / 100f), 2);
            }

            return Math.Max(shippingTotal, decimal.Zero);
        }

        #endregion

        #region IShippingRateComputationMethod implementation

        public async Task<IShipmentTracker> GetShipmentTrackerAsync()
        {
            return new DPDShipmentTracker(_addressService, _countryService, _dpdApiService, _logger, _orderService, _shipmentRequestService);
        }

        public IShipmentTracker ShipmentTracker => new DPDShipmentTracker(_addressService, _countryService, _dpdApiService, _logger, _orderService, _shipmentRequestService);

        public async Task<decimal?> GetFixedRateAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            return 0;
        }

        public async Task<GetShippingOptionResponse> GetShippingOptionsAsync(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null || !getShippingOptionRequest.Items.Any())
            {
                response.AddError("No shipment items");
                return response;
            }

            if (_dpdSettings.ShippingByLocationByTotalByWeightEnabled)
            {
                //shipping rate calculation by products weight

                if (getShippingOptionRequest.ShippingAddress == null)
                {
                    response.AddError("Shipping address is not set");
                    return response;
                }

                var storeId = getShippingOptionRequest.StoreId != 0 ? getShippingOptionRequest.StoreId : (await _storeContext.GetCurrentStoreAsync()).Id;
                var countryId = getShippingOptionRequest.ShippingAddress.CountryId ?? 0;
                var stateProvinceId = getShippingOptionRequest.ShippingAddress.StateProvinceId ?? 0;
                var warehouseId = getShippingOptionRequest.WarehouseFrom?.Id ?? 0;
                var zip = getShippingOptionRequest.ShippingAddress.ZipPostalCode;

                //get subtotal of shipped items
                var subTotal = decimal.Zero;
                foreach (var packageItem in getShippingOptionRequest.Items)
                {
                    if (await _shippingService.IsFreeShippingAsync(packageItem.ShoppingCartItem))
                        continue;

                    //TODO we should use getShippingOptionRequest.Items.GetQuantity() method to get subtotal
                    subTotal += (await _shoppingCartService.GetSubTotalAsync(packageItem.ShoppingCartItem, true)).subTotal;
                }

                //get weight of shipped items (excluding items with free shipping)
                var weight = await _shippingService.GetTotalWeightAsync(getShippingOptionRequest, ignoreFreeShippedItems: true);

                foreach (var shippingMethod in (await _shippingService.GetAllShippingMethodsAsync(countryId)).Where(a => a.Name.ToLower().Contains("dpd")))
                {
                    var rate = await GetRateAsync(subTotal, weight, shippingMethod.Id, storeId, warehouseId, countryId, stateProvinceId, zip);
                    if (!rate.HasValue)
                        continue;

                    response.ShippingOptions.Add(new ShippingOption
                    {
                        Name = await _localizationService.GetLocalizedAsync(shippingMethod, x => x.Name),
                        Description = await _localizationService.GetLocalizedAsync(shippingMethod, x => x.Description),
                        Rate = rate.Value
                    });
                }
            }
            else
            {
                //shipping rate calculation by fixed rate
                var restrictByCountryId = (await _countryService.GetCountryByAddressAsync(getShippingOptionRequest.ShippingAddress)).Id;
                response.ShippingOptions = await (await _shippingService.GetAllShippingMethodsAsync(restrictByCountryId)).SelectAwait(async shippingMethod => new ShippingOption
                {
                    Name = await _localizationService.GetLocalizedAsync(shippingMethod, x => x.Name),
                    Description = await _localizationService.GetLocalizedAsync(shippingMethod, x => x.Description),
                    Rate = await GetRateAsync(shippingMethod.Id)
                }).ToListAsync();
            }

            return response;
        }

        #endregion

        #region IShippingProviderPlugin implementation

        public async Task<IList<ShipmentCanceledModel>> CancelShippingRequestToShippingProviderAsync(ShipmentRequest shipmentRequest)
        {
            var order = await _orderService.GetOrderByIdAsync(shipmentRequest.OrderId);

            return await CancelShippingRequestToShippingProviderAsync(order);
        }

        public async Task<IList<ShipmentCanceledModel>> CancelShippingRequestToShippingProviderAsync(Order order)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByOrderIdAsync(order.Id);

            var models = new List<ShipmentCanceledModel>();

            if (shipmentRequest.ShipmentCreated)
            {
                var response = await _dpdApiService.CancelParcelAsync(shipmentRequest);

                if (response != null && (response.status.Contains("1") || response.status.Contains("cancelled")))
                {
                    var model = new ShipmentCanceledModel
                    {
                        Order = order,
                        OrderId = order.Id,
                        ResponseMessage = response.status,
                        ShipmentRequest = shipmentRequest,
                        ShipmentRequestGuid = shipmentRequest.ShipmentRequestGuid,
                        ShipmentRequestId = shipmentRequest.Id,
                    };

                    models.Add(model);
                }
            }
            else
            {
                var response = await _dpdApiService.DeleteParcelAsync(shipmentRequest);

                if (response != null && (response.status.Contains("1") || response.status.Contains("cancelled")))
                {
                    var model = new ShipmentCanceledModel
                    {
                        Order = order,
                        OrderId = order.Id,
                        ResponseMessage = response.status,
                        ShipmentRequest = shipmentRequest,
                        ShipmentRequestGuid = shipmentRequest.ShipmentRequestGuid,
                        ShipmentRequestId = shipmentRequest.Id,
                    };

                    models.Add(model);
                }
            }

            return models;
        }

        public async Task<IList<ShipmentCanceledModel>> CancelShippingRequestToShippingProviderByOrderIdAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            return await CancelShippingRequestToShippingProviderAsync(order);
        }

        public async Task<IList<ShipmentCanceledModel>> CancelShippingRequestToShippingProviderByShipmentRequestIdAsync(int shipmentRequestId)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByIdAsync(shipmentRequestId);

            return await CancelShippingRequestToShippingProviderAsync(shipmentRequest);
        }

        public async Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderAsync(Shipment shipment)
        {
            throw new NotImplementedException();
        }

        public async Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderAsync(Order order)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByOrderIdAsync(order.Id);

            return await GetShippingRequestFromShippingProviderAsync(shipmentRequest);
        }

        public async Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderAsync(ShipmentRequest shipmentRequest)
        {
            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentRequest.ShipmentId ?? 0);
            var order = await _orderService.GetOrderByIdAsync(shipmentRequest.OrderId);
            var response = await _dpdApiService.GetParcelStatusAsync(shipmentRequest);

            var shipmentRequestModel = new ShipmentRequestModel
            {
                Id = shipmentRequest.Id,
                Order = order,
                OrderGuid = shipmentRequest.OrderGuid,
                OrderId = shipmentRequest.OrderId,
                PackageReference = shipmentRequest.PackageReference,
                ShipmentCreated = shipmentRequest.ShipmentCreated,
                Shipment = shipment,
                ShipmentId = shipmentRequest.ShipmentId,
                ShipmentModel = shipment?.ToModel<ShipmentModel>(),
                ShipmentPickupAddressId = shipmentRequest.ShipmentPickupAddressId,
                ShipmentRequestGuid = shipmentRequest.ShipmentRequestGuid,
                ShippingProvider = shipmentRequest.ShippingProvider,
                ShippingProviderSystemName = shipmentRequest.ShippingProviderSystemName,
            };

            var status = DPDDefaults.Statuses.FirstOrDefault(a => a.Key == response.parcel_status).Value;
            order.ShippingStatus = status;
            await _orderService.UpdateOrderAsync(order);

            if (shipment == null)
            {
                shipment = new Shipment
                {
                    CreatedOnUtc = DateTime.Now,
                    OrderId = order.Id,
                    TrackingNumber = shipmentRequest.PackageReference,
                };

                if (order.ShippingStatus == ShippingStatus.Shipped)
                {
                    shipment.ShippedDateUtc = DateTime.UtcNow;
                }
                else if (order.ShippingStatus == ShippingStatus.Delivered)
                {
                    shipment.ShippedDateUtc = DateTime.UtcNow;
                    shipment.DeliveryDateUtc = DateTime.UtcNow;
                }

                await _shipmentService.InsertShipmentAsync(shipment);

                foreach (var orderItem in await _orderService.GetOrderItemsAsync(order.Id))
                {
                    var shipmentItem = new ShipmentItem
                    {
                        ShipmentId = shipment.Id,
                        OrderItemId = orderItem.Id,
                        Quantity = orderItem.Quantity,
                        WarehouseId = (await _shippingService.GetNearestWarehouseAsync(await _addressService.GetAddressByIdAsync(order.ShippingAddressId ?? 0)))?.Id ?? 0
                    };

                    await _shipmentService.InsertShipmentItemAsync(shipmentItem);
                }
            }
            else
            {
                if (order.ShippingStatus == ShippingStatus.Shipped)
                {
                    shipment.ShippedDateUtc = DateTime.UtcNow;
                }
                else if (order.ShippingStatus == ShippingStatus.Delivered)
                {
                    shipment.DeliveryDateUtc = DateTime.UtcNow;
                }
            }

            var model = new UpdatedShipmentRequestModel
            {
                ShipmentRequestModel = shipmentRequestModel,
                ShipmentRequestStatus = response.parcel_status,
                ShipmentStatusEvent = null,
            };

            return model;
        }

        public async Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderAsync(ShipmentRequest shipmentRequest, Order order)
        {
            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentRequest.ShipmentId ?? 0);
            var response = await _dpdApiService.GetParcelStatusAsync(shipmentRequest);

            var shipmentRequestModel = new ShipmentRequestModel
            {
                Id = shipmentRequest.Id,
                Order = order,
                OrderGuid = shipmentRequest.OrderGuid,
                OrderId = shipmentRequest.OrderId,
                PackageReference = shipmentRequest.PackageReference,
                ShipmentCreated = shipmentRequest.ShipmentCreated,
                Shipment = shipment,
                ShipmentId = shipmentRequest.ShipmentId,
                ShipmentModel = shipment.ToModel<ShipmentModel>(),
                ShipmentPickupAddressId = shipmentRequest.ShipmentPickupAddressId,
                ShipmentRequestGuid = shipmentRequest.ShipmentRequestGuid,
                ShippingProvider = shipmentRequest.ShippingProvider,
                ShippingProviderSystemName = shipmentRequest.ShippingProviderSystemName,
            };

            var status = DPDDefaults.Statuses.FirstOrDefault(a => a.Key == response.parcel_status).Value;
            order.ShippingStatus = status;

            await _orderService.UpdateOrderAsync(order);

            var model = new UpdatedShipmentRequestModel
            {
                ShipmentRequestModel = shipmentRequestModel,
                ShipmentRequestStatus = response.parcel_status,
                ShipmentStatusEvent = null,
            };

            return model;
        }

        public async Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderByOrderIdAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            return await GetShippingRequestFromShippingProviderAsync(order);
        }

        public async Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderByShipmentIdAsync(int shipmentId)
        {
            var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentId);

            return await GetShippingRequestFromShippingProviderAsync(shipment);
        }

        public async Task<UpdatedShipmentRequestModel> GetShippingRequestFromShippingProviderByShipmentRequestIdAsync(int shipmentRequestId)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByIdAsync(shipmentRequestId);

            return await GetShippingRequestFromShippingProviderAsync(shipmentRequest);
        }

        public async Task<IList<UpdatedShipmentRequestModel>> GetShippingRequestsFromShippingProviderAsync(IEnumerable<Shipment> shipments)
        {
            var models = new List<UpdatedShipmentRequestModel>();
            foreach(var shipment in shipments)
            {
                var response = await _dpdApiService.GetParcelStatusAsync(shipment);

                var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByShipmentIdAsync(shipment.Id);
                var order = await _orderService.GetOrderByIdAsync(shipment.OrderId);
                var shipmentRequestModel = new ShipmentRequestModel
                {
                    Shipment = shipment,
                    Id = shipmentRequest.Id,
                    Order = order,
                    OrderGuid = order.OrderGuid,
                    OrderId = shipment.OrderId,
                    PackageReference = shipment.TrackingNumber,
                    ShipmentCreated = shipmentRequest.ShipmentCreated,
                    ShipmentId = shipment.Id,
                    ShipmentModel = shipment.ToModel<ShipmentModel>(),
                    ShipmentPickupAddressId = shipmentRequest.ShipmentPickupAddressId,
                    ShipmentRequestGuid = shipmentRequest.ShipmentRequestGuid,
                    ShippingProvider = "Shipping.DPD",
                    ShippingProviderSystemName = "Shipping.DPD",
                };

                var status = DPDDefaults.Statuses.FirstOrDefault(a => a.Key == response.parcel_status).Value;
                order.ShippingStatus = status;

                await _orderService.UpdateOrderAsync(order);

                var model = new UpdatedShipmentRequestModel
                {
                    ShipmentRequestModel = shipmentRequestModel,
                    ShipmentRequestStatus = response.parcel_status,
                    ShipmentStatusEvent = null,
                };

                models.Add(model);
            }

            return models;
        }

        public async Task<IList<UpdatedShipmentRequestModel>> GetShippingRequestsFromShippingProviderAsync(IEnumerable<Order> orders)
        {
            return await GetShippingRequestsFromShippingProviderAsync(orders.SelectManyAwait(async a => await _shipmentService.GetShipmentsByOrderIdAsync(a.Id)).ToEnumerable());
        }

        public async Task<IList<UpdatedShipmentRequestModel>> GetShippingRequestsFromShippingProviderAsync(IEnumerable<ShipmentRequest> shipmentRequests)
        {
            return await GetShippingRequestsFromShippingProviderByShipmentIdsAsync(shipmentRequests.Select(a => a.ShipmentId ?? 0));
        }

        public async Task<IList<UpdatedShipmentRequestModel>> GetShippingRequestsFromShippingProviderByOrderIdsAsync(IEnumerable<int> orderIds)
        {
            var orders = new List<Order>();
            foreach(var orderId in orderIds)
            {
                orders.Add(await _orderService.GetOrderByIdAsync(orderId));
            }

            return await GetShippingRequestsFromShippingProviderAsync(orders);
        }

        public async Task<IList<UpdatedShipmentRequestModel>> GetShippingRequestsFromShippingProviderByShipmentIdsAsync(IEnumerable<int> shipmentIds)
        {
            var shipments = new List<Shipment>();
            foreach(var shipmentId in shipmentIds)
            {
                shipments.Add(await _shipmentService.GetShipmentByIdAsync(shipmentId));
            }

            return await GetShippingRequestsFromShippingProviderAsync(shipments);
        }

        public async Task<IList<UpdatedShipmentRequestModel>> GetShippingRequestsFromShippingProviderByShipmentRequestIdsAsync(IEnumerable<int> shipmentRequestIds)
        {
            var shipmentRequests = new List<ShipmentRequest>();

            foreach(var shipmentRequestId in shipmentRequestIds)
            {
                shipmentRequests.Add(await _shipmentRequestService.GetShipmentRequestByIdAsync(shipmentRequestId));
            }

            return await GetShippingRequestsFromShippingProviderAsync(shipmentRequests);
        }

        public async Task<IList<CreatedShipmentRequestModel>> SendShippingRequestToShippingProviderAsync(int orderId, int addressId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            var packagePickupAddress = await _addressService.GetAddressByIdAsync(addressId);

            return await SendShippingRequestToShippingProviderAsync(order, packagePickupAddress);
        }

        public async Task<IList<CreatedShipmentRequestModel>> SendShippingRequestToShippingProviderAsync(Order order, Address address)
        {
            var response = await _dpdApiService.GenerateParcelsAsync(order, address);

            var models = new List<CreatedShipmentRequestModel>();

            if(response != null)
            {
                var model = new CreatedShipmentRequestModel
                {
                    ResponseMessage = $"{response.status}",
                };

                if(response.status.Contains("err") || response.pl_number == null)
                {
                    model.ResponseMessage = $"{response.status} - {response.errlog}";
                }
                else
                {
					foreach (var pl_number in response.pl_number)
					{
						var shipmentRequestModel = new ShipmentRequestModel
						{
							Id = 0,
							Order = order,
							OrderGuid = order.OrderGuid,
							OrderId = order.Id,
							PackageReference = pl_number,
							Shipment = null,
							ShipmentCreated = false,
							ShipmentId = null,
							ShipmentModel = null,
							ShipmentPickupAddressId = address.Id,
							ShipmentRequestGuid = Guid.NewGuid(),
							ShippingProvider = "Shipping.DPD",
							ShippingProviderSystemName = "Shipping.DPD",
						};
	
						model.ShipmentRequests.Add(shipmentRequestModel);
					}
				}
                models.Add(model);
            }

            return models;
        }

        #endregion

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/DPDPlugin/Configure";
        }

        #region Install / Uninstall

        public override System.Threading.Tasks.Task InstallAsync()
        {
            CreateScheduleTasksAsync();
            AddLocalizationAsync();
            return base.InstallAsync();
        }

        public override System.Threading.Tasks.Task UninstallAsync()
        {
            DeleteScheduleTasksAsync();
            DeleteLocalizationAsync();
            return base.UninstallAsync();
        }

        #endregion

        #region Schedule tasks

        public async Task CreateScheduleTasksAsync()
        {
            var dpdGetParcelStatusTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Shipping.DPD.Tasks.DPDShipmentStatusTask");
            if(dpdGetParcelStatusTask == null)
            {
                dpdGetParcelStatusTask = new ScheduleTask
                {
                    Enabled = false,
                    Name = "DPD get parcel status",
                    Seconds = 43200,
                    StopOnError = false,
                    Type = "Nop.Plugin.Shipping.DPD.Tasks.DPDShipmentStatusTask",
                };
                await _scheduleTaskService.InsertTaskAsync(dpdGetParcelStatusTask);
            }
        }

        public async void DeleteScheduleTasksAsync()
        {
            var dpdGetParcelStatusTask = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Shipping.DPD.Tasks.DPDShipmentStatusTask");
            if(dpdGetParcelStatusTask != null)
            {
                await _scheduleTaskService.DeleteTaskAsync(dpdGetParcelStatusTask);
            }
        }

        #endregion

        #region Localization

        public async void AddLocalizationAsync()
        {
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.Title", "DPD API settings");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.UseSandbox", "Use sandbox:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.ProductionUri", "DPD production API uri:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.ProductionClientNumber", "Client number:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.ProductionUsername", "Username:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.ProductionPassword", "Password:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.TestUri", "DPD test API uri:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.TestClientNumber", "Test client number:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.TestUsername", "Test username:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.TestPassword", "Test password:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.PackageTrackingURL", "Package tracking URL:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.SendPackagePickedUpNotifications", "Send package picked up by DPD notification:");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Description", "Ship packages using DPD");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Name", "DPD");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.AddRecord", "Add record");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.AdditionalFixedCost", "Additional fixed cost");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.AdditionalFixedCost.Hint", "Specify an additional fixed cost per shopping cart for this option. Set to 0 if you don't want an additional fixed cost to be applied.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Country", "Country");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Country.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers, regardless of the country.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.DataHtml", "Data");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.LimitMethodsToCreated", "Limit shipping methods to configured ones");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.LimitMethodsToCreated.Hint", "If you check this option, then your customers will be limited to shipping options configured here. Otherwise, they'll be able to choose any existing shipping options even they are not configured here (zero shipping fee in this case).");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.OrderSubtotalFrom", "Order subtotal from");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.OrderSubtotalFrom.Hint", "Order subtotal from.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.OrderSubtotalTo", "Order subtotal to");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.OrderSubtotalTo.Hint", "Order subtotal to.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.PercentageRateOfSubtotal", "Charge percentage (of subtotal)");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.PercentageRateOfSubtotal.Hint", "Charge percentage (of subtotal).");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Rate", "Rate");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.ShippingMethod", "Shipping method");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.ShippingMethod.Hint", "Choose shipping method.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.StateProvince", "State / province");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.StateProvince.Hint", "If an asterisk is selected, then this shipping rate will apply to all customers from the given country, regardless of the state.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Store", "Store");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Store.Hint", "If an asterisk is selected, then this shipping rate will apply to all stores.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Warehouse", "Warehouse");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Warehouse.Hint", "If an asterisk is selected, then this shipping rate will apply to all warehouses.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Zip", "Zip");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Zip.Hint", "Zip / postal code. If zip is empty, then this shipping rate will apply to all customers from the given country or state, regardless of the zip code.");
            await _localizationService.AddOrUpdateLocaleResourceAsync("Plugins.Shipping.DPD.Fixed", "Fixed Rate");
        }

        public async void DeleteLocalizationAsync()
        {
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.Title");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.UseSandbox");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.ProductionUri");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.ProductionClientNumber");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.ProductionUsername");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.ProductionPassword");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.TestUri");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.TestClientNumber");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.TestUsername");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Admin.Configure.TestPassword");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Description");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Name");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.AddRecord");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.AdditionalFixedCost");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.AdditionalFixedCost.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Country");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Country.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.DataHtml");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.LimitMethodsToCreated");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.LimitMethodsToCreated.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.OrderSubtotalFrom");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.OrderSubtotalFrom.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.OrderSubtotalTo");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.OrderSubtotalTo.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.PercentageRateOfSubtotal");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.PercentageRateOfSubtotal.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Rate");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.ShippingMethod");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.ShippingMethod.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.StateProvince");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.StateProvince.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Store");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Store.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Warehouse");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Warehouse.Hint");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Zip");
            await _localizationService.DeleteLocaleResourceAsync("Plugins.Shipping.DPD.Fields.Zip.Hint");
        }

        #endregion
    }
}
