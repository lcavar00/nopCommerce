namespace Nop.Plugin.Payments.WsPay.Models
{
    public class TransactionModel
    {
        public string WsPayOrderId { get; set; }
        public string Signature { get; set; }
        public string STAN { get; set; }
        public string ApprovalCode { get; set; }
        public string ShopID { get; set; }
        public string ShoppingCartId { get; set; }
        public float Amount { get; set; }
        public string ActionSuccess { get; set; }
        public string Authorized { get; set; }
        public string Completed { get; set; }
        public string Voided { get; set; }
        public string Refunded { get; set; }
        public string PaymentPlan { get; set; }
        public string Partner { get; set; }
        public string OnSite { get; set; }
        public string CreditCardName { get; set; }
        public string CreditCardNumber { get; set; }
        public string ECI { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerCountry { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerZIP { get; set; }
        public string CustomerEmail { get; set; }
    }
}

