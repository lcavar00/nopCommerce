namespace Nop.Plugin.Payments.WsPay.Models
{
    public class RatePaymentDefaults
    {
        public CardDefault[] CardDefaults { get; set; }
    }

    public class CardDefault
    {
        public decimal MinimalAmount { get; set; }
        public Card[] Cards { get; set; }
    }

    public class Card
    {
        public string Name { get; set; }
        public Bank[] Banks { get; set; }
    }

    public class Bank
    {
        public string Name { get; set; }
        public int MaxRateNumber { get; set; }
    }
}
