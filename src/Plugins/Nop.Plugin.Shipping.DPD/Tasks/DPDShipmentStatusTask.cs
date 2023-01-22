using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.Core.Services;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Shipping.DPD.Tasks
{
    public class DPDShipmentStatusTask : IScheduleTask
    {
        #region Fields

        private readonly INotificationService _notificationService;
        private readonly IOrderService _orderService;
        private readonly IShipmentRequestService _shipmentRequestService;
        private readonly IShippingMethodPluginManager _shippingMethodPluginManager;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public DPDShipmentStatusTask(INotificationService notificationService,
            IOrderService orderService,
            IShipmentRequestService shipmentRequestService,
            IShippingMethodPluginManager shippingMethodPluginManager,
            IStoreContext storeContext,
            IWorkContext workContext)
        {
            _notificationService = notificationService;
            _orderService = orderService;
            _shipmentRequestService = shipmentRequestService;
            _shippingMethodPluginManager = shippingMethodPluginManager;
            _storeContext = storeContext;
            _workContext = workContext;
        }

        #endregion


        public async System.Threading.Tasks.Task ExecuteAsync()
        {
            var shipmentRequests = (await _shipmentRequestService.GetShipmentRequestsByPluginSystemNameAsync("Shipping.DPD")).ToList();

            foreach (var shipmentRequest in shipmentRequests)
            {
                var shippingMethodPlugin = await _shippingMethodPluginManager.LoadPluginBySystemNameAsync(shipmentRequest.ShippingProviderSystemName);

                if (!shippingMethodPlugin.PluginDescriptor.Installed)
                {
                    _notificationService.ErrorNotification("Shipping provider not found");
                    break;
                }

                var order = await _orderService.GetOrderByIdAsync(shipmentRequest.OrderId);

                if (order.ShippingStatus != ShippingStatus.Delivered)
                {
                    await shippingMethodPlugin.GetShippingRequestFromShippingProviderAsync(shipmentRequest);
                }
            }
        }
    }
}
