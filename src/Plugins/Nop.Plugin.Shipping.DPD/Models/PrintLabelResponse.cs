namespace Nop.Plugin.Shipping.DPD.Models
{
    public class PrintLabelResponse
    {
        public byte[] PdfResponse { get; set; }
        public ParcelResponse[] ParcelResponse { get; set; }
    }
}
