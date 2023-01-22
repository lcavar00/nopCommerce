namespace Nop.Plugin.Payments.WsPay.Models
{
    public class CallbackData
    {
        public string WsPayOrderId { get; set; }
        public int UniqueTransactionNumber { get; set; }
        public string Signature { get; set; }
        public string STAN { get; set; }
        public string ApprovalCode { get; set; }
        public string ShopID { get; set; }
        public string ShoppingCartID { get; set; }
        public int Amount { get; set; }
        public int CurrencyCode { get; set; }
        public string ActionSuccess { get; set; }
        public string Success { get; set; }
        public string Authorized { get; set; }
        public string Completed { get; set; }
        public string Voided { get; set; }
        public string Refunded { get; set; }
        public string PaymentPlan { get; set; }
        public string Partner { get; set; }
        public string OnSite { get; set; }
        public string CreditCardName { get; set; }
        public string CreditCardNumber { get; set; }
        public object ECI { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerCountry { get; set; }
        public string CustomerPhone { get; set; }
        public string CustomerZIP { get; set; }
        public string CustomerEmail { get; set; }
        public string TransactionDateTime { get; set; }
        public bool IsLessThen30DaysFromTransaction { get; set; }
        public bool CanBeCompleted { get; set; }
        public bool CanBeVoided { get; set; }
        public bool CanBeRefunded { get; set; }
        public string Token { get; set; }
        public string TokenNumber { get; set; }
        public string ExpirationDate { get; set; }
    }
}
