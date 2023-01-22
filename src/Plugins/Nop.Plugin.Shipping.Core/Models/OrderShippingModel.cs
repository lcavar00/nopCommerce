namespace Nop.Plugin.Shipping.Core.Models
{
    public class OrderShippingModel
    {
        public int OrderId { get; set; }
        public string ShippingMethodSystemName { get; set; }
        public int AddressId { get; set; }
    }
}
