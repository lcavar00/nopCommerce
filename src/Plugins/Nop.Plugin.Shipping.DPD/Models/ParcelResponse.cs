namespace Nop.Plugin.Shipping.DPD.Models
{

    public class ParcelResponse
    {
        public string status { get; set; }
        public string errlog { get; set; }
        public string[] pl_number { get; set; }
    }

}
