using System.Threading.Tasks;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Shipping.Core.Models;

namespace Nop.Plugin.Shipping.Core.Factories
{
    public interface IShipmentRequestModelFactory
    {
        Task<ShipmentRequestModel> PrepareShipmentRequestModelAsync(int orderId);
        Task<ShipmentRequestModel> PrepareShipmentRequestModelAsync(Order order);
    }
}