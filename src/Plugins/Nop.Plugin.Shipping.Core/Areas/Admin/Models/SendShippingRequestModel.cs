namespace Nop.Plugin.Shipping.Core.Areas.Admin.Models
{
    public class SendShippingRequestModel
    {
        public int OrderId { get; set; }
        public int AddressId { get; set; }
        public string ShippingMethod { get; set; }
    }
}
