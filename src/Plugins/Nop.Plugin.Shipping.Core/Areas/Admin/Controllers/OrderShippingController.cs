using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Core.Areas.Admin.Models;
using Nop.Plugin.Shipping.Core.Domain;
using Nop.Plugin.Shipping.Core.Factories;
using Nop.Plugin.Shipping.Core.Models;
using Nop.Plugin.Shipping.Core.Services;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class OrderShippingController : BaseController
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IPermissionService _permissionService;
        private readonly IProductService _productService;
        private readonly IShippingAddressModelFactory _shippingAddressModelFactory;
        private readonly IShippingMethodPluginManager _shippingPluginManager;
        private readonly IShipmentRequestService _shipmentRequestService;
        private readonly IShipmentService _shipmentService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public OrderShippingController(IAddressService addressService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILogger logger,
            INotificationService notificationService,
            IOrderService orderService,
            IPermissionService permissionService,
            IProductService productService,
            IShippingAddressModelFactory shippingAddressModelFactory,
            IShippingMethodPluginManager shippingPluginManager,
            IShipmentRequestService shipmentRequestService,
            IShipmentService shipmentService,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _addressService = addressService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _logger = logger;
            _notificationService = notificationService;
            _orderService = orderService;
            _permissionService = permissionService;
            _productService = productService;
            _shippingAddressModelFactory = shippingAddressModelFactory;
            _shippingPluginManager = shippingPluginManager;
            _shipmentRequestService = shipmentRequestService;
            _shipmentService = shipmentService;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        protected virtual async ValueTask<bool> HasAccessToOrderAsync(Order order)
        {
            return order != null && await HasAccessToOrderAsync(order.Id);
        }

        protected virtual async Task<bool> HasAccessToOrderAsync(int orderId)
        {
            if (orderId == 0)
                return false;

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null)
                //not a vendor; has access
                return true;

            var vendorId = currentVendor.Id;
            var hasVendorProducts = (await _orderService.GetOrderItemsAsync(orderId, vendorId: vendorId)).Any();

            return hasVendorProducts;
        }

        protected virtual async ValueTask<bool> HasAccessToProductAsync(OrderItem orderItem)
        {
            if (orderItem == null || orderItem.ProductId == 0)
                return false;

            var currentVendor = await _workContext.GetCurrentVendorAsync();
            if (currentVendor == null)
                //not a vendor; has access
                return true;

            var vendorId = currentVendor.Id;

            return (await _productService.GetProductByIdAsync(orderItem.ProductId))?.VendorId == vendorId;
        }

        protected virtual async ValueTask<bool> HasAccessToShipmentAsync(Shipment shipment)
        {
            if (shipment == null)
                throw new ArgumentNullException(nameof(shipment));

            if (await _workContext.GetCurrentVendorAsync() == null)
                //not a vendor; has access
                return true;

            return await HasAccessToOrderAsync(shipment.OrderId);
        }

        protected virtual async Task LogEditOrderAsync(int orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            await _customerActivityService.InsertActivityAsync("EditOrder",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.EditOrder"), order.CustomOrderNumber), order);
        }

        #endregion

        [HttpPost]
        public async Task<IActionResult> SendShippingRequest([FromBody] SendShippingRequestModel model)
        {
            var order = await _orderService.GetOrderByIdAsync(model.OrderId);
            var pickupAddress = await _addressService.GetAddressByIdAsync(model.AddressId);

            var plugin = _shippingPluginManager.LoadActivePluginsAsync(await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetActiveStoreScopeConfigurationAsync(), model.ShippingMethod);
            var shippingMethodPlugin = await _shippingPluginManager.LoadPluginBySystemNameAsync(model.ShippingMethod);
            if (!shippingMethodPlugin.PluginDescriptor.Installed)
            {
                _notificationService.ErrorNotification("Shipping provider not found.");
                return StatusCode(StatusCodes.Status500InternalServerError, "Required shipping method plugin not installed.");
            }
            else if (shippingMethodPlugin != null && shippingMethodPlugin is IShippingProviderPlugin)
            {
                order.ShippingMethod = model.ShippingMethod;
                await _orderService.UpdateOrderAsync(order);

                var shippingProviderResponse = await shippingMethodPlugin.SendShippingRequestToShippingProviderAsync(order, pickupAddress);
                if(shippingProviderResponse == null)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, "Shipping provider did not respond");
                }

                foreach (var createdShipmentRequest in shippingProviderResponse)
                {
                    var shipmentRequests = new List<ShipmentRequest>();
                    foreach (var shipmentRequestModel in createdShipmentRequest.ShipmentRequests)
                    {
                        var shipmentRequest = new ShipmentRequest
                        {
                            OrderGuid = shipmentRequestModel.OrderGuid,
                            OrderId = shipmentRequestModel.OrderId,
                            PackageReference = shipmentRequestModel.PackageReference,
                            ShipmentCreated = shipmentRequestModel.ShipmentCreated,
                            ShipmentPickupAddressId = shipmentRequestModel.ShipmentPickupAddressId,
                            ShipmentId = shipmentRequestModel.ShipmentId,
                            ShippingProviderSystemName = shipmentRequestModel.ShippingProviderSystemName,
                            ShippingProvider = shipmentRequestModel.ShippingProvider,
                            CreatedAt = DateTime.Now,
                            UpdatedAt = DateTime.Now,
                            NotificationSent = false,
                            ShipmentRequestGuid = shipmentRequestModel.ShipmentRequestGuid,
                        };

                        shipmentRequests.Add(shipmentRequest);
                    }

                    await _shipmentRequestService.InsertShipmentRequestsAsync(shipmentRequests);

                    if (!string.IsNullOrEmpty(createdShipmentRequest.ResponseMessage))
                    {
                        await _logger.InsertLogAsync(LogLevel.Information, $"Create shipment request response message: {createdShipmentRequest.ResponseMessage}");
                    }
                }
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Shipping method not found.");
            }

            return Ok();
        }

        public async Task<IActionResult> CancelShipmentRequest(int shipmentRequestId)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByIdAsync(shipmentRequestId);

            var order = await _orderService.GetOrderByIdAsync(shipmentRequest.OrderId);

            var shippingMethodPlugin = await _shippingPluginManager.LoadPluginBySystemNameAsync(shipmentRequest.ShippingProviderSystemName);
            if (!shippingMethodPlugin.PluginDescriptor.Installed)
            {
                _notificationService.ErrorNotification("Shipping provider not found.");
            }
            else if (shippingMethodPlugin != null && shippingMethodPlugin is IShippingProviderPlugin)
            {
                var shippingProviderResponse = await shippingMethodPlugin.CancelShippingRequestToShippingProviderAsync(order);
                if (shippingProviderResponse == null)
                {
                    _notificationService.ErrorNotification("Shipping provider did not respond.");
                }

                var deleteShipmentRequests = new List<ShipmentRequest>();
                foreach (var cancelShipmentModel in shippingProviderResponse)
                {
                    var deleteShipmentRequest = await _shipmentRequestService.GetShipmentRequestByIdAsync(cancelShipmentModel.ShipmentRequestId);

                    if (deleteShipmentRequest.ShipmentCreated)
                    {
                        var shipment = await _shipmentService.GetShipmentByIdAsync(deleteShipmentRequest.ShipmentId ?? 0);

                        await _shipmentService.DeleteShipmentAsync(shipment);
                    }

                    deleteShipmentRequests.Add(deleteShipmentRequest);
                }

                await _shipmentRequestService.DeleteShipmentRequestsAsync(deleteShipmentRequests);
            }
            else
            {
                _notificationService.ErrorNotification("Shipping provider not found.");
            }

            return RedirectToAction("Edit", "Order", new { id = shipmentRequest.OrderId });
        }

        public async Task<IActionResult> GetShipmentStatus(int shipmentRequestId)
        {
            var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByIdAsync(shipmentRequestId);

            var order = await _orderService.GetOrderByIdAsync(shipmentRequest.OrderId);

            var shippingMethodPlugin = await _shippingPluginManager.LoadPluginBySystemNameAsync(shipmentRequest.ShippingProviderSystemName);
            if (!shippingMethodPlugin.PluginDescriptor.Installed)
            {
                _notificationService.ErrorNotification("Shipping provider not found.");
            }
            else if (shippingMethodPlugin != null && shippingMethodPlugin is IShippingProviderPlugin)
            {
                var shipment = await _shipmentService.GetShipmentByIdAsync(shipmentRequest.ShipmentId ?? 0);

                UpdatedShipmentRequestModel shippingProviderResponse;
                if (shipmentRequest.ShipmentCreated && shipment != null)
                {
                    shippingProviderResponse = await shippingMethodPlugin.GetShippingRequestFromShippingProviderAsync(shipment);
                }
                else
                {
                    shippingProviderResponse = await shippingMethodPlugin.GetShippingRequestFromShippingProviderByOrderIdAsync(order.Id);
                }

                if (shippingProviderResponse == null)
                {
                    _notificationService.ErrorNotification("Shipping provider did not respond.");
                }
            }
            else
            {
                _notificationService.ErrorNotification("Shipping provider not found.");
            }

            return RedirectToAction("Edit", "Order", new { id = shipmentRequest.OrderId });
        }

        #region Send multiple shipment requests

        [HttpPost]
        public virtual async Task<IActionResult> SendMultipleShipmentRequests(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orders = new List<Order>();
            var shipmentRequests = new List<ShipmentRequest>();
            var shipments = new List<Shipment>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(await (await _orderService.GetOrdersByIdsAsync(ids))
                    .WhereAwait(HasAccessToOrderAsync).ToListAsync());
                foreach(var id in ids)
                {
                    var shipmentRequest = await _shipmentRequestService.GetShipmentRequestByOrderIdAsync(id);
                    if(shipmentRequest != null)
                    {
                        shipmentRequests.Add(shipmentRequest);
                    }

                    var shipment = await _shipmentService.GetShipmentsByOrderIdAsync(id);
                    if(shipment != null)
                    {
                        shipments.AddRange(shipment);
                    }
                }
            }

            var pickupAddress = (await _shippingAddressModelFactory.PrepareAddressesAsync())?.FirstOrDefault().Key;
            if (!pickupAddress.HasValue)
            {
                _notificationService.ErrorNotification($"Please set default package pickup (warehouse) address.");
                return RedirectToAction("List", "Order");
            }

            orders = orders.Where(a => !shipmentRequests.Any(b => b.OrderId == a.Id) && !shipments.Any(b => b.OrderId == a.Id)).ToList();
            if (!orders.Any())
            {
                _notificationService.SuccessNotification($"Selected orders have already been sent to the shipping provider.");
                return RedirectToAction("List", "Order");
            }

            var groupedOrders = orders.GroupBy(a => a.ShippingRateComputationMethodSystemName);
            foreach (var kvp in groupedOrders)
            {
                try
                {
                    var shippingMethodPlugin = await _shippingPluginManager.LoadPluginBySystemNameAsync(kvp.Key);
                    if (!shippingMethodPlugin.PluginDescriptor.Installed)
                    {
                        _notificationService.ErrorNotification("Shipping provider not found.");
                        return StatusCode(StatusCodes.Status500InternalServerError, "Required shipping method plugin not installed.");
                    }
                    else if (shippingMethodPlugin != null && shippingMethodPlugin is IShippingProviderPlugin)
                    {
                        foreach(var order in kvp.ToList())
                        {
                            var shippingProviderResponse = await shippingMethodPlugin.SendShippingRequestToShippingProviderAsync(order.Id, pickupAddress.Value);
                            if (shippingProviderResponse == null)
                            {
                                return StatusCode(StatusCodes.Status500InternalServerError, "Shipping provider did not respond");
                            }

                            foreach (var createdShipmentRequest in shippingProviderResponse)
                            {
                                var newShipmentRequests = new List<ShipmentRequest>();
                                foreach (var shipmentRequestModel in createdShipmentRequest.ShipmentRequests)
                                {
                                    var shipmentRequest = new ShipmentRequest
                                    {
                                        OrderGuid = shipmentRequestModel.OrderGuid,
                                        OrderId = shipmentRequestModel.OrderId,
                                        PackageReference = shipmentRequestModel.PackageReference,
                                        ShipmentCreated = shipmentRequestModel.ShipmentCreated,
                                        ShipmentPickupAddressId = shipmentRequestModel.ShipmentPickupAddressId,
                                        ShipmentId = shipmentRequestModel.ShipmentId,
                                        ShippingProviderSystemName = shipmentRequestModel.ShippingProviderSystemName,
                                        ShippingProvider = shipmentRequestModel.ShippingProvider,
                                        CreatedAt = DateTime.Now,
                                        UpdatedAt = DateTime.Now,
                                        NotificationSent = false,
                                        ShipmentRequestGuid = shipmentRequestModel.ShipmentRequestGuid,
                                    };

                                    order.ShippingStatus = ShippingStatus.PartiallyShipped;
                                    await _orderService.UpdateOrderAsync(order);
                                    newShipmentRequests.Add(shipmentRequest);
                                }

                                await _shipmentRequestService.InsertShipmentRequestsAsync(newShipmentRequests);
                                

                                if (!string.IsNullOrEmpty(createdShipmentRequest.ResponseMessage))
                                {
                                    await _logger.InsertLogAsync(LogLevel.Information, $"Create shipment request response message: {createdShipmentRequest.ResponseMessage}");
                                }
                            }
                        }
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, "Shipping method not found.");
                    }
                }
                catch (Exception exc)
                {
                    await _notificationService.ErrorNotificationAsync(exc);
                    await _logger.ErrorAsync(exc.Message, exc);
                }
            }

            return RedirectToAction("List", "Order");
        }

        [HttpPost]
        public virtual async Task<IActionResult> CancelMultipleShipmentRequests(string selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageOrders))
                return AccessDeniedView();

            var orders = new List<Order>();
            if (selectedIds != null)
            {
                var ids = selectedIds
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(x => Convert.ToInt32(x))
                    .ToArray();
                orders.AddRange(await (await _orderService.GetOrdersByIdsAsync(ids))
                    .WhereAwait(HasAccessToOrderAsync).ToListAsync());
            }
            var groupedOrders = orders.GroupBy(a => a.ShippingRateComputationMethodSystemName);

            var pickupAddress = (await _shippingAddressModelFactory.PrepareAddressesAsync())?.FirstOrDefault().Key;
            if (!pickupAddress.HasValue)
            {
                _notificationService.ErrorNotification($"Please set default package pickup (warehouse) address.");
                return RedirectToAction("List", "Order");
            }

            foreach (var kvp in groupedOrders)
            {
                try
                {
                    var shippingMethodPlugin = await _shippingPluginManager.LoadPluginBySystemNameAsync(kvp.Key);
                    if (!shippingMethodPlugin.PluginDescriptor.Installed)
                    {
                        _notificationService.ErrorNotification("Shipping provider not found.");
                        return StatusCode(StatusCodes.Status500InternalServerError, "Required shipping method plugin not installed.");
                    }
                    else if (shippingMethodPlugin != null && shippingMethodPlugin is IShippingProviderPlugin)
                    {
                        foreach (var order in kvp.ToList())
                        {
                            var shippingProviderResponse = await shippingMethodPlugin.CancelShippingRequestToShippingProviderAsync(order);
                            if (shippingProviderResponse == null)
                            {
                                _notificationService.ErrorNotification("Shipping provider did not respond.");
                            }

                            var deleteShipmentRequests = new List<ShipmentRequest>();
                            foreach (var cancelShipmentModel in shippingProviderResponse)
                            {
                                var deleteShipmentRequest = await _shipmentRequestService.GetShipmentRequestByIdAsync(cancelShipmentModel.ShipmentRequestId);

                                if (deleteShipmentRequest.ShipmentCreated)
                                {
                                    var shipment = await _shipmentService.GetShipmentByIdAsync(deleteShipmentRequest.ShipmentId ?? 0);

                                    await _shipmentService.DeleteShipmentAsync(shipment);
                                }

                                deleteShipmentRequests.Add(deleteShipmentRequest);
                            }

                            await _shipmentRequestService.DeleteShipmentRequestsAsync(deleteShipmentRequests);
                        }
                    }
                    else
                    {
                        return StatusCode(StatusCodes.Status500InternalServerError, "Shipping method not found.");
                    }
                }
                catch (Exception exc)
                {
                    await _notificationService.ErrorNotificationAsync(exc);
                    await _logger.ErrorAsync(exc.Message, exc);
                }
            }

            return RedirectToAction("List", "Order");
        }

        #endregion
    }
}
