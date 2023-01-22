using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Shipping.Core.Areas.Admin.Models;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Orders;

namespace Nop.Plugin.Shipping.Core.Areas.Admin.Factories
{
    public interface IOrderModelFactoryExtension : IOrderModelFactory
    {
        Task<OrderModelExtension> PrepareOrderModelExtensionAsync(OrderModel orderModel, Order order, bool excludeProperties = false);
    }
}